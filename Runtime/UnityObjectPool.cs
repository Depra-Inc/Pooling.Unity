// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Depra.Borrow;
using Depra.Pooling.Object;
using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Depra.Pooling
{
#if ENABLE_IL2CPP
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
	public sealed class UnityObjectPool<TPooled> : IDisposable, IAsyncPool<TPooled>, IPoolHandle<TPooled>
		where TPooled : MonoBehaviour, IPooled
	{
		private readonly int _maxCapacity;
		private readonly object _sync = new();
		private readonly Factory _objectFactory;
		private readonly OverflowStrategy _overflowStrategy;
		private readonly IBorrowBuffer<PooledInstance<TPooled>> _passiveInstances;
		private readonly BorrowCircularList<PooledInstance<TPooled>> _activeInstances;

		public UnityObjectPool(TPooled prefab, PoolSettings settings)
		{
			Key = prefab.GetInstanceID();
			_maxCapacity = settings.MaxCapacity;
			_overflowStrategy = settings.OverflowStrategy;
			_objectFactory = new Factory(prefab.name, prefab);
			_activeInstances = new BorrowCircularList<PooledInstance<TPooled>>(settings.MaxCapacity, DisposeInstance);
			_passiveInstances = BorrowBuffer.Create<PooledInstance<TPooled>>(settings.BorrowStrategy, settings.Capacity, DisposeInstance);
		}

		public void Dispose()
		{
			_activeInstances.Dispose();
			_passiveInstances.Dispose();
			_objectFactory.Dispose();
		}

		public object Key { get; }

		public int CountAll
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		public int CountActive
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => CountAll - CountPassive;
		}

		public int CountPassive
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _passiveInstances.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TPooled Request() => Request(out _);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TPooled Request(out PooledInstance<TPooled> instance)
		{
			TPooled obj;
			if (CountPassive == 0)
			{
				obj = CountActive < _maxCapacity
					? CreateInstance(out instance)
					: CreateInstanceOverCapacity(out instance);
			}
			else
			{
				obj = ReusePassiveInstance(out instance);
			}

			instance.OnPoolGet();
			_objectFactory.OnEnable(Key, obj);
			_activeInstances.Add(instance);

			return obj;
		}

		public async Task<TPooled[]> RequestAsync(int count, CancellationToken cancellationToken)
		{
			if (count <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
			}

			int inStockCount;
			var lastIndex = 0;
			var results = new TPooled[count];

			lock (_sync)
			{
				inStockCount = Math.Min(count, CountPassive);
				if (inStockCount > 0)
				{
					for (var i = lastIndex; i < inStockCount; i++)
					{
						var obj = ReusePassiveInstance(out var instance);
						results[lastIndex++] = obj;
						instance.OnPoolGet();
						_objectFactory.OnEnable(Key, obj);
						_activeInstances.Add(instance);
					}
				}
			}

			var toCreateCount = count - inStockCount;
			TPooled[] created = null;
			if (toCreateCount > 0)
			{
				created = await _objectFactory.CreateAsync(toCreateCount, cancellationToken);
			}

			if (created == null)
			{
				return results;
			}

			lock (_sync)
			{
				for (var index = 0; index < toCreateCount; index++)
				{
					++CountAll;
					var obj = results[lastIndex++] = created[index];
					obj.OnPoolCreate(this);
					_objectFactory.OnEnable(Key, obj);

					var instance = new PooledInstance<TPooled>(this, obj);
					instance.OnPoolGet();
					_activeInstances.Add(instance);
				}
			}

			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release(TPooled obj)
		{
			if (obj is null)
			{
				throw new ArgumentNullException(nameof(obj), "Cannot release a null object.");
			}

			var instance = PooledInstance<TPooled>.Create(this, obj);
			instance.OnPoolSleep();
			_objectFactory.OnDisable(Key, obj);

			if (CountPassive < _maxCapacity)
			{
				_passiveInstances.Add(instance);
			}

			if (CountActive > 0)
			{
				_activeInstances.Remove(instance);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPooled CreateInstanceOverCapacity(out PooledInstance<TPooled> instance) => _overflowStrategy switch
		{
			OverflowStrategy.REQUEST => CreateInstance(out instance),
			OverflowStrategy.REUSE => ReuseActiveInstance(out instance),
			OverflowStrategy.THROW_EXCEPTION => throw new PoolOverflowed(Key),
			_ => throw new ArgumentOutOfRangeException(nameof(OverflowStrategy))
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPooled CreateInstance(out PooledInstance<TPooled> instance)
		{
			instance = new PooledInstance<TPooled>(this, _objectFactory.Create(Key));
			var obj = instance.Obj;
			obj.OnPoolCreate(this);
			++CountAll;

			return obj;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DisposeInstance(PooledInstance<TPooled> instance)
		{
			var obj = instance.Obj;
			if (obj == null)
			{
				return; // Already disposed.
			}

			obj.OnPoolSleep();
			_objectFactory.OnDisable(Key, obj);
			_objectFactory.Destroy(Key, obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPooled ReuseActiveInstance(out PooledInstance<TPooled> instance)
		{
			instance = _activeInstances.Next();
			var obj = instance.Obj;
			obj.OnPoolReuse();

			return obj;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TPooled ReusePassiveInstance(out PooledInstance<TPooled> instance)
		{
			instance = _passiveInstances.Next();
			var obj = instance.Obj;
			obj.OnPoolReuse();

			return obj;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IPooled IPool.RequestPooled() => Request();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IPool.ReleasePooled(IPooled pooled) => Release((TPooled)pooled);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IPoolHandle<TPooled>.ReturnInstanceToPool(PooledInstance<TPooled> instance) => Release(instance.Obj);

#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
		private sealed class Factory : IPooledObjectFactory<TPooled>
		{
			private const string NAME_FORMAT = "[Pool] {0}";

			private readonly TPooled _original;
			private readonly Transform _parent;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Factory(string displayName, TPooled original)
			{
				_original = original;
				_parent = new GameObject(string.Format(NAME_FORMAT, displayName))
				{
					hideFlags = HideFlags.NotEditable
				}.transform;

				_parent.SetParent(UnityPoolRoot.Instance);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				if (_parent)
				{
					UnityEngine.Object.Destroy(_parent.gameObject);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public TPooled Create(object key) =>
				UnityEngine.Object.Instantiate(_original);

			public Task<TPooled[]> CreateAsync(int count, CancellationToken cancellationToken)
			{
				var operation = UnityEngine.Object.InstantiateAsync(_original, count, new InstantiateParameters());
				return operation.AsTask(cancellationToken);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Destroy(object key, TPooled instance)
			{
				if (instance)
				{
					UnityEngine.Object.Destroy(instance.gameObject);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void OnEnable(object key, TPooled instance)
			{
				instance.transform.SetParent(null);
				instance.gameObject.SetActive(true);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void OnDisable(object key, TPooled instance)
			{
				if (instance)
				{
					instance.transform.SetParent(_parent);
					instance.gameObject.SetActive(false);
				}
			}
		}
	}
}
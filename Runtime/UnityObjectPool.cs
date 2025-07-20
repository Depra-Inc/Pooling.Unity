// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System;
using System.Runtime.CompilerServices;
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
	public sealed class UnityObjectPool<TPooled> : IDisposable, IPool<TPooled>, IPoolHandle<TPooled>
		where TPooled : MonoBehaviour, IPooled
	{
		private readonly Factory _factory;
		private readonly ObjectPool<TPooled> _objectPool;

		public UnityObjectPool(TPooled prefab, PoolSettings settings)
		{
			_factory = new Factory(prefab.name, prefab);
			_objectPool = new ObjectPool<TPooled>(_factory,
				new PoolConfiguration(
					settings.Capacity,
					settings.MaxCapacity,
					settings.BorrowStrategy,
					settings.OverflowStrategy),
				prefab.GetInstanceID());
		}

		public void Dispose()
		{
			_objectPool?.Dispose();
			_factory.Dispose();
		}

		public int CountAll
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _objectPool.CountAll;
		}

		public int CountActive
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _objectPool.CountActive;
		}

		public int CountPassive
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _objectPool.CountPassive;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TPooled Request() => _objectPool.Request();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release(TPooled obj) => _objectPool.Release(obj);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IPooled IPool.RequestPooled() => Request();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IPool.ReleasePooled(IPooled pooled) => Release((TPooled)pooled);

		void IPoolHandle<TPooled>.ReturnInstanceToPool(PooledInstance<TPooled> instance)
		{
			IPoolHandle<TPooled> handle = _objectPool;
			handle.ReturnInstanceToPool(instance);
		}

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
			TPooled IPooledObjectFactory<TPooled>.Create(object key) =>
				UnityEngine.Object.Instantiate(_original);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IPooledObjectFactory<TPooled>.Destroy(object key, TPooled instance)
			{
				if (instance)
				{
					UnityEngine.Object.Destroy(instance.gameObject);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IPooledObjectFactory<TPooled>.OnEnable(object key, TPooled instance)
			{
				instance.transform.SetParent(null);
				instance.gameObject.SetActive(true);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IPooledObjectFactory<TPooled>.OnDisable(object key, TPooled instance)
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
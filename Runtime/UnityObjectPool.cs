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
		private readonly ObjectPool<TPooled> _objectPool;

		public UnityObjectPool(TPooled prefab, PoolSettings settings) => _objectPool = new ObjectPool<TPooled>(
			new Factory(prefab.name, prefab),
			new PoolConfiguration(
				settings.Capacity,
				settings.MaxCapacity,
				settings.BorrowStrategy,
				settings.OverflowStrategy),
			prefab.GetInstanceID());

		public void Dispose() => _objectPool.Dispose();

		public int CountAll => _objectPool.CountAll;
		public int CountActive => _objectPool.CountActive;
		public int CountPassive => _objectPool.CountPassive;

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
			private readonly TPooled _original;
			private readonly Transform _parent;

			public Factory(string displayName, TPooled original)
			{
				_original = original;
				_parent = new GameObject($"[Pool] {displayName}")
				{
					hideFlags = HideFlags.NotEditable
				}.transform;
				_parent.SetParent(UnityPoolRoot.Instance);
			}

			TPooled IPooledObjectFactory<TPooled>.Create(object key) =>
				UnityEngine.Object.Instantiate(_original);

			void IPooledObjectFactory<TPooled>.Destroy(object key, TPooled instance) =>
				UnityEngine.Object.Destroy(instance.gameObject);

			void IPooledObjectFactory<TPooled>.OnEnable(object key, TPooled instance)
			{
				instance.transform.SetParent(null);
				instance.gameObject.SetActive(true);
			}

			void IPooledObjectFactory<TPooled>.OnDisable(object key, TPooled instance)
			{
				instance.transform.SetParent(_parent);
				instance.gameObject.SetActive(false);
			}
		}
	}
}
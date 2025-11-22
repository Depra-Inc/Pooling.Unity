// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System.Runtime.CompilerServices;
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
	public class PooledGameObject : MonoBehaviour, IPooled
	{
		[SerializeField] private PooledComponent[] _components;

		public virtual void OnPoolCreate(IPool pool) { }

		public virtual void OnPoolGet() { }

		public virtual void OnPoolSleep() { }

		public virtual void OnPoolReuse() { }

		public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;

			if (TryGetComponent(out Rigidbody rb))
			{
				rb.position = position;
				rb.rotation = rotation;
			}

			ResetComponents();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResetComponents()
		{
			if (_components.Length == 0)
			{
				return;
			}

			foreach (var component in _components)
			{
				component.ResetState();
			}
		}

#if UNITY_EDITOR
		[ContextMenu(nameof(Reset))]
		private void Reset()
		{
			_components = GetComponentsInChildren<PooledComponent>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}
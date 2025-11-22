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

		private Rigidbody _rigidbody;

		private void Awake() => _rigidbody = GetComponent<Rigidbody>();

		public virtual void OnPoolCreate(IPool pool) { }

		public virtual void OnPoolGet() { }

		public virtual void OnPoolSleep() { }

		public virtual void OnPoolReuse() { }

		/// <summary>
		/// Sets the position and rotation, then resets all pooled components.
		/// Use this method if you explicitly want both actions together.
		/// </summary>
		public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			transform.SetPositionAndRotation(position, rotation);
			if (_rigidbody)
			{
				_rigidbody.position = position;
				_rigidbody.rotation = rotation;
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
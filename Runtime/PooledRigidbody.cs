// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledRigidbody : PooledComponent
	{
		[SerializeField] private Rigidbody _rigidbody;

		private bool _isKinematic;
		private bool _useGravity;

		public override void OnPoolCreate()
		{
			_isKinematic = _rigidbody.isKinematic;
			_useGravity = _rigidbody.useGravity;
		}

		public override void OnPoolSleep()
		{
			ResetState();
			_rigidbody.isKinematic = _isKinematic;
			_rigidbody.useGravity = _useGravity;
		}

		public override void ResetState()
		{
			if (!_rigidbody.isKinematic)
			{
				_rigidbody.linearVelocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			if (_rigidbody != null)
			{
				return;
			}

			_rigidbody = GetComponentInChildren<Rigidbody>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}
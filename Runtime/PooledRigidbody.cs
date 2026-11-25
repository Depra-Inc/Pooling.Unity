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

		// It may have been set loose, e.g. a weapon dropped by its owner: it comes back the way it was created.
		public override void OnPoolSleep()
		{
			ResetState();
			_rigidbody.isKinematic = _isKinematic;
			_rigidbody.useGravity = _useGravity;
		}

		public override void ResetState()
		{
			// Unity complains when a kinematic body is given a velocity.
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
// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledRigidbody : PooledComponent
	{
		[SerializeField] private Rigidbody _rigidbody;

		public override void ResetState()
		{
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;
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
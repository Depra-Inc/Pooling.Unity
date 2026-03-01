// SPDX-License-Identifier: Apache-2.0
// © 2024-2026 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledParticleSystem : PooledComponent
	{
		[SerializeField] private ParticleSystem _system;
		[SerializeField] private bool _clearOnReset = true;
		[SerializeField] private bool _playOnSpawn = true;
		[SerializeField] private bool _withChildren = true;

		public override void ResetState()
		{
			if (_clearOnReset)
			{
				_system.Clear(_withChildren);
			}

			if (_playOnSpawn)
			{
				_system.Play(_withChildren);
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			if (_system != null)
			{
				return;
			}

			_system = GetComponentInChildren<ParticleSystem>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}
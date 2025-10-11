// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledTrailRenderer : PooledComponent
	{
		[SerializeField] private TrailRenderer _renderer;

		public override void ResetState() => _renderer.Clear();

#if UNITY_EDITOR
		private void Reset()
		{
			if (_renderer != null)
			{
				return;
			}

			_renderer = GetComponentInChildren<TrailRenderer>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}
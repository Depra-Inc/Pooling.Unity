// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledLineRenderer : PooledComponent
	{
		[SerializeField] private LineRenderer _renderer;

		public override void ResetState() => _renderer.positionCount = 0;

#if UNITY_EDITOR
		private void Reset()
		{
			if (_renderer != null)
			{
				return;
			}

			_renderer = GetComponentInChildren<LineRenderer>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}
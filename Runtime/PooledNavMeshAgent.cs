// SPDX-License-Identifier: Apache-2.0
// © 2026 Depra <n.melnikov@depra.org>

using UnityEngine;
using UnityEngine.AI;

namespace Depra.Pooling
{
	public sealed class PooledNavMeshAgent : PooledComponent
	{
		[SerializeField] private NavMeshAgent _agent;

		public override void OnPoolCreate() => _agent.enabled = false;

		public override void OnPoolSleep() => _agent.enabled = false;

		public override void ResetState()
		{
			if (_agent.enabled)
			{
				_agent.Warp(_agent.transform.position);
			}
			else
			{
				_agent.enabled = true;
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			if (_agent != null)
			{
				return;
			}

			_agent = GetComponentInChildren<NavMeshAgent>();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}

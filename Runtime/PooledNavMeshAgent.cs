// SPDX-License-Identifier: Apache-2.0
// © 2026 Depra <n.melnikov@depra.org>

using UnityEngine;
using UnityEngine.AI;

namespace Depra.Pooling
{
	/// <summary>
	/// Keeps the agent off while the instance is enabled where it slept,
	/// so it is created only once the instance is placed.
	/// </summary>
	public sealed class PooledNavMeshAgent : PooledComponent
	{
		[SerializeField] private NavMeshAgent _agent;

		public override void OnPoolCreate() => _agent.enabled = false;

		public override void OnPoolSleep() => _agent.enabled = false;

		public override void ResetState()
		{
			// An instance reused while still active keeps its agent, which would pull it back to where it was.
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

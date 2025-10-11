// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public abstract class PooledComponent : MonoBehaviour
	{
		public abstract void ResetState();
	}
}
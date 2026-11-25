// SPDX-License-Identifier: Apache-2.0
// © 2026 Depra <n.melnikov@depra.org>

using System;

namespace Depra.Pooling
{
	public sealed class PoolDemandConflict : Exception
	{
		public PoolDemandConflict(PooledGameObject prefab, string reason) :
			base($"Pool demand for '{prefab.name}' cannot be met: {reason}.") { }
	}
}

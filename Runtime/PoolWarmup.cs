// SPDX-License-Identifier: Apache-2.0
// © 2026 Depra <n.melnikov@depra.org>

namespace Depra.Pooling
{
	public readonly struct PoolWarmup
	{
		public readonly PooledGameObject Prefab;
		public readonly int Required;
		public readonly int Created;

		public PoolWarmup(PooledGameObject prefab, int required, int created)
		{
			Prefab = prefab;
			Required = required;
			Created = created;
		}
	}
}

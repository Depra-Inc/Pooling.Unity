// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using Depra.Pooling;

namespace Depra.Spawn
{
	public interface IPoolingBank
	{
		void RegisterPools(PoolService service);
	}
}
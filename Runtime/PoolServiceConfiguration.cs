// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;
using static Depra.Pooling.Module;

namespace Depra.Pooling
{
	[CreateAssetMenu(menuName = MENU_PATH + "Configuration", fileName = nameof(PoolServiceConfiguration), order = DEFAULT_ORDER)]
	public sealed class PoolServiceConfiguration : ScriptableObject
	{
		[SerializeField] private PrefabPoolBank[] _banks;

		public void Configure(PoolService service)
		{
			foreach (var bank in _banks)
			{
				bank.RegisterPools(service);
			}
		}
	}
}
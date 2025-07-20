// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System;
using System.Collections.Generic;
using Depra.Spawn;
using UnityEngine;
using static Depra.Pooling.Module;

namespace Depra.Pooling
{
	[CreateAssetMenu(menuName = MENU_PATH + "Bank", fileName = nameof(PrefabPoolBank), order = DEFAULT_ORDER)]
	public sealed class PrefabPoolBank : ScriptableObject, IPoolingBank
	{
		[SerializeField] private List<Entry> _entries;

		public void RegisterPools(PoolService service)
		{
			foreach (var (prefab, settings) in _entries)
			{
				var instanceId = prefab.GetInstanceID();
				if (service.IsRegistered(instanceId))
				{
					continue;
				}

				var objectPool = new UnityObjectPool<PooledGameObject>(prefab, settings);
				service.Register(instanceId, objectPool);

				if (settings.WarmupCapacity > 0)
				{
					objectPool.WarmUp(Math.Min(settings.WarmupCapacity, settings.MaxCapacity));
				}
			}
		}

		[Serializable]
		private sealed class Entry
		{
			[field: SerializeField] public PooledGameObject Prefab { get; private set; }
			[field: SerializeField] public PoolSettings Settings { get; private set; }

			public void Deconstruct(out PooledGameObject prefab, out PoolSettings settings)
			{
				prefab = Prefab;
				settings = Settings;
			}
		}
	}
}
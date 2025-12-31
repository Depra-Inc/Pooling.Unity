// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
				foreach (var (prefab, settings) in bank.Entries)
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
		}

		public Task ConfigureAsync(PoolService service, CancellationToken cancellationToken = default)
		{
			var tasks = new List<Task>();
			foreach (var bank in _banks)
			{
				foreach (var (prefab, settings) in bank.Entries)
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
						var count = Math.Min(settings.WarmupCapacity, settings.MaxCapacity);
						var task = objectPool.WarmUpAsync(count, cancellationToken);
						tasks.Add(task);
					}
				}
			}

			return Task.WhenAll(tasks);
		}
	}
}
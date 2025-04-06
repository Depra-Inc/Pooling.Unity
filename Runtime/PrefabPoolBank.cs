// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

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
				service.Register(prefab.GetInstanceID(), new UnityObjectPool<PooledGameObject>(prefab, settings));
			}
		}

		[System.Serializable]
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
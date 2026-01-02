// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using System;
using System.Collections.Generic;
using UnityEngine;
using static Depra.Pooling.Module;

namespace Depra.Pooling
{
	[CreateAssetMenu(menuName = MENU_PATH + "Bank", fileName = nameof(PrefabPoolBank), order = DEFAULT_ORDER)]
	public sealed class PrefabPoolBank : ScriptableObject
	{
		[SerializeField] private List<Entry> _entries;

		public IReadOnlyList<Entry> Entries => _entries;

		[Serializable]
		public sealed class Entry
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
// SPDX-License-Identifier: Apache-2.0
// © 2026 Depra <n.melnikov@depra.org>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Depra.Pooling
{
	/// <summary>
	/// Pools that must be warmed up while loading, declared by whoever spawns from them.
	/// Sources are asked when the warm-up runs, so a released source leaves nothing behind.
	/// </summary>
	public sealed class PoolDemand
	{
		private readonly List<Action<PoolDemand>> _sources = new();
		private readonly Dictionary<PooledGameObject, Entry> _entries = new();

		public IDisposable Add(Action<PoolDemand> source)
		{
			_sources.Add(source);
			return new Registration(this, source);
		}

		public void Require(PooledGameObject prefab, int count, PoolSettings settings)
		{
			if (!prefab || count <= 0)
			{
				return;
			}

			if (_entries.TryGetValue(prefab, out var entry))
			{
				_entries[prefab] = new Entry(entry.Count + count, entry.Settings,
					entry.Conflicting || !entry.Settings.SameAs(settings));
				return;
			}

			_entries[prefab] = new Entry(count, settings, false);
		}

		/// <param name="report">Receives a <see cref="PoolDemandConflict"/> per prefab that cannot be warmed up.</param>
		public async Task<IReadOnlyList<PoolWarmup>> WarmUpAsync(PoolService service, Action<Exception> report,
			CancellationToken token)
		{
			var warmups = new List<PoolWarmup>();
			_entries.Clear();
			foreach (var source in _sources)
			{
				source(this);
			}

			try
			{
				foreach (var (prefab, entry) in _entries)
				{
					if (entry.Conflicting)
					{
						report(new PoolDemandConflict(prefab, "it is required with different settings"));
						continue;
					}

					if (!TryGetPool(service, prefab, entry.Settings, report, out var pool))
					{
						continue;
					}

					var created = Math.Max(0, entry.Count - pool.CountAll);
					await pool.WarmUpAsync(created, token);
					warmups.Add(new PoolWarmup(prefab, entry.Count, created));
				}
			}
			finally
			{
				_entries.Clear();
			}

			return warmups;
		}

		private static bool TryGetPool(PoolService service, PooledGameObject prefab, PoolSettings settings,
			Action<Exception> report, out UnityObjectPool<PooledGameObject> pool)
		{
			var key = prefab.GetInstanceID();
			if (!service.IsRegistered(key))
			{
				pool = new UnityObjectPool<PooledGameObject>(prefab, settings);
				service.Register(key, pool);
				return true;
			}

			pool = ((IPoolService) service).Pool<PooledGameObject>(key) as UnityObjectPool<PooledGameObject>;
			if (pool == null)
			{
				report(new PoolDemandConflict(prefab, "its pool is not a UnityObjectPool"));
				return false;
			}

			if (!pool.Settings.SameAs(settings))
			{
				report(new PoolDemandConflict(prefab, "it is already pooled with different settings, e.g. by a bank"));
				return false;
			}

			return true;
		}

		private readonly struct Entry
		{
			public readonly int Count;
			public readonly bool Conflicting;
			public readonly PoolSettings Settings;

			public Entry(int count, PoolSettings settings, bool conflicting)
			{
				Count = count;
				Settings = settings;
				Conflicting = conflicting;
			}
		}

		private sealed class Registration : IDisposable
		{
			private PoolDemand _demand;
			private readonly Action<PoolDemand> _source;

			public Registration(PoolDemand demand, Action<PoolDemand> source)
			{
				_demand = demand;
				_source = source;
			}

			public void Dispose()
			{
				_demand?._sources.Remove(_source);
				_demand = null;
			}
		}
	}
}

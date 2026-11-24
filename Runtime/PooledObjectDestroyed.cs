// SPDX-License-Identifier: Apache-2.0
// © 2024-2026 Depra <n.melnikov@depra.org>

using System;
using System.Text;
using UnityEngine;

namespace Depra.Pooling
{
	public sealed class PooledObjectDestroyed : Exception
	{
		public PooledObjectDestroyed(Component pooled) : base(
			$"'{pooled.name}' was destroyed while taken from its pool, attached to '{PathOf(pooled.transform.parent)}'. " +
			"Return it to the pool before destroying the hierarchy.") { }

		private static string PathOf(Transform node)
		{
			if (!node)
			{
				return "nothing";
			}

			var path = new StringBuilder(node.name);
			for (var parent = node.parent; parent; parent = parent.parent)
			{
				path.Insert(0, '/').Insert(0, parent.name);
			}

			return path.ToString();
		}
	}
}

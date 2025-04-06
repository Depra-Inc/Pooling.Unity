// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	internal static class UnityPoolRoot
	{
		private static Transform _instance;

		public static Transform Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}

				_instance = new GameObject("[Pooling]").transform;
				_instance.hideFlags = HideFlags.NotEditable;
				_instance.gameObject.SetActive(false);
				return _instance;
			}
		}
	}
}
﻿// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;

namespace Depra.Pooling
{
	public static class UnityPoolRoot
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

				var gameObject = new GameObject("[Pooling]") { hideFlags = HideFlags.NotEditable };
				_instance = gameObject.transform;

				return _instance;
			}
		}

		public static void Destroy()
		{
			if (_instance != null)
			{
				UnityEngine.Object.Destroy(_instance.gameObject);
				_instance = null;
			}
		}
	}
}
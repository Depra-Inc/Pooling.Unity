﻿// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using UnityEngine;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Depra.Pooling
{
#if ENABLE_IL2CPP
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
	public class PooledGameObject : MonoBehaviour, IPooled
	{
		public virtual void OnPoolCreate(IPool pool) { }

		public virtual void OnPoolGet() { }

		public virtual void OnPoolSleep() { }

		public virtual void OnPoolReuse() { }
	}
}
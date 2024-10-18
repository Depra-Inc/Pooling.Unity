// SPDX-License-Identifier: Apache-2.0
// © 2024 Nikolay Melnikov <n.melnikov@depra.org>

using System;
using Depra.Borrow;
using UnityEngine;

namespace Depra.Pooling
{
	[Serializable]
	public sealed class PoolSettings
	{
		[field: SerializeField] public string Key { get; private set; }
		[field: SerializeField] public int Capacity { get; private set; } = 10;
		[field: SerializeField] public int MaxCapacity { get; private set; } = 100;
		[field: SerializeField] public BorrowStrategy BorrowStrategy { get; private set; }
	}
}
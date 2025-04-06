// SPDX-License-Identifier: Apache-2.0
// © 2024-2025 Depra <n.melnikov@depra.org>

using Depra.Borrow;
using UnityEngine;

namespace Depra.Pooling
{
	[System.Serializable]
	public sealed class PoolSettings
	{
		[field: SerializeField] public string Key { get; private set; }
		[field: SerializeField] public int Capacity { get; private set; } = 10;
		[field: SerializeField] public int MaxCapacity { get; private set; } = 100;
		[field: SerializeField] public BorrowStrategy BorrowStrategy { get; private set; }
		[field: SerializeField] public OverflowStrategy OverflowStrategy { get; private set; }
	}
}
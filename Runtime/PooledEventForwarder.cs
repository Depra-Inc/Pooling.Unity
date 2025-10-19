using UnityEngine;
using UnityEngine.Events;

namespace Depra.Pooling
{
	public sealed class PooledEventForwarder : PooledComponent
	{
		[SerializeField] private UnityEvent _onReuse;

		public override void ResetState() => _onReuse.Invoke();
	}
}
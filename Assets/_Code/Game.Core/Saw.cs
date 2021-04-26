using UnityEngine;

namespace Game.Core
{
	public class Saw : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _blade;
		[SerializeField] private float _rotationSpeed;

		private void Update()
		{
			_blade.transform.Rotate(Vector3.forward * (_rotationSpeed * Time.deltaTime));
		}
	}
}

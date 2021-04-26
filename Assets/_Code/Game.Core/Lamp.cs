using DG.Tweening;
using UnityEngine;

namespace Game.Core
{
	// I love Lamp
	public class Lamp : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _lamp;
		[SerializeField] private float _duration = 2f;
		[SerializeField] private Vector3 _angle = new Vector3(0, 0, 20f);

		private void Start()
		{
			_lamp.transform.DORotate(_angle, _duration, RotateMode.Fast).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
		}
	}
}

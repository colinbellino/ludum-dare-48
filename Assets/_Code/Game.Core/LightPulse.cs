using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Game.Core
{
	public class LightPulse : MonoBehaviour
	{
		[SerializeField] private float _duration = 3f;
		[SerializeField] private float _force = 1.5f;
		[SerializeField] private Light2D _light;

		private void Start()
		{
			DOTween
				.To(() => _light.pointLightOuterRadius, x => _light.pointLightOuterRadius = x, _light.pointLightOuterRadius * _force, _duration)
				.SetLoops(-1, LoopType.Yoyo)
				.SetEase(Ease.InOutQuad);
		}
	}
}

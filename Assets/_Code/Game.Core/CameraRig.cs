using Cinemachine;
using UnityEngine;

namespace Game.Core
{
	public class CameraRig : MonoBehaviour
	{
		[SerializeField] public Camera Camera;
		[SerializeField] public CinemachineVirtualCamera VirtualCamera;
		[SerializeField] public CinemachineConfiner Confiner;
	}
}

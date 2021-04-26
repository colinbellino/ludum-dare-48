using UnityEngine;
using Prime31;

namespace Game.Core
{
	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField] public Rigidbody2D Rigidbody;
		[SerializeField] public CharacterController2D Controller;
		[SerializeField] public Animator Animator;

		[SerializeField] public float Gravity = -25f;
		[SerializeField] public float RunSpeed = 8f;
		[SerializeField] public float GroundDamping = 20f;
		[SerializeField] public float InAirDamping = 5f;
		[SerializeField] public float JumpHeight = 3f;

		[HideInInspector] public RaycastHit2D LastControllerColliderHit;
		[HideInInspector] public Vector3 Velocity;
		[HideInInspector] public float NormalizedHorizontalSpeed = 0;
		[HideInInspector] public float DigAnimationEndTimestamp;
		[HideInInspector] public float StartDiggingTimestamp;
		[HideInInspector] public Vector3Int DigDirection;
	}
}

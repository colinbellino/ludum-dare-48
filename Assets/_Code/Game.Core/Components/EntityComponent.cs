using UnityEngine;

namespace Game.Core
{
	public enum Alliances { Foe, Ally }
	public enum Brain { Player, Helper, Shooter, Roamer }

	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField] public int BaseHealth = 3;
		[SerializeField] public float MoveSpeed = 10f;
		[SerializeField] public Alliances Alliance;
		[SerializeField] public Brain Brain;
		[SerializeField] public AudioClip FireClip;
		[SerializeField] public AudioClip DamagedClip;
		[SerializeField] public AudioClip DestroyedClip;
		[SerializeField] public Rigidbody2D Rigidbody;

		[HideInInspector] public int CurrentHealth;
	}
}

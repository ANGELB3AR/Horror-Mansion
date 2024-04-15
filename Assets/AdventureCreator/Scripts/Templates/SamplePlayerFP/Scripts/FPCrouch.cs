using UnityEngine;

namespace AC.Templates.FirstPersonPlayer
{

	[RequireComponent (typeof (AC.Player))]
	public class FPCrouch : MonoBehaviour
	{

		#region Variables

		public string inputButton = "Crouch";
		public Transform cameraParent;
		public CharacterController characterController;
		public LayerMask standLayerMask;
		private Player player;
		
		[Range (0.1f, 0.9f)] public float speedReduction = 0.6f;

		private bool isCrouching = false;
		private float normalWalkSpeed;
		private float normalRunSpeed;

		private float radius = 0.3f;
		private float standingHeight = 1.5f;
		private float cameraStandingHeight = 1.4f;

		[Range (0.1f, 0.9f)] public float heightReduction = 0.6f;
		public float transitionSpeed = 5f;
		public bool preventRunning;

		private float targetHeight, targetCameraHeight;

		#endregion


		#region UnityStandards

		private void Start ()
		{
			player = GetComponent<Player> ();

			normalWalkSpeed = player.walkSpeedScale;
			normalRunSpeed = player.runSpeedScale;

			radius = characterController.radius;
			standingHeight = characterController.height;
			if (cameraParent) cameraStandingHeight = cameraParent.localPosition.y;

			Stand (true);
		}


		private void Update ()
		{
			if (KickStarter.stateHandler.IsInGameplay () && KickStarter.playerInput.InputGetButtonDown (inputButton))
			{
				if (isCrouching)
				{
					Stand (false);
				}
				else
				{
					Crouch ();
				}
			}

			if (cameraParent)
			{
				float newCameraHeight = Mathf.Lerp (cameraParent.localPosition.y, targetCameraHeight, Time.deltaTime * transitionSpeed);
				cameraParent.localPosition = new Vector3 (cameraParent.localPosition.x, newCameraHeight, cameraParent.localPosition.z);
			}

			characterController.height = Mathf.Lerp (characterController.height, targetHeight, Time.deltaTime * transitionSpeed);
			characterController.center = new Vector3 (0f, characterController.height / 2f, 0f);
		}

		#endregion


		#region PublicFunctions

		public void Stand ()
		{
			Stand (true);
		}

		#endregion


		#region PrivateFunctions

		private void Crouch (bool force = false)
		{
			if (force || CanCrouch ())
			{
				isCrouching = true;

				player.walkSpeedScale = normalWalkSpeed * speedReduction;
				player.runSpeedScale = normalRunSpeed * speedReduction;

				targetHeight = standingHeight * heightReduction;
				targetCameraHeight = cameraStandingHeight * heightReduction;

				if (preventRunning)
				{
					player.runningLocked = PlayerMoveLock.AlwaysWalk;
				}
			}
		}


		private void Stand (bool force)
		{
			if (force || CanStand ())
			{
				isCrouching = false;

				player.walkSpeedScale = normalWalkSpeed;
				player.runSpeedScale = normalRunSpeed;

				targetHeight = standingHeight;
				targetCameraHeight = cameraStandingHeight;

				if (preventRunning)
				{
					player.runningLocked = PlayerMoveLock.Free;
				}
			}
		}


		private bool CanStand ()
		{
			Collider[] overlapColliders = Physics.OverlapCapsule (player.transform.position + (Vector3.up * (radius + 0.01f)), player.transform.position + (Vector3.up * (standingHeight - radius)), radius, standLayerMask);
			return (overlapColliders == null || overlapColliders.Length == 0);
		}


		private bool CanCrouch ()
		{
			return KickStarter.player.IsGrounded ();
		}

		#endregion


		#region GetSet

		public bool IsCrouching
		{
			get
			{
				return isCrouching;
			}
		}

		#endregion

	}

}
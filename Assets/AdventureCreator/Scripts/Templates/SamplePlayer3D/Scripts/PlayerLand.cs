using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC.Templates.SamplePlayer3D
{

	public class PlayerLand : MonoBehaviour
	{

		#region Variables
		
		[SerializeField] private Player player = null;
		[SerializeField] private float heightThreshold = 1.5f;
		[SerializeField] private string landTrigger = "Land";
		[SerializeField] private float slowMovementTime = 0.7f;
		[SerializeField] [Range (0f, 1f)] private float slowMovementFactor = 0.2f;
		private float lastGroundedHeight;
		private bool isPlayingAnim;

		#endregion


		#region UnityStandards

		private void Update ()
		{
			if (player.IsGrounded ())
			{
				float heightDiff = lastGroundedHeight - player.transform.position.y;
				if (heightDiff >= heightThreshold && !isPlayingAnim)
				{
					StartCoroutine (PlayLandAnim ());
				}
				lastGroundedHeight = player.transform.position.y;
			}
		}

		#endregion


		#region PrivateFunctions

		private IEnumerator PlayLandAnim ()
		{
			isPlayingAnim = true;

			float originalWalkSpeed = player.walkSpeedScale;
			float originalRunSpeed = player.runSpeedScale;

			player.walkSpeedScale *= slowMovementFactor;
			player.runSpeedScale = slowMovementFactor;

			player.GetAnimator ().SetTrigger (landTrigger);
			yield return new WaitForSeconds (slowMovementTime);

			player.walkSpeedScale = originalWalkSpeed;
			player.runSpeedScale = originalRunSpeed;

			isPlayingAnim = false;
		}

		#endregion

	}

}
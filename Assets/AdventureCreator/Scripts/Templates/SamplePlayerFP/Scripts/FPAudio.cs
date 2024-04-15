using System.Collections;
using UnityEngine;

namespace AC.Templates.FirstPersonPlayer
{

	public class FPAudio : MonoBehaviour
	{

		#region Variables

		[SerializeField] private AudioClip jumpSound = null;
		[SerializeField] private AudioClip landSound = null;
		[SerializeField] private AudioSource audioSource = null;
		[SerializeField] private float minLandTime = 0.5f;

		#endregion


		#region UnityStandards

		private void OnEnable () { EventManager.OnPlayerJump += OnPlayerJump; }
		private void OnDisable () { EventManager.OnPlayerJump -= OnPlayerJump; }

		#endregion


		#region CustomEvents

		private void OnPlayerJump (Player player)
		{
			if (audioSource == null)
			{
				return;
			}

			if (jumpSound)
			{
				audioSource.PlayOneShot (jumpSound);
			}

			StopAllCoroutines ();

			if (landSound)
			{
				StartCoroutine (AwaitLanding (player));
			}
		}

		#endregion


		#region PrivateFunctions

		private IEnumerator AwaitLanding (Player player)
		{
			float startTime = Time.time;
			while (!player.IsGrounded ())
			{
				yield return null;
			}
			float endTime = Time.time;

			if ((endTime - startTime) > minLandTime)
			{
				audioSource.PlayOneShot (landSound);
			}
		}

		#endregion

	}

}
/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberVisibility.cs"
 * 
 *	This script is attached to scene objects
 *	whose renderer.enabled state we wish to save.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Attach this to GameObjects whose Renderer's enabled state you wish to save.
	 * Fading in and out, due to the SpriteFader component, is also saved.
	 */
	[AddComponentMenu("Adventure Creator/Save system/Remember Visibility")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_visibility.html")]
	public class RememberVisibility : Remember
	{

		#region Variables

		[SerializeField] private Renderer rendererToSave = null;
		
		/** Whether the Renderer is enabled or not when the game begins */
		public AC_OnOff startState = AC_OnOff.On;
		/** True if child Renderers should be affected as well */
		public bool affectChildren = false;
		/** If True, the sprite's colour/alpha will be saved */
		public bool saveColour = false;

		private LimitVisibility limitVisibility;

		#endregion


		#region UnityStandards

		private void OnValidate ()
		{
			if (rendererToSave == null)
			{
				rendererToSave = GetComponent<Renderer> ();
			}
		}

		#endregion


		#region CustomEvents

		protected override void OnInitialiseScene ()
		{
			bool state = startState == AC_OnOff.On;

			if (Renderer)
			{
				limitVisibility = Renderer.GetComponent<LimitVisibility> ();
				if (limitVisibility)
				{
					limitVisibility.IsLockedOff = !state;
				}
				else
				{
					Renderer.enabled = state;
				}
			}

			if (affectChildren)
			{
				Renderer[] renderers = Renderer ? Renderer.GetComponentsInChildren<Renderer> () : GetComponentsInChildren<Renderer> ();
				foreach (Renderer _renderer in renderers)
				{
					_renderer.enabled = state;
				}
			}
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			VisibilityData visibilityData = new VisibilityData ();
			visibilityData.objectID = constantID;
			visibilityData.savePrevented = savePrevented;

			if (Renderer)
			{
				SpriteFader spriteFader = Renderer.GetComponent<SpriteFader> ();
				if (spriteFader)
				{
					visibilityData.isFading = spriteFader.IsFading;
					if (spriteFader.IsFading)
					{
						if (spriteFader.FadeType == FadeType.fadeIn)
						{
							visibilityData.isFadingIn = true;
						}
						else
						{
							visibilityData.isFadingIn = false;
						}

						visibilityData.fadeTime = spriteFader.FadeTime;
						visibilityData.fadeStartTime = spriteFader.FadeStartTime;
					}
					visibilityData.fadeAlpha = Renderer.GetComponent<SpriteRenderer> ().color.a;
				}
				else if (saveColour)
				{
					SpriteRenderer spriteRenderer = Renderer as SpriteRenderer;
					if (spriteRenderer)
					{
						Color _color = spriteRenderer.color;
						visibilityData.colourR = _color.r;
						visibilityData.colourG = _color.g;
						visibilityData.colourB = _color.b;
						visibilityData.colourA = _color.a;
					}
				}

				FollowTintMap followTintMap = Renderer.GetComponent<FollowTintMap> ();
				if (followTintMap)
				{
					visibilityData = followTintMap.SaveData (visibilityData);
				}
			}

			if (limitVisibility)
			{
				visibilityData.isOn = !limitVisibility.IsLockedOff;
			}
			else
			{
				if (Renderer)
				{
					visibilityData.isOn = Renderer.enabled;
				}
				else
				{
					Canvas canvas = GetComponent <Canvas>();
					if (canvas)
					{
						visibilityData.isOn = canvas.enabled;
					}
					else if (affectChildren)
					{
						Renderer childRenderer = GetComponentInChildren <Renderer>();
						if (childRenderer)
						{
							visibilityData.isOn = childRenderer.enabled;
						}
					}
				}
			}

			return Serializer.SaveScriptData <VisibilityData> (visibilityData);
		}
		

		public override void LoadData (string stringData)
		{
			VisibilityData data = Serializer.LoadScriptData <VisibilityData> (stringData);
			if (data == null)
			{
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			SpriteFader spriteFader = GetComponent <SpriteFader>();
			if (spriteFader)
			{
				if (data.isFading)
				{
					if (data.isFadingIn)
					{
						spriteFader.Fade (FadeType.fadeIn, data.fadeTime, data.fadeAlpha);
					}
					else
					{
						spriteFader.Fade (FadeType.fadeOut, data.fadeTime, data.fadeAlpha);
					}
				}
				else
				{
					spriteFader.EndFade ();
					spriteFader.SetAlpha (data.fadeAlpha);
				}
			}
			else
			{
				if (saveColour)
				{
					SpriteRenderer spriteRenderer = GetComponent <SpriteRenderer>();
					if (spriteRenderer)
					{
						Color _color = new Color (data.colourR, data.colourG, data.colourB, data.colourA);
						spriteRenderer.color = _color;
					}
				}
			}
			
			FollowTintMap followTintMap = GetComponent <FollowTintMap>();
			if (followTintMap)
			{
				followTintMap.LoadData (data);
			}

			if (limitVisibility)
			{
				limitVisibility.IsLockedOff = !data.isOn;
			}
			else
			{
				if (Renderer)
				{
					Renderer.enabled = data.isOn;
				}
				else
				{
					Canvas canvas = GetComponent <Canvas>();
					if (canvas)
					{
						canvas.enabled = data.isOn;
					}
				}
			}

			if (affectChildren)
			{
				Renderer[] renderers = Renderer ? Renderer.GetComponentsInChildren<Renderer>() : GetComponentsInChildren<Renderer> ();
				foreach (Renderer _renderer in renderers)
				{
					_renderer.enabled = data.isOn;
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			OnValidate ();

			CustomGUILayout.Header ("Visibility");
			CustomGUILayout.BeginVertical ();
			rendererToSave = (Renderer) CustomGUILayout.ObjectField<Renderer> ("Renderer to save:", rendererToSave, true); 
			startState = (AC_OnOff) CustomGUILayout.EnumPopup ("Visibility on start:", startState, "", "The Renderer's enabled state when the game begins");
			affectChildren = CustomGUILayout.Toggle ("Affect children?", affectChildren, "", "If True, child Renderers should be affected as well");

			if (rendererToSave && rendererToSave.GetComponent<SpriteFader> () == null && rendererToSave.GetComponent<SpriteRenderer> ())
			{
				saveColour = CustomGUILayout.Toggle ("Save colour/alpha?", saveColour, "", "If True, the sprite's colour/alpha will be saved");
			}

			CustomGUILayout.EndVertical ();

		}

		#endif

		#endregion


		#region GetSet

		private Renderer Renderer
		{
			get
			{
				if (rendererToSave == null || !Application.isPlaying)
				{
					rendererToSave = GetComponent<Renderer> ();
				}
				return rendererToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberVisibility script. */
	[System.Serializable]
	public class VisibilityData : RememberData
	{

		/** True if the Renderer is enabled */
		public bool isOn;
		/** True if the Renderer is fading */
		public bool isFading;
		/** True if the Renderer is fading in */
		public bool isFadingIn;
		/** The fade duration, if the Renderer is fading */
		public float fadeTime;
		/** The fade start time, if the Renderer is fading */
		public float fadeStartTime;
		/** The current alpha, if the Renderer is fading */
		public float fadeAlpha;

		/** If True, then the attached FollowTintMap makes use of the default TintMap defined in SceneSettings */
		public bool useDefaultTintMap;
		/** The ConstantID number of the attached FollowTintMap's tintMap object */
		public int tintMapID;
		/** The intensity value of the attached FollowTintMap component */
		public float tintIntensity;

		/** The Red channel of the sprite's colour */
		public float colourR;
		/** The Green channel of the sprite's colour */
		public float colourG;
		/** The Blue channel of the sprite's colour */
		public float colourB;
		/** The Alpha channel of the sprite's colour */
		public float colourA;

		/** The default Constructor. */
		public VisibilityData () { }

	}

}
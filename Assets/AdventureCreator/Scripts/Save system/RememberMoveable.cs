/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberMoveable.cs"
 * 
 *	This script, when attached to Moveable objects in the scene,
 *	will record appropriate positional data
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** This script is attached to Moveable, Draggable or PickUp objects you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Moveable")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_moveable.html")]
	public class RememberMoveable : Remember
	{

		#region Variables

		[SerializeField] private Moveable moveableToSave = null;
		/** Determines whether the object is on or off when the game starts */
		public AC_OnOff startState = AC_OnOff.On;
		/** The co-ordinate system to record the object's Transform values in */
		public Space saveTransformInSpace = Space.WorldSpace;
		public enum Space { WorldSpace, LocalSpace };

		#endregion


		#region UnityStandards
		
		protected override void OnInitialiseScene ()
		{
			if (KickStarter.settingsManager && isActiveAndEnabled && Moveable)
			{
				DragBase dragBase = Moveable as DragBase;
				if (dragBase)
				{
					if (startState == AC_OnOff.On)
					{
						dragBase.TurnOn ();
					}
					else
					{
						dragBase.TurnOff ();
					}
				}

				if (startState == AC_OnOff.On)
				{
					Moveable.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
				}
				else
				{
					Moveable.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
				}
			}
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Moveable == null) return string.Empty;

			MoveableData moveableData = new MoveableData ();
			
			moveableData.objectID = constantID;
			moveableData.savePrevented = savePrevented;

			Moveable_Drag moveable_Drag = Moveable as Moveable_Drag;
			if (moveable_Drag)
			{
				moveableData.isOn = moveable_Drag.IsOn ();
				moveableData.trackID = 0;
				if (moveable_Drag.dragMode == DragMode.LockToTrack && moveable_Drag.track && moveable_Drag.track.GetComponent<ConstantID>())
				{
					moveableData.trackID = moveable_Drag.track.GetComponent<ConstantID>().constantID;
				}
				moveableData.trackValue = moveable_Drag.trackValue;
				moveableData.revolutions = moveable_Drag.revolutions;
			}
			else
			{
				moveableData.isOn = (gameObject.layer == LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
			}

			Transform _transform = Moveable.Transform;
			
			switch (saveTransformInSpace)
			{
				case Space.WorldSpace:
					moveableData.LocX = _transform.position.x;
					moveableData.LocY = _transform.position.y;
					moveableData.LocZ = _transform.position.z;

					moveableData.RotX = _transform.eulerAngles.x;
					moveableData.RotY = _transform.eulerAngles.y;
					moveableData.RotZ = _transform.eulerAngles.z;
					break;

				case Space.LocalSpace:
					moveableData.LocX = _transform.localPosition.x;
					moveableData.LocY = _transform.localPosition.y;
					moveableData.LocZ = _transform.localPosition.z;

					moveableData.RotX = _transform.localEulerAngles.x;
					moveableData.RotY = _transform.localEulerAngles.y;
					moveableData.RotZ = _transform.localEulerAngles.z;
					break;

				default:
					break;
			}

			moveableData.ScaleX = _transform.localScale.x;
			moveableData.ScaleY = _transform.localScale.y;
			moveableData.ScaleZ = _transform.localScale.z;

			moveableData = Moveable.SaveData (moveableData);
			
			return Serializer.SaveScriptData <MoveableData> (moveableData);
		}
		

		public override void LoadData (string stringData)
		{
			if (Moveable == null) return;

			MoveableData data = Serializer.LoadScriptData <MoveableData> (stringData);
			if (data == null)
			{
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			DragBase dragBase = Moveable as DragBase;
			if (dragBase)
			{
				if (data.isOn)
				{
					dragBase.TurnOn ();
				}
				else
				{
					dragBase.TurnOff ();
				}
			}

			if (data.isOn)
			{
				Moveable.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				Moveable.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}

			if (Moveable.GetComponent<Player> () == null && Moveable.GetComponent<RememberNPC> () == null)
			{
				switch (saveTransformInSpace)
				{
					case Space.WorldSpace:
						Moveable.Transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
						Moveable.Transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
						break;

					case Space.LocalSpace:
						Moveable.Transform.localPosition = new Vector3 (data.LocX, data.LocY, data.LocZ);
						Moveable.Transform.localEulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
						break;

					default:
						break;
				}
			}

			Moveable.Transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);

			Moveable_Drag moveable_Drag = Moveable as Moveable_Drag;
			if (moveable_Drag)
			{
				if (moveable_Drag.IsHeld)
				{
					moveable_Drag.LetGo ();
				}
				if (moveable_Drag.dragMode == DragMode.LockToTrack)
				{
					DragTrack dragTrack = ConstantID.GetComponent<DragTrack> (data.trackID);
					if (dragTrack)
					{
						moveable_Drag.SnapToTrack (dragTrack, data.trackValue);
					}

					if (moveable_Drag.track)
					{
						moveable_Drag.trackValue = data.trackValue;
						moveable_Drag.revolutions = data.revolutions;
						moveable_Drag.StopAutoMove ();
						moveable_Drag.track.SetPositionAlong (data.trackValue, moveable_Drag);
					}
				}
			}

			Moveable.LoadData (data);
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (moveableToSave == null) moveableToSave = GetComponent<Moveable> ();

			CustomGUILayout.Header ("Moveable");
			CustomGUILayout.BeginVertical ();
			moveableToSave = (Moveable) CustomGUILayout.ObjectField<Moveable> ("Moveable to save:", moveableToSave, true);
			startState = (AC_OnOff) CustomGUILayout.EnumPopup ("Moveable state on start:", startState, "", "The interactive state of the object when the game begins");
			saveTransformInSpace = (RememberMoveable.Space) CustomGUILayout.EnumPopup ("Save Transforms in:", saveTransformInSpace, "", "The co-ordinate system to record the object's Transform values in");
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region GetSet

		private Moveable Moveable
		{
			get
			{
				if (moveableToSave == null)
				{
					moveableToSave = GetComponent <Moveable> ();
				}
				return moveableToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberMoveable script. */
	[System.Serializable]
	public class MoveableData : RememberData
	{

		/** True if the object is on */
		public bool isOn;

		/** The ConstantID value of the track it's attached to (if locked to a track) */
		public int trackID;

		/** How far along a DragTrack a Draggable object is (if locked to a track) */
		public float trackValue;
		/** If a Draggable object is locked to a DragTrack_Curved, how many revolutions it has made */
		public int revolutions;

		/** Its X position */
		public float LocX;
		/** Its Y position */
		public float LocY;
		/** Its Z position */
		public float LocZ;

		/** If True, the attached Moveable component is rotating with euler angles, not quaternions */
		public bool doEulerRotation;
		/** Its W rotation */
		public float RotW;
		/** Its X rotation */
		public float RotX;
		/** Its Y position */
		public float RotY;
		/** Its Z position */
		public float RotZ;

		/** Its X scale */
		public float ScaleX;
		/** Its Y scale */
		public float ScaleY;
		/** Its Z scale */
		public float ScaleZ;

		/** If True, the movement is occuring in world-space */
		public bool inWorldSpace;


		/** The default Constructor. */
		public MoveableData () { }
		
	}
	
}
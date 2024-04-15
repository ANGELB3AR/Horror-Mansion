/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"SetDragParameters.cs"
 * 
 *	A component used to set all of a Moveable_Drag's parameters when run as the result of either moving or dropping a draggable object.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A component used to set all of a Moveable_Drag's parameters when run as the result of either moving or dropping a draggable object. */
	[AddComponentMenu ("Adventure Creator/ActionList paramaters/Set Drag parameters")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_set_drag_parameters.html")]
	public class SetDragParameters : SetParametersBase
	{

		#region Variables

		private Moveable_Drag drag;
		private Moveable_PickUp pickUp;
		private enum PickUpAsset { OnGrab, OnDrop };
		private enum DragAsset { OnMove, OnDrop };
		[SerializeField] private DragAsset dragAsset = DragAsset.OnDrop;
		[SerializeField] private PickUpAsset pickUpAsset = PickUpAsset.OnDrop;

		#endregion


		#region UnityStandards

		protected void OnEnable ()
		{
			drag = GetComponent<Moveable_Drag> ();
			pickUp = GetComponent<Moveable_PickUp> ();
			if (drag || pickUp)
			{
				EventManager.OnBeginActionList += OnBeginActionList;
			}
			else
			{
				ACDebug.LogWarning ("The 'Set Drag parameters' component must be attached to either a Draggable or PickUp object to work.", this);
			}
		}


		protected void OnDisable ()
		{
			EventManager.OnBeginActionList -= OnBeginActionList;
		}

		#endregion


		#region CustomEvents

		protected void OnBeginActionList (ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping)
		{
			if (actionListAsset == null || actionListAsset.NumParameters < 1) return;

			if (drag && actionListAsset == drag.actionListAssetOnMove)
			{
				if (drag.actionListAssetOnDrop && dragAsset == DragAsset.OnDrop) return;

				ActionParameter moveParam = actionList.GetParameter (drag.moveParameterID);
				if (moveParam == null || moveParam.gameObject != drag.gameObject) return;
			}
			else if (drag && actionListAsset == drag.actionListAssetOnDrop)
			{
				if (drag.actionListAssetOnMove && dragAsset == DragAsset.OnMove) return;

				ActionParameter dropParam = actionList.GetParameter (drag.dropParameterID);
				if (dropParam == null || dropParam.gameObject != drag.gameObject) return;
			}
			else if (pickUp && actionListAsset == pickUp.actionListAssetOnGrab)
			{
				if (pickUp.actionListAssetOnDrop && pickUpAsset == PickUpAsset.OnDrop) return;

				ActionParameter grabParam = actionList.GetParameter (pickUp.moveParameterID);
				if (grabParam == null || grabParam.gameObject != pickUp.gameObject) return;
			}
			else if (pickUp && actionListAsset == pickUp.actionListAssetOnDrop)
			{
				if (pickUp.actionListAssetOnGrab && pickUpAsset == PickUpAsset.OnGrab) return;

				ActionParameter dropParam = actionList.GetParameter (pickUp.dropParameterID);
				if (dropParam == null || dropParam.gameObject != pickUp.gameObject) return;
			}

			AssignParameterValues (actionList);
		}

		#endregion


#if UNITY_EDITOR

		public void ShowGUI ()
		{
			Moveable_Drag drag = GetComponent<Moveable_Drag> ();
			if (drag)
			{
				if (drag.actionListAssetOnDrop && drag.actionListAssetOnMove)
				{
					dragAsset = (DragAsset) CustomGUILayout.EnumPopup ("ActionList asset:", dragAsset);
				}

				if (drag.actionListAssetOnDrop && (dragAsset == DragAsset.OnDrop || drag.actionListAssetOnMove == null)) ShowParametersGUI (drag.actionListAssetOnDrop.DefaultParameters, true);
				else if (drag.actionListAssetOnMove && (dragAsset == DragAsset.OnMove || drag.actionListAssetOnDrop == null)) ShowParametersGUI (drag.actionListAssetOnMove.DefaultParameters, true);
				return;
			}
			
			Moveable_PickUp pickUp = GetComponent<Moveable_PickUp> ();
			if (pickUp)
			{
				if (pickUp.actionListAssetOnDrop && pickUp.actionListAssetOnGrab)
				{
					pickUpAsset = (PickUpAsset) CustomGUILayout.EnumPopup ("ActionList asset:", pickUpAsset);
				}

				if (pickUp.actionListAssetOnDrop && (pickUpAsset == PickUpAsset.OnDrop || pickUp.actionListAssetOnGrab == null)) ShowParametersGUI (pickUp.actionListAssetOnDrop.DefaultParameters, true);
				else if (pickUp.actionListAssetOnGrab && (pickUpAsset == PickUpAsset.OnGrab || pickUp.actionListAssetOnDrop == null)) ShowParametersGUI (pickUp.actionListAssetOnGrab.DefaultParameters, true);
				return;
			}

			EditorGUILayout.HelpBox ("This component must be attached to either a Draggable or PickUp GameObject", MessageType.Warning);
		}

#endif

	}

}


#if UNITY_EDITOR

namespace AC
{

	[CustomEditor (typeof (SetDragParameters))]
	public class SetDragParametersEditor : Editor
	{

		private SetDragParameters _target;


		public override void OnInspectorGUI ()
		{
			_target = (SetDragParameters) target;

			_target.ShowGUI ();

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}

#endif
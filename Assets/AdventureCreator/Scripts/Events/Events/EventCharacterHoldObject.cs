using UnityEngine;

namespace AC
{

	public class EventCharacterHoldObject : EventBase
	{

		[SerializeField] private bool isPlayer;
		[SerializeField] private Char character = null;
		[SerializeField] private HoldDrop holdDrop;
		public enum HoldDrop { Hold, Drop };


		public override string[] EditorNames { get { return new string[] { "Character/Hold object", "Character/Drop object" }; } }
		protected override string EventName { get { return holdDrop == HoldDrop.Hold ? "OnCharacterHoldObject" : "OnCharacterDropObject"; } }


		protected override string ConditionHelp
		{
			get
			{
				string ending = isPlayer ? "the Player" : "a character";
				if (!isPlayer && character) ending = "character '" + character.name + "'";
				return "Whenever an object is " + ((holdDrop == HoldDrop.Hold) ? "held" : "dropped") + " by " + ending + "."; 
			}
		}


		public EventCharacterHoldObject (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, bool _isPlayer, Char _character, HoldDrop _holdDrop)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			isPlayer = _isPlayer;
			character = _character;
			holdDrop = _holdDrop;
		}


		public EventCharacterHoldObject () {}


		public override void Register ()
		{
			EventManager.OnCharacterHoldObject += OnCharacterHoldObject;
			EventManager.OnCharacterDropObject += OnCharacterDropObject;
		}


		public override void Unregister ()
		{
			EventManager.OnCharacterHoldObject -= OnCharacterHoldObject;
			EventManager.OnCharacterDropObject -= OnCharacterDropObject;
		}


		private void OnCharacterHoldObject (Char _character, GameObject heldObject, Hand hand)
		{
			if (holdDrop == HoldDrop.Hold)
			{
				if ((isPlayer && _character.IsActivePlayer ()) ||
					(!isPlayer && character == null) ||
					(!isPlayer && character == _character))
				{
					Run (new object[] { _character.gameObject, heldObject, (hand == Hand.Right) });
				}
			}
		}


		private void OnCharacterDropObject (Char _character, GameObject heldObject, Hand hand)
		{
			if (holdDrop == HoldDrop.Drop)
			{
				if ((isPlayer && _character.IsActivePlayer ()) ||
					(!isPlayer && character == null) ||
					(!isPlayer && character == _character))
				{
					Run (new object[] { _character.gameObject, heldObject, (hand == Hand.Right) });
				}
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Character"),
				new ParameterReference (ParameterType.GameObject, "Held object"),
				new ParameterReference (ParameterType.Boolean, "In right hand?"),
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			holdDrop = (HoldDrop) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			isPlayer = CustomGUILayout.Toggle ("Is Player?", isPlayer);
			if (!isAssetFile && !isPlayer)
			{
				character = (Char) CustomGUILayout.ObjectField<Char> ("Character:", character, true);
			}
		}

#endif

	}

}
using System;
using UnityEngine;

namespace AC
{

	public class EventSaveSave : EventBase
	{

		[SerializeField] private FileAccessState fileAccessState;


		public override string[] EditorNames { get { return new string[] { "Save/Save/Before", "Save/Save/After", "Save/Save/Fail" }; } }


		protected override string EventName 
		{
			get
			{
				switch (fileAccessState)
				{
					case FileAccessState.Before:
						return "OnBeforeSaving";

					case FileAccessState.After:
						return "OnFinishSaving";

					case FileAccessState.Fail:
						return "OnFailSaving";

					default:
						return string.Empty;
				}
			}
		}

	
		protected override string ConditionHelp
		{
			get
			{
				switch (fileAccessState)
				{
					case FileAccessState.Before:
						return "Before a save-game file is saved."; 

					case FileAccessState.After:
						return "After a save-game file is saved.";

					case FileAccessState.Fail:
						return "After a save-game file fails to save.";

					default:
						return string.Empty;
				}
			}
		}


		public EventSaveSave (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, FileAccessState _fileAccessState)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			fileAccessState = _fileAccessState;
		}


		public EventSaveSave () {}


		public override void Register ()
		{
			EventManager.OnBeforeSaving += OnBeforeSaving;
			EventManager.OnFinishSaving += OnFinishSaving;
			EventManager.OnFailSaving += OnFailSaving;
		}


		public override void Unregister ()
		{
			EventManager.OnBeforeSaving -= OnBeforeSaving;
			EventManager.OnFinishSaving -= OnFinishSaving;
			EventManager.OnFailSaving -= OnFailSaving;
		}


		private void OnBeforeSaving (int saveID)
		{
			if (fileAccessState == FileAccessState.Before)
			{
				Run (new object[] { saveID });
			}
		}

		
		private void OnFinishSaving (SaveFile saveFile)
		{
			if (fileAccessState == FileAccessState.After)
			{
				Run (new object[] { saveFile.saveID });
			}
		}


		private void OnFailSaving (int saveID)
		{
			if (fileAccessState == FileAccessState.Fail)
			{
				Run (new object[] { saveID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Integer, "Save ID"),
			};
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return false; }


		public override void AssignVariant (int variantIndex)
		{
			fileAccessState = (FileAccessState) variantIndex;
		}

#endif

	}

}
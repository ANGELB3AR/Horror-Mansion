using UnityEngine;

namespace AC
{

	public class EventSaveLoad : EventBase
	{

		[SerializeField] private FileAccessState fileAccessState;


		public override string[] EditorNames { get { return new string[] { "Save/Load/Before", "Save/Load/After", "Save/Load/Fail" }; } }
		

		protected override string EventName 
		{
			get
			{
				switch (fileAccessState)
				{
					case FileAccessState.Before:
						return "OnBeforeLoading";

					case FileAccessState.After:
						return "OnFinishLoading";

					case FileAccessState.Fail:
						return "OnFailLoading";

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


		public EventSaveLoad (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, FileAccessState _fileAccessState)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			fileAccessState = _fileAccessState;
		}


		public EventSaveLoad () {}


		public override void Register ()
		{
			EventManager.OnBeforeLoading += OnBeforeLoading;
			EventManager.OnFinishLoading += OnFinishLoading;
			EventManager.OnFailLoading += OnFailLoading;
		}


		public override void Unregister ()
		{
			EventManager.OnBeforeLoading -= OnBeforeLoading;
			EventManager.OnFinishLoading -= OnFinishLoading;
			EventManager.OnFailLoading -= OnFailLoading;
		}


		private void OnBeforeLoading (SaveFile saveFile)
		{
			if (fileAccessState == FileAccessState.Before)
			{
				Run (new object[] { saveFile.saveID });
			}
		}

		
		private void OnFinishLoading (int saveID)
		{
			if (fileAccessState == FileAccessState.After)
			{
				Run (new object[] { saveID });
			}
		}


		private void OnFailLoading (int saveID)
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
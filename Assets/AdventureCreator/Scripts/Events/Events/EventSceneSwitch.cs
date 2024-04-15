using UnityEngine;

namespace AC
{

	public class EventSceneSwitch : EventBase
	{

		[SerializeField] private BeforeAfter beforeAfter;
		public enum BeforeAfter { Before, After };
		[SerializeField] private DueToLoadingSave dueToLoadingSave;
		public enum DueToLoadingSave { No, Yes, Either };


		public override string[] EditorNames { get { return new string[] { "Scene/Change/Before", "Scene/Change/After" }; } }
		protected override string EventName { get { return beforeAfter == BeforeAfter.Before ? "OnBeforeChangeScene" : "OnAfterChangeScene"; } }
		
		
		protected override string ConditionHelp { get { return beforeAfter.ToString () + " a change in the active scene" + (dueToLoadingSave == DueToLoadingSave.Either ? "." : (", due to " + (dueToLoadingSave == DueToLoadingSave.Yes ? "loading a save-file." : "gameplay."))); } }


		public EventSceneSwitch (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, BeforeAfter _beforeAfter, DueToLoadingSave _dueToLoadingSave)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			beforeAfter = _beforeAfter;
			dueToLoadingSave = _dueToLoadingSave;
		}


		public EventSceneSwitch () {}


		public override void Register ()
		{
			EventManager.OnBeforeChangeScene += OnBeforeChangeScene;
			EventManager.OnAfterChangeScene += OnAfterChangeScene;
		}


		public override void Unregister ()
		{
			EventManager.OnBeforeChangeScene -= OnBeforeChangeScene;
			EventManager.OnAfterChangeScene -= OnAfterChangeScene;
		}


		private void OnBeforeChangeScene (string nextSceneName)
		{
			if (beforeAfter == BeforeAfter.Before)
			{
				LoadingGame loadingGame = KickStarter.saveSystem.loadingGame;
				if (dueToLoadingSave == DueToLoadingSave.Yes && (loadingGame == LoadingGame.No || loadingGame == LoadingGame.JustSwitchingPlayer)) return;
				if (dueToLoadingSave == DueToLoadingSave.No && (loadingGame == LoadingGame.InNewScene || loadingGame == LoadingGame.InSameScene)) return;

				Run (new object[] { nextSceneName });
			}
		}


		private void OnAfterChangeScene (LoadingGame loadingGame)
		{
			if (beforeAfter == BeforeAfter.After)
			{
				if (dueToLoadingSave == DueToLoadingSave.Yes && (loadingGame == LoadingGame.No || loadingGame == LoadingGame.JustSwitchingPlayer)) return;
				if (dueToLoadingSave == DueToLoadingSave.No && (loadingGame == LoadingGame.InNewScene || loadingGame == LoadingGame.InSameScene)) return;

				Run (new object[] { KickStarter.sceneSettings.gameObject.scene.name });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.String, "Scene name"),
			};
		}


#if UNITY_EDITOR

		protected override void ShowConditionGUI (bool isAssetFile)
		{
			dueToLoadingSave = (DueToLoadingSave) CustomGUILayout.EnumPopup ("Due to loading save-file:", dueToLoadingSave);
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }
		

		public override void AssignVariant (int variantIndex)
		{
			beforeAfter = (BeforeAfter) variantIndex;
		}

#endif

	}

}
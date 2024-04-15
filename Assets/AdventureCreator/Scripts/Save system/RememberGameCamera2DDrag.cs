using UnityEngine;

namespace AC
{


	[AddComponentMenu ("Adventure Creator/Save system/Remember GameCamera 2D Drag")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_game_camera2_d_drag.html")]
	public class RememberGameCamera2DDrag : Remember
	{

		#region Variables

		[SerializeField] private GameCamera2DDrag dragCamera = null;

		#endregion


		#region UnityStandards

		private void OnValidate ()
		{
			if (dragCamera == null) dragCamera = GetComponent<GameCamera2DDrag> ();
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (dragCamera == null) return string.Empty;

			GameCamera2DDragData data = new GameCamera2DDragData ();
			data.objectID = constantID;
			data.savePrevented = savePrevented;

			data.positionX = dragCamera.GetPosition ().x;
			data.positionY = dragCamera.GetPosition ().y;
			data.deltaPositionX = dragCamera.DeltaPosition.x;
			data.deltaPositionY = dragCamera.DeltaPosition.y;

			return Serializer.SaveScriptData<GameCamera2DDragData> (data);
		}


		public override void LoadData (string stringData)
		{
			if (dragCamera == null) return;

			GameCamera2DDragData data = Serializer.LoadScriptData<GameCamera2DDragData> (stringData);
			if (data == null) return;
			SavePrevented = data.savePrevented; if (savePrevented) return;

			dragCamera.SetPosition (new Vector2 (data.positionX, data.positionY));
			dragCamera.DeltaPosition = new Vector2 (data.deltaPositionX, data.deltaPositionY);
		}

		#endregion


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			CustomGUILayout.Header ("Camera");
			CustomGUILayout.BeginVertical ();
			dragCamera = (GameCamera2DDrag) UnityEditor.EditorGUILayout.ObjectField ("Drag camera:", dragCamera, typeof (GameCamera2DDrag), true);
			CustomGUILayout.EndVertical ();
		}

		#endif

	}


	[System.Serializable]
	public class GameCamera2DDragData : RememberData
	{

		public float positionX;
		public float positionY;
		public float deltaPositionX;
		public float deltaPositionY;

		public GameCamera2DDragData () { }

	}

}
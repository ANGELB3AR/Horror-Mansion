/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberMaterial.cs"
 * 
 *	This script is attached to renderers with materials we wish to record changes in.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if AddressableIsPresent
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace AC
{

	/** Attach this to Renderer components with Materials you wish to record changes in. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Material")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_material.html")]
	public class RememberMaterial : Remember
	{

		#region Variables

		[SerializeField] private Renderer rendererToSave = null;

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Renderer == null) return string.Empty;

			MaterialData materialData = new MaterialData ();
			materialData.objectID = constantID;
			materialData.savePrevented = savePrevented;

			List<string> materialIDs = new List<string> ();
			Material[] mats = Renderer.materials;

			foreach (Material material in mats)
			{
				materialIDs.Add (AssetLoader.GetAssetInstanceID (material));
			}
			materialData._materialIDs = ArrayToString<string> (materialIDs.ToArray ());

			return Serializer.SaveScriptData <MaterialData> (materialData);
		}
		

		public override IEnumerator LoadDataCo (string stringData)
		{
			if (Renderer == null) yield break;

			MaterialData data = Serializer.LoadScriptData <MaterialData> (stringData);
			if (data == null) yield break;
			SavePrevented = data.savePrevented; if (savePrevented) yield break;

			#if AddressableIsPresent

			if (KickStarter.settingsManager.saveAssetReferencesWithAddressables)
			{
				var loadDataCoroutine = LoadDataFromAddressable (data);
				while (loadDataCoroutine.MoveNext ())
				{
					yield return loadDataCoroutine.Current;
				}
				
				yield break;
			}

			#endif

			LoadDataFromResources (data);
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (rendererToSave == null) rendererToSave = GetComponent<Renderer> ();

			CustomGUILayout.Header ("Material");
			CustomGUILayout.BeginVertical ();
			rendererToSave = (Renderer) CustomGUILayout.ObjectField<Renderer> ("Renderer to save:", rendererToSave, true);
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region PrivateFunctions

		#if AddressableIsPresent

		private IEnumerator LoadDataFromAddressable (MaterialData data)
		{
			Material[] mats = Renderer.materials;
			string[] materialIDs = StringToStringArray (data._materialIDs);

			int count = Mathf.Min (materialIDs.Length, mats.Length);
			for (int i = 0; i < count; i++)
			{
				if (string.IsNullOrEmpty (materialIDs[i])) continue;
				AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material> (materialIDs[i]);
				yield return handle;
				if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
				{
					mats[i] = handle.Result;
				}
				Addressables.Release (handle);
			}

			Renderer.materials = mats;
		}

		#endif


		private void LoadDataFromResources (MaterialData data)
		{
			Material[] mats = Renderer.materials;
			string[] materialIDs = StringToStringArray (data._materialIDs);

			for (int i = 0; i < materialIDs.Length; i++)
			{
				if (i < mats.Length)
				{
					Material _material = AssetLoader.RetrieveAsset (mats[i], materialIDs[i]);
					if (_material)
					{
						mats[i] = _material;
					}
				}
			}

			Renderer.materials = mats;
		}

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


	/** A data container used by the RememberMaterial script. */
	[System.Serializable]
	public class MaterialData : RememberData
	{

		/** The unique identifier of each Material in the Renderer */
		public string _materialIDs;

		/** The default Constructor. */
		public MaterialData () { }

	}
	
}
/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberContainer.cs"
 * 
 *	This script is attached to container objects in the scene
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/** This script is attached to Container objects in the scene you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Container")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_container.html")]
	public class RememberContainer : Remember
	{

		#region Variables

		[SerializeField] private Container containerToSave;

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Container == null) return string.Empty;

			ContainerData containerData = new ContainerData ();
			containerData.objectID = constantID;
			containerData.savePrevented = savePrevented;
			
			containerData.collectionData = Container.InvCollection.GetSaveData ();

			containerData._linkedIDs = string.Empty; // Now deprecated
			containerData._counts = string.Empty; // Now deprecated
			containerData._IDs = string.Empty; // Now deprecated
			
			return Serializer.SaveScriptData <ContainerData> (containerData);
		}
		

		public override void LoadData (string stringData)
		{
			if (Container == null) return;

			ContainerData data = Serializer.LoadScriptData <ContainerData> (stringData);
			if (data == null) return;
			SavePrevented = data.savePrevented; if (savePrevented) return;

			if (!string.IsNullOrEmpty (data.collectionData))
			{
				Container.InvCollection = InvCollection.LoadData (data.collectionData, Container);
				return;
			}

			Container.InvCollection = new InvCollection (Container);

			if (!string.IsNullOrEmpty (data._linkedIDs))
			{
				Container.InvCollection.DeleteAll ();

				int[] linkedIDs = StringToIntArray (data._linkedIDs);
				int[] counts = StringToIntArray (data._counts);
				
				if (linkedIDs != null)
				{
					for (int i=0; i<linkedIDs.Length; i++)
					{
						InvInstance invInstance = new InvInstance (linkedIDs[i], counts[i]);
						Container.InvCollection.Add (invInstance);
					}
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (containerToSave == null) containerToSave = GetComponent<Container> ();

			CustomGUILayout.Header ("Container");
			CustomGUILayout.BeginVertical ();
			containerToSave = (Container) CustomGUILayout.ObjectField<Container> ("Container to save:", containerToSave, true);
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region GetSet

		private Container Container
		{
			get
			{
				if (containerToSave == null)
				{
					containerToSave = GetComponent <Container>();
				}
				return containerToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberContainer script. */
	[System.Serializable]
	public class ContainerData : RememberData
	{

		/** (Deprecated) */
		public string _linkedIDs;
		/** (Deprecated) */
		public string _counts;
		/** (Deprecated) */
		public string _IDs;
		/** The contents of the container's InvCollection. */
		public string collectionData;

		/** The default Constructor. */
		public ContainerData () { }

	}
	
}
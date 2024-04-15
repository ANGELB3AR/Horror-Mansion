using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	[Serializable]
	public class SceneManagerPrefabData
	{

		#region Variables

		[SerializeField] private string category;
		[SerializeField] private string label;
		[SerializeField] private string description;
		[SerializeField] private Texture2D icon;
		[SerializeField] private GameObject prefab;

		#endregion


		#region Constructors

		public SceneManagerPrefabData (string _category, string _label, string _description, Texture2D _icon, GameObject _prefab)
		{
			category = _category;
			label = _label;
			description = _description;
			icon = _icon;
			prefab = _prefab;
		}

		#endregion


		#region PublicFunctions

		public bool IsValid ()
		{
			if (string.IsNullOrEmpty (category) ||
				string.IsNullOrEmpty (label) ||
				icon == null ||
				prefab == null)
			{
				return false; 
			}
			return true;
		}

		#endregion


		#region GetSet

		public string Category { get { return category; }} 
		public string Label { get { return label; }} 
		public string Description { get { return description; }} 
		public Texture2D Icon { get { return icon; }} 
		public GameObject Prefab { get { return prefab; }} 

		#endregion

	}

}	
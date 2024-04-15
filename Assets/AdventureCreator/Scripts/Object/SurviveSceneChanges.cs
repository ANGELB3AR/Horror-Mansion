/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"SurviveSceneChanges.cs"
 * 
 *	Attaching this script to an object will move it to the DontDestroyOnLoad scene in the Hierarchy causing it to survive scene changes.
 * 
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/** Attaching this script to an object will move it to the DontDestroyOnLoad scene in the Hierarchy causing it to survive scene changes. */
	[AddComponentMenu("Adventure Creator/Misc/Survive scene changes")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_survive_scene_changes.html")]
	public class SurviveSceneChanges : MonoBehaviour
	{
		
		private void Start ()
		{
			DontDestroyOnLoad (gameObject);
		}

	}

}
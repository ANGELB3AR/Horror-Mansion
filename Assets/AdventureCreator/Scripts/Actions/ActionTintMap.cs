/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionTintMap.cs"
 * 
 *	This action changes which TintMap a FollowTintMap uses, and the intensity of the effect
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionTintMap : Action
	{

		public bool isPlayer;
		public int playerID = -1;
		public int playerParameterID = -1;

		public FollowTintMap followTintMap;
		public int followTintMapConstantID = 0;
		public int followTintMapParameterID = -1;
		protected FollowTintMap runtimeFollowTintMap;

		public TintMapMethod tintMapMethod = TintMapMethod.ChangeTintMap;

		public float newIntensity = 1f;
		public bool isInstant = true;
		public float timeToChange = 0f;

		public bool followDefault = false;
		public TintMap newTintMap;
		public int newTintMapConstantID = 0;
		public int newTintMapParameterID = -1;
		protected TintMap runtimeNewTintMap;


		public override ActionCategory Category { get { return ActionCategory.Object; }}
		public override string Title { get { return "Change Tint map"; }}
		public override string Description { get { return "Changes which Tint map a Follow Tint Map component uses, and the intensity of the effect."; }}
		

		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				Player player = AssignPlayer (playerID, parameters, playerParameterID);
				if (player != null && player.spriteChild != null)
				{
					runtimeFollowTintMap = player.spriteChild.GetComponent <FollowTintMap>();
				}
			}
			else
			{
				runtimeFollowTintMap = AssignFile <FollowTintMap> (parameters, followTintMapParameterID, followTintMapConstantID, followTintMap);
			}

			if (tintMapMethod == TintMapMethod.ChangeTintMap && !followDefault)
			{
				runtimeNewTintMap = AssignFile <TintMap> (parameters, newTintMapParameterID, newTintMapConstantID, newTintMap);
			}

			if (timeToChange < 0f)
			{
				timeToChange = 0f;
			}
		}


		public override float Run ()
		{
			if (runtimeFollowTintMap == null)
			{
				if (isPlayer)
				{
					LogWarning ("Could not find a FollowTintMap component on the Player - be sure to place one on the sprite child.");
				}

				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (tintMapMethod == TintMapMethod.ChangeIntensity)
				{
					if (isInstant || timeToChange <= 0f)
					{
						runtimeFollowTintMap.SetIntensity (newIntensity);
					}
					else
					{
						runtimeFollowTintMap.SetIntensity (newIntensity, timeToChange);

						if (willWait)
						{
							return timeToChange;
						}
					}
				}
				else if (tintMapMethod == TintMapMethod.ChangeTintMap)
				{
					if (followDefault)
					{
						runtimeFollowTintMap.useDefaultTintMap = true;
						runtimeFollowTintMap.ResetTintMap ();
					}
					else
					{
						if (runtimeNewTintMap)
						{
							runtimeFollowTintMap.useDefaultTintMap = false;
							runtimeFollowTintMap.tintMap = runtimeNewTintMap;
							runtimeFollowTintMap.ResetTintMap ();
						}
						else
						{
							LogWarning ("Could not change " + runtimeFollowTintMap.gameObject.name + " - no alternative provided!");
						}
					}
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			tintMapMethod = (TintMapMethod) EditorGUILayout.EnumPopup ("Change to make:", tintMapMethod);

			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (isPlayer)
			{
				PlayerField (ref playerID, parameters, ref playerParameterID);
			}
			else
			{
				ComponentField ("FollowTintMap:", ref followTintMap, ref followTintMapConstantID, parameters, ref followTintMapParameterID);
			}

			if (tintMapMethod == TintMapMethod.ChangeTintMap)
			{
				followDefault = EditorGUILayout.Toggle ("Follow scene default?", followDefault);
				if (!followDefault)
				{
					ComponentField ("New TintMap:", ref newTintMap, ref newTintMapConstantID, parameters, ref newTintMapParameterID);
				}
			}
			else if (tintMapMethod == TintMapMethod.ChangeIntensity)
			{
				newIntensity = EditorGUILayout.Slider ("New intensity:", newIntensity, 0f, 1f);
				isInstant = EditorGUILayout.Toggle ("Change instantly?", isInstant);
				if (!isInstant)
				{
					timeToChange = EditorGUILayout.FloatField ("Time to change (s):", timeToChange);
					if (timeToChange > 0f)
					{
						willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
					}
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				FollowTintMap obToUpdate = followTintMap;
				if (isPlayer && (KickStarter.settingsManager == null || KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow))
				{
					if (!fromAssetFile)
					{
						Player _player = UnityVersionHandler.FindObjectOfType<Player> ();
						if (_player)
						{
							obToUpdate = _player.GetComponentInChildren <FollowTintMap>();
						}
					}

					if (obToUpdate == null && KickStarter.settingsManager != null)
					{
						Player player = KickStarter.settingsManager.GetDefaultPlayer ();
						obToUpdate = player.GetComponentInChildren <FollowTintMap>();
					}
				}

				AddSaveScript <RememberVisibility> (obToUpdate);
			}
			newTintMapConstantID = AssignConstantID<TintMap> (newTintMap, newTintMapConstantID, newTintMapParameterID);
		}


		public override string SetLabel ()
		{
			string labelAdd = tintMapMethod.ToString ();

			if (isPlayer)
			{
				labelAdd += " - Player";
			}
			else if (followTintMap != null && followTintMap.gameObject)
			{
				labelAdd += " - " + followTintMap.gameObject.name;
			}

			return labelAdd;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (followTintMapParameterID < 0)
			{
				if (followTintMap && followTintMap.gameObject == gameObject) return true;
				if (followTintMapConstantID == id && id != 0) return true;
			}
			if (newTintMapParameterID < 0 && tintMapMethod == TintMapMethod.ChangeTintMap)
			{
				if (newTintMap && newTintMap.gameObject == gameObject) return true;
				if (newTintMapConstantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (!isPlayer) return false;
			if (_playerID < 0) return true;
			if (playerID < 0 && playerParameterID < 0) return true;
			return (playerParameterID < 0 && playerID == _playerID);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Object: Change Tint map' Action, set to change the active TintMap</summary>
		 * <param name = "followTintMap">The FollowTintMap to update</param>
		 * <param name = "newTintMap">The new TintMap to assign to the FollowTintMap</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTintMap CreateNew_ChangeTintMap (FollowTintMap followTintMap, TintMap newTintMap)
		{
			ActionTintMap newAction = CreateNew<ActionTintMap> ();
			newAction.tintMapMethod = TintMapMethod.ChangeTintMap;
			newAction.followTintMap = followTintMap;
			newAction.TryAssignConstantID (newAction.followTintMap, ref newAction.followTintMapConstantID);
			newAction.newTintMap = newTintMap;
			newAction.TryAssignConstantID (newAction.newTintMap, ref newAction.newTintMapConstantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Object: Change Tint map' Action, set to change a FollowTintMap's intensity</summary>
		 * <param name = "followTintMap">The FollowTintMap to update</param>
		 * <param name = "newIntensity">The FollowTintMap's new target intensity</param>
		 * <param name = "transitionTime">The duration, in seconds, of the transition to the new intensity</param>
		 * <param name = "waitUntilFinish">If True, then the Action will wait until the transition is complete</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTintMap CreateNew_ChangeIntensity (FollowTintMap followTintMap, float newIntensity, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionTintMap newAction = CreateNew<ActionTintMap> ();
			newAction.tintMapMethod = TintMapMethod.ChangeIntensity;
			newAction.followTintMap = followTintMap;
			newAction.TryAssignConstantID (newAction.followTintMap, ref newAction.followTintMapConstantID);
			newAction.newIntensity = newIntensity;
			newAction.timeToChange = transitionTime;
			newAction.isInstant = (transitionTime <= 0f);
			newAction.willWait = waitUntilFinish;
			return newAction;
		}
		
	}
	
}
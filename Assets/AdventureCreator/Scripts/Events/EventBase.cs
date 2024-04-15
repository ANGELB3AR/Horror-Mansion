/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"EventBase.cs"
 * 
 *	A base class for an Editor-set Event triggered by the EventManager.
 * 
 */

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A base class for an Editor-set Event triggered by the EventManager. */
	[Serializable]
	public abstract class EventBase
	{

		#region Variables

		[SerializeField] protected string label;
		[SerializeField] protected int id;
		[SerializeField] protected ActionListAsset actionListAsset;
		[SerializeField] protected int[] parameterIDs;
		private ParameterReference[] parameterReferences;

		#endregion


		#region PublicFunctions

		public abstract void Register ();
		public abstract void Unregister ();

		#endregion


		#region ProtectedFunctions

		protected void Run ()
		{
			Run (new object[0] { });
		}


		protected void Run (object[] args)
		{
			if (actionListAsset == null) return;

			for (int i = 0; i < ParameterIDs.Length; i++)
			{
				ActionParameter parameter = actionListAsset.GetParameter (ParameterIDs[i]);
				ParameterReference parameterReference = ParameterReferences[i];

				if (parameter != null && parameter.parameterType == parameterReference.Type)
				{
					object arg = args[i];

					switch (parameter.parameterType)
					{
						case ParameterType.String:
							if (arg is string)
							{
								string stringValue = (string) arg;
								parameter.SetValue (stringValue);
							}
							break;

						case ParameterType.Integer:
						case ParameterType.InventoryItem:
						case ParameterType.Document:
						case ParameterType.Objective:
						case ParameterType.GlobalVariable:
						case ParameterType.LocalVariable:
							if (arg is int)
							{
								int intVal = (int) arg;
								parameter.SetValue (intVal);
							}
							break;

						case ParameterType.Float:
							if (arg is float)
							{
								float floatVal = (float) arg;
								parameter.SetValue (floatVal);
							}
							break;

						case ParameterType.ComponentVariable:
							if (arg is GVar)
							{
								GVar gVar = (GVar) arg;
								Variables[] variables = UnityVersionHandler.FindObjectsOfType<Variables> ();
								foreach (Variables _variables in variables)
								{
									if (_variables.vars.Contains (gVar))
									{
										parameter.SetValue (_variables, gVar.id);
									}
								}
							}
							break;

						case ParameterType.GameObject:
							if (arg is GameObject)
							{
								GameObject gameObjectVal = (GameObject) arg;
								parameter.SetValue (gameObjectVal);
							}
							break;

						default:
							break;
					}
				}
			}

			actionListAsset.Interact ();
		}


		protected virtual ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[0];
		}

		#endregion


		#region GetSet

		public int ID { get { return id; } }
		public string Label { get { return !string.IsNullOrEmpty (label) ? label : EventName; } }

		public abstract string[] EditorNames { get; }
		protected abstract string EventName { get; }
		protected abstract string ConditionHelp { get; }


		private ParameterReference[] ParameterReferences
		{
			get
			{
				if (parameterReferences == null)
				{
					parameterReferences = GetParameterReferences ();
				}
				return parameterReferences;
			}
		}


		private int[] ParameterIDs
		{
			get
			{
				if (parameterIDs == null)
				{
					parameterIDs = new int[ParameterReferences.Length];
					for (int i = 0; i < parameterIDs.Length; i++)
					{
						parameterIDs[i] = -1;
					}
				}
				else if (parameterIDs.Length != ParameterReferences.Length)
				{
					if (parameterIDs.Length < ParameterReferences.Length)
					{
						// Increase
						int[] backupParameters = new int[ParameterReferences.Length];
						for (int i = 0; i < backupParameters.Length; i++)
						{
							if (i < parameterIDs.Length)
							{
								backupParameters[i] = parameterIDs[i];
							}
							else
							{
								backupParameters[i] = -1;
							}
						}

						parameterIDs = backupParameters;
					}
					else if (parameterIDs.Length > ParameterReferences.Length)
					{
						// Decrease
						int[] backupParameters = new int[ParameterReferences.Length];
						for (int i = 0; i < backupParameters.Length; i++)
						{
							backupParameters[i] = parameterIDs[i];
						}

						parameterIDs = backupParameters;
					}
				}
				return parameterIDs;
			}
		}

		#endregion


		#if UNITY_EDITOR

		public void AssignID (int _id)
		{
			id = _id;
		}


		public virtual void AssignVariant (int variantIndex) { }


		public void ShowGUI (bool isAssetFile)
		{
			label = CustomGUILayout.TextField ("Label:", label);
			CustomGUILayout.LabelField ("Event:", EventName);

			if (HasConditions (isAssetFile))
			{
				ShowConditionGUI (isAssetFile);
			}

			CustomGUILayout.MultiLineLabelGUI ("Triggered:", ConditionHelp);
			actionListAsset = ActionListAssetMenu.AssetGUI ("ActionList when trigger:", actionListAsset, EventName, string.Empty, "The ActionListAsset to run when the event is triggered", OnAutoCreateActionList);

			if (actionListAsset)
			{
				if (ParameterReferences.Length > 0)
				{
					EditorGUILayout.Space ();
					CustomGUILayout.LabelField ("Parameters:");
					for (int i = 0; i < ParameterReferences.Length; i++)
					{
						ParameterIDs[i] = AC.Action.ChooseParameterGUI (ParameterReferences[i].Label, actionListAsset.DefaultParameters, ParameterIDs[i], ParameterReferences[i].Type, -1, string.Empty, true);
					}
				}
			}
		}


		protected virtual void ShowConditionGUI (bool isAssetFile) { }
		protected abstract bool HasConditions (bool isAssetFile);


		private void OnAutoCreateActionList (ActionListAsset _actionListAsset)
		{
			_actionListAsset.useParameters = true;
			_actionListAsset.DefaultParameters.Clear ();

			for (int i = 0; i < ParameterReferences.Length; i++)
			{
				ActionParameter newParameter = new ActionParameter (i);
				newParameter.parameterType = ParameterReferences[i].Type;
				newParameter.label = ParameterReferences[i].Label;
				newParameter.description = "Set by the " + EventName + " event";

				_actionListAsset.DefaultParameters.Add (newParameter);
				ParameterIDs[i] = i;
			}
		}

		#endif


		#region PrivateClasses

		protected class ParameterReference
		{

			public readonly ParameterType Type;
			public readonly string Label;

			public ParameterReference (ParameterType type, string label)
			{
				Type = type;
				Label = label;
			}

		}

		#endregion

	}

}
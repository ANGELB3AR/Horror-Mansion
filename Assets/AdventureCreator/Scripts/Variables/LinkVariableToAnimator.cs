/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"Variables.cs"
 * 
 *	This component allows Component variables to be linked to an Animator parameter.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** This component allows Component variables to be linked to an Animator parameter. */
	[AddComponentMenu ("Adventure Creator/Logic/Link Variable to Animator")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_link_variable_to_animator.html")]
	public class LinkVariableToAnimator : MonoBehaviour
	{

		#region Variables

		/** The name shared by the Component Variable and the Animator */
		public string sharedVariableName;
		/** The Variables component with the variable to link */
		public Variables variables;
		/** The Animator component with the parameter to link */
		public Animator _animator;
		
		private GVar linkedVariable = null;
		[SerializeField] private LinkableVariableLocation variableLocation = LinkableVariableLocation.Component;
		private enum LinkableVariableLocation { Global=0, Component=2 };

		private string saveDataBackup;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			if (_animator == null)
			{
				_animator = GetComponent <Animator>();
				if (_animator == null)
				{
					ACDebug.LogWarning ("No Animator component found for Link Variable To Animator on " + gameObject, this);
				}
			}

			AssignVariable ();

			EventManager.OnDownloadVariable += OnDownload;
			EventManager.OnUploadVariable += OnUpload;
			EventManager.OnPrepareSaveThread += OnPrepareSaveThread;
			EventManager.OnFailSaving += OnFailSaving;
			EventManager.OnFinishSaving += OnFinishSaving;
		}


		private void Start ()
		{
			if (variableLocation == LinkableVariableLocation.Component && variables == null)
			{
				variables = GetComponent<Variables> ();
				if (variables == null)
				{
					ACDebug.LogWarning ("No Variables component found for Link Variable To Animator on " + gameObject, this);
					return;
				}
			}

			if (string.IsNullOrEmpty (sharedVariableName))
			{
				ACDebug.LogWarning ("No shared variable name set for Link Variable To Animator on " + gameObject, this);
				return;
			}

			AssignVariable ();
		}


		private void OnDisable ()
		{
			EventManager.OnDownloadVariable -= OnDownload;
			EventManager.OnUploadVariable -= OnUpload;
			EventManager.OnPrepareSaveThread -= OnPrepareSaveThread;
			EventManager.OnFailSaving -= OnFailSaving;
			EventManager.OnFinishSaving -= OnFinishSaving;
		}


		private void Update ()
		{
			if (linkedVariable == null || _animator == null || linkedVariable.link == VarLink.CustomScript) return;

			switch (linkedVariable.type)
			{
				case VariableType.Boolean:
					_animator.SetBool (sharedVariableName, linkedVariable.BooleanValue);
					break;

				case VariableType.Integer:
				case VariableType.PopUp:
					_animator.SetInteger (sharedVariableName, linkedVariable.IntegerValue);
					break;

				case VariableType.Float:
					_animator.SetFloat (sharedVariableName, linkedVariable.FloatValue);
					break;

				default:
					break;
			}
		}

		#endregion


#if UNITY_EDITOR

		public void ShowGUI ()
		{
			sharedVariableName = EditorGUILayout.DelayedTextField ("Shared Variable name:", sharedVariableName);

			variableLocation = (LinkableVariableLocation) EditorGUILayout.EnumPopup ("Variable location:", variableLocation);
			if (variableLocation == LinkableVariableLocation.Component)
			{
				variables = (Variables) EditorGUILayout.ObjectField ("Variables:", variables, typeof (Variables), true);
			}
			if (_animator == null) _animator = GetComponent<Animator> ();
			_animator = (Animator) EditorGUILayout.ObjectField ("Animator:", _animator, typeof (Animator), true);

			if (!string.IsNullOrEmpty (sharedVariableName))
			{
				if (variableLocation == LinkableVariableLocation.Global)
				{
					GVar linkedVariable = KickStarter.variablesManager.GetVariable (sharedVariableName);
					if (linkedVariable != null)
					{
						if (linkedVariable.link != VarLink.CustomScript)
						{
							EditorGUILayout.HelpBox ("The Global variable '" + sharedVariableName + "' does not have its 'Link to' field set to 'Custom Script' - the variable will update the Animator, but not vice-versa.", MessageType.Info);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("Variable '" + sharedVariableName + "' was not found for Link Variable To Animator on " + gameObject, MessageType.Warning);
					}
				}
				else
				{
					Variables _variables = variables ? variables : GetComponent<Variables> ();
					if (_variables)
					{
						GVar linkedVariable = _variables.GetVariable (sharedVariableName);
						if (linkedVariable != null)
						{
							if (linkedVariable.link != VarLink.CustomScript)
							{
								EditorGUILayout.HelpBox ("The Component variable '" + sharedVariableName + "' does not have its 'Link to' field set to 'Custom Script' - the variable will update the Animator, but not vice-versa.", MessageType.Info);
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("Variable '" + sharedVariableName + "' was not found for Link Variable To Animator on " + gameObject, MessageType.Warning);
						}
					}
				}
			}
		}

		#endif


		#region CustomEvents

		private void OnDownload (GVar variable, Variables variables)
		{
			if (linkedVariable == null)
			{
				AssignVariable ();
			}

			if (variable != linkedVariable || variable.link != VarLink.CustomScript)
			{
				return;
			}

			switch (variable.type)
			{
				case VariableType.Boolean:
					if (!string.IsNullOrEmpty (saveDataBackup))
					{
						variable.BooleanValue = saveDataBackup == "1";
						break;
					}
					variable.BooleanValue = _animator.GetBool (sharedVariableName);
					break;

				case VariableType.Integer:
				case VariableType.PopUp:
					if (!string.IsNullOrEmpty (saveDataBackup))
					{
						int intValue = 0;
						if (int.TryParse (saveDataBackup, out intValue))
						{
							variable.IntegerValue = intValue;
							break;
						}
					}
					variable.IntegerValue = _animator.GetInteger (sharedVariableName);
					break;

				case VariableType.Float:
					if (!string.IsNullOrEmpty (saveDataBackup))
					{
						float floatValue = 0;
						if (float.TryParse (saveDataBackup, out floatValue))
						{
							variable.FloatValue = floatValue;
							break;
						}
					}
					variable.FloatValue = _animator.GetFloat (sharedVariableName);
					break;

				default:
					break;
			}
		}


		private void OnUpload (GVar variable, Variables variables)
		{
			if (linkedVariable == null)
			{
				AssignVariable ();
			}

			if (variable != linkedVariable || variable.link != VarLink.CustomScript)
			{
				return;
			}

			switch (variable.type)
			{
				case VariableType.Boolean:
					_animator.SetBool (sharedVariableName, variable.BooleanValue);
					break;

				case VariableType.Integer:
				case VariableType.PopUp:
					_animator.SetInteger (sharedVariableName, variable.IntegerValue);
					break;

				case VariableType.Float:
					_animator.SetFloat (sharedVariableName, variable.FloatValue);
					break;

				default:
					break;
			}
		}


		private void OnPrepareSaveThread (SaveFile saveFile)
		{
			if (linkedVariable == null)
			{
				AssignVariable ();
			}

			if (linkedVariable == null) return;

			switch (linkedVariable.type)
			{
				case VariableType.Boolean:
					saveDataBackup = _animator.GetBool (sharedVariableName) ? "1" : "0";
					break;

				case VariableType.Integer:
				case VariableType.PopUp:
					saveDataBackup = _animator.GetInteger (sharedVariableName).ToString ();
					break;

				case VariableType.Float:
					saveDataBackup = _animator.GetFloat (sharedVariableName).ToString ();
					break;

				default:
					break;
			}
		}


		private void OnFinishSaving (SaveFile saveFile)
		{
			saveDataBackup = string.Empty;
		}


		private void OnFailSaving (int saveID)
		{
			saveDataBackup = string.Empty;
		}

		#endregion


		#region PrivateFunctions

		private void AssignVariable ()
		{
			if (linkedVariable != null) return;

			switch (variableLocation)
			{
				case LinkableVariableLocation.Global:
					linkedVariable = GlobalVariables.GetVariable (sharedVariableName);
					break;

				case LinkableVariableLocation.Component:
					linkedVariable = variables.GetVariable (sharedVariableName);
					break;
			}

			if (linkedVariable == null)
			{
				if (KickStarter.runtimeVariables)
				{
					ACDebug.LogWarning ("Variable '" + sharedVariableName + "' was not found for Link Variable To Animator on " + gameObject, this);
				}
			}
			else
			{
				if (linkedVariable.updateLinkOnStart)
				{
					OnDownload (linkedVariable, null);
				}
				else
				{
					OnUpload (linkedVariable, null);
				}
			}
		}

		#endregion

	}

}
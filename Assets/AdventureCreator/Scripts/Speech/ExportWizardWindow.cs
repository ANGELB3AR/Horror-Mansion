#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AC.SML;

namespace AC
{
	
	/** Provides an EditorWindow to manage the export of game text */
	public class ExportWizardWindow : EditorWindow
	{

		private ExportWizardData exportData = new ExportWizardData ();
		private const string ExportDataBackupKey = "ExportWizardWindowBackup";
		
		private SpeechManager speechManager;
		private int sideMenuIndex = -1;

		public enum RowSorting { ByID, ByType, ByScene, ByAssociatedObject, ByDescription };
		private string[] sceneNames;

		private Vector2 scroll;


		private void _Init (SpeechManager _speechManager, string[] _sceneNames, int forLanguage)
		{
			speechManager = _speechManager;
			sceneNames = _sceneNames;

			exportData = new ExportWizardData (speechManager, forLanguage);

			string exportDataBackup = EditorPrefs.GetString (ExportDataBackupKey, string.Empty);
			if (ACEditorPrefs.RetainExportFieldData && !string.IsNullOrEmpty (exportDataBackup))
			{
				EditorJsonUtility.FromJsonOverwrite (exportDataBackup, exportData);

				if (exportData == null)
				{
					exportData = new ExportWizardData (speechManager, forLanguage);
				}
			}
		}


		/** Initialises the window. */
		public static void Init (SpeechManager _speechManager, string[] _sceneNames, int forLanguage = 0)
		{
			if (_speechManager == null) return;

			ExportWizardWindow window = (ExportWizardWindow) GetWindow (typeof (ExportWizardWindow));
			window.titleContent.text = "Text export wizard";
			window.position = new Rect (300, 200, 350, 550);
			window._Init (_speechManager, _sceneNames, forLanguage);
			window.minSize = new Vector2 (300, 180);
		}

		
		private void OnGUI ()
		{
			if (speechManager == null)
			{
				EditorGUILayout.HelpBox ("A Speech Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			if (speechManager.lines == null || speechManager.lines.Count == 0)
			{
				EditorGUILayout.HelpBox ("No text is available to export - click 'Gather text' in your Speech Manager to find your game's text.", MessageType.Warning);
				return;
			}
			
			if (exportData.exportColumns == null)
			{
				exportData.exportColumns = new List<ExportColumn>();
				exportData.exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.LabelField ("Text export wizard", CustomStyles.managerHeader);
			scroll = GUILayout.BeginScrollView (scroll);

			EditorGUILayout.HelpBox ("Choose the fields to export as columns below, then click 'Export CSV'.", MessageType.Info);
			EditorGUILayout.Space ();

			ShowColumnsGUI ();
			ShowRowsGUI ();
			ShowSortingGUI ();

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Export", CustomStyles.subHeader);

			exportData.maxXMLRows = EditorGUILayout.IntField ("Sheet row limit (SML only):", exportData.maxXMLRows);
			if (exportData.exportColumns.Count == 0)
			{
				GUI.enabled = false;
			}
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Export CSV", GUILayout.Width (position.width / 2f - 5)))
			{
				Export (ExportFormat.CSV);
			}
			if (GUILayout.Button ("Export SpreadsheetML", GUILayout.Width (position.width / 2f - 5)))
			{
				Export (ExportFormat.XML);
			}
			EditorGUILayout.EndHorizontal ();
			GUI.enabled = true;

			GUILayout.EndScrollView ();

			if (GUI.changed)
			{
				string exportDataBackup = EditorJsonUtility.ToJson (exportData);
				EditorPrefs.SetString (ExportDataBackupKey, exportDataBackup);
			}
		}


		private void ShowColumnsGUI ()
		{
			EditorGUILayout.LabelField ("Define columns",  CustomStyles.subHeader);
			for (int i = 0; i < exportData.exportColumns.Count; i++)
			{
				CustomGUILayout.BeginVertical ();

				EditorGUILayout.BeginHorizontal ();
				exportData.exportColumns[i].ShowFieldSelector (i);

				if (GUILayout.Button ("", CustomStyles.IconCog))
				{
					SideMenu (i);
				}
				EditorGUILayout.EndHorizontal ();

				exportData.exportColumns[i].ShowLanguageSelector (speechManager.GetLanguageNameArray ());

				CustomGUILayout.EndVertical ();
			}

			if (GUILayout.Button ("Add new column"))
			{
				exportData.exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.Space ();
		}


		private void ShowRowsGUI ()
		{
			EditorGUILayout.LabelField ("Row filtering", CustomStyles.subHeader);

			exportData.filterByType = EditorGUILayout.Toggle ("Filter by type?", exportData.filterByType);
			if (exportData.filterByType)
			{
				exportData.textTypeFilters = (AC_TextTypeFlags) EditorGUILayout.EnumFlagsField ("Limit to type(s):", exportData.textTypeFilters);
			}

			exportData.filterByScene = EditorGUILayout.Toggle ("Filter by scene?", exportData.filterByScene);
			if (exportData.filterByScene)
			{
				if (sceneNames != null && sceneNames.Length > 0)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("-> Limit to scene:", GUILayout.Width (100f));
					exportData.sceneFilter = EditorGUILayout.Popup (exportData.sceneFilter, sceneNames);
					EditorGUILayout.EndHorizontal ();
				}
			}

			exportData.filterByText = EditorGUILayout.Toggle ("Filter by text:", exportData.filterByText);
			if (exportData.filterByText)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("-> Limit to text:", GUILayout.Width (100f));
				exportData.filterSpeechLine = (FilterSpeechLine) EditorGUILayout.EnumPopup (exportData.filterSpeechLine, GUILayout.MaxWidth (100f));
				exportData.textFilter = EditorGUILayout.TextField (exportData.textFilter);
				EditorGUILayout.EndHorizontal ();
			}

			if (IsTextTypeFiltered (AC_TextType.Speech) && speechManager.useSpeechTags)
			{
				exportData.filterByTag = EditorGUILayout.Toggle ("Filter by speech tag:", exportData.filterByTag);
				if (exportData.filterByTag)
				{
					if (speechManager.speechTags != null && speechManager.speechTags.Count > 1)
					{
						if (exportData.tagFilter == -1)
						{
							exportData.tagFilter = 0;
						}

						List<string> tagNames = new List<string>();
						foreach (SpeechTag speechTag in speechManager.speechTags)
						{
							tagNames.Add (speechTag.label);
						}

						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("-> Limit by tag:", GUILayout.Width (65f));
						exportData.tagFilter = EditorGUILayout.Popup (exportData.tagFilter, tagNames.ToArray ());
						EditorGUILayout.EndHorizontal ();
					}
					else
					{
						exportData.tagFilter = -1;
						EditorGUILayout.HelpBox ("No tags defined - they can be created by clicking 'Edit speech tags' in the Speech Manager.", MessageType.Info);
					}
				}
			}

			for (int i = 0; i < exportData.exportColumns.Count; i++)
			{ 
				if (exportData.exportColumns[i].GetLanguageIndex () > 0)
				{
					exportData.excludeLinesWithTranslations = EditorGUILayout.Toggle ("No lines with translations?", exportData.excludeLinesWithTranslations);
					break;
				}
			}

			EditorGUILayout.Space ();
		}


		private bool IsTextTypeFiltered (AC_TextType textType)
		{
			int s1 = (int) textType;
			int s1_modified = (int) Mathf.Pow (2f, (float) s1);
			int s2 = (int) exportData.textTypeFilters;
			return (s1_modified & s2) != 0;
		}


		private void ShowSortingGUI ()
		{
			EditorGUILayout.LabelField ("Row sorting", CustomStyles.subHeader);

			exportData.doRowSorting = EditorGUILayout.Toggle ("Apply row sorting?", exportData.doRowSorting);
			if (exportData.doRowSorting)
			{
				exportData.rowSorting = (RowSorting) EditorGUILayout.EnumPopup ("Sort rows:", exportData.rowSorting);
			}
		}


		private void SideMenu (int i)
		{
			GenericMenu menu = new GenericMenu ();

			sideMenuIndex = i;

			if (exportData.exportColumns.Count > 1)
			{
				if (i > 0)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, MenuCallback, "Move to top");
					menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, MenuCallback, "Move up");
				}
				if (i < (exportData.exportColumns.Count - 1))
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, MenuCallback, "Move down");
					menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, MenuCallback, "Move to bottom");
				}
				menu.AddSeparator ("");
			}
			menu.AddItem (new GUIContent ("Delete"), false, MenuCallback, "Delete");
			menu.ShowAsContext ();
		}


		private void MenuCallback (object obj)
		{
			if (sideMenuIndex >= 0)
			{
				int i = sideMenuIndex;
				ExportColumn _column = exportData.exportColumns[i];

				switch (obj.ToString ())
				{
					case "Move to top":
						exportData.exportColumns.Remove (_column);
						exportData.exportColumns.Insert (0, _column);
						break;
					
					case "Move up":
						exportData.exportColumns.Remove (_column);
						exportData.exportColumns.Insert (i-1, _column);
					break;
					
					case "Move to bottom":
						exportData.exportColumns.Remove (_column);
						exportData.exportColumns.Insert (exportData.exportColumns.Count, _column);
						break;
					
					case "Move down":
						exportData.exportColumns.Remove (_column);
						exportData.exportColumns.Insert (i+1, _column);
						break;

					case "Delete":
						exportData.exportColumns.Remove (_column);
						break;

					default:
						break;
				}
			}
			
			sideMenuIndex = -1;
		}

		
		private void Export (ExportFormat format)
		{
			#if UNITY_WEBPLAYER
			ACDebug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			#else
			
			if (speechManager == null || exportData.exportColumns == null || exportData.exportColumns.Count == 0 || speechManager.lines == null || speechManager.lines.Count == 0) return;

			string suggestedFilename = string.Empty;
			if (KickStarter.settingsManager)
			{
				suggestedFilename = KickStarter.settingsManager.saveFileName + " - ";
			}

			string extension;
			switch (format)
			{
				case ExportFormat.CSV:
				default:
					extension = "csv";
					break;

				case ExportFormat.XML:
					extension = "xml";
					break;
			}
			
			suggestedFilename += "GameText." + extension;

			string fileName = EditorUtility.SaveFilePanel ("Export game text", "Assets", suggestedFilename, extension);
			if (fileName.Length == 0)
			{
				return;
			}

			List<int> exportedTranslations = new List<int> ();
			foreach (ExportColumn exportColumn in exportData.exportColumns)
			{
				int languageIndex = exportColumn.GetLanguageIndex ();
				if (languageIndex > 0 && !exportedTranslations.Contains (languageIndex))
				{
					exportedTranslations.Add (languageIndex);
				}
			}

			List<SpeechLine> exportLines = new List<SpeechLine>();
			foreach (SpeechLine line in speechManager.lines)
			{
				if (line.ignoreDuringExport)
				{
					continue;
				}

				if (exportData.filterByType)
				{
					if (!IsTextTypeFiltered (line.textType))
					{
						continue;
					}
				}

				if (exportData.filterByScene)
				{
					if (sceneNames != null && sceneNames.Length > exportData.sceneFilter)
					{
						string selectedScene = sceneNames[exportData.sceneFilter] + ".unity";
						string scenePlusExtension = (string.IsNullOrEmpty (line.scene)) ? string.Empty : (line.scene + ".unity");
						
						if ((string.IsNullOrEmpty (line.scene) && exportData.sceneFilter == 0)
						    || exportData.sceneFilter == 1
						    || (!string.IsNullOrEmpty (line.scene) && exportData.sceneFilter > 1 && line.scene.EndsWith (selectedScene))
						    || (!string.IsNullOrEmpty (line.scene) && exportData.sceneFilter > 1 && scenePlusExtension.EndsWith (selectedScene)))
						{}
						else
						{
							continue;
						}
					}
				}

				if (exportData.filterByText)
				{
					if (!line.Matches (exportData.textFilter, exportData.filterSpeechLine))
					{
						continue;
					}
				}

				if (exportData.filterByTag)
				{
					if (exportData.tagFilter == -1
						|| (exportData.tagFilter < speechManager.speechTags.Count && line.tagID == speechManager.speechTags[exportData.tagFilter].ID))
					{}
					else
					{
						continue;
					}
				}

				if (exportData.excludeLinesWithTranslations)
				{
					bool anyNotEmpty = false;
					foreach (int translationIndex in exportedTranslations)
					{
						if (!string.IsNullOrEmpty (line.GetTranslation (string.Empty, translationIndex)))
						{
							anyNotEmpty = true;
						}
					}
					if (anyNotEmpty && exportedTranslations.Count > 0 && line.translationText.Count > 0)
					{
						continue;
					}
				}

				exportLines.Add (new SpeechLine (line));
			}

			if (exportData.doRowSorting)
			{
				switch (exportData.rowSorting)
				{
					case RowSorting.ByID:
						exportLines.Sort ((a, b) => a.lineID.CompareTo (b.lineID));
						break;

					case RowSorting.ByDescription:
						exportLines.Sort ((a, b) => string.Compare (a.description, b.description, System.StringComparison.Ordinal));
						break;

					case RowSorting.ByType:
						exportLines.Sort ((a, b) => string.Compare (a.textType.ToString (), b.textType.ToString (), System.StringComparison.Ordinal));
						break;

					case RowSorting.ByAssociatedObject:
						exportLines.Sort ((a, b) => string.Compare (a.owner, b.owner, System.StringComparison.Ordinal));
						break;

					case RowSorting.ByScene:
						exportLines.Sort ((a, b) => string.Compare (a.scene, b.scene, System.StringComparison.Ordinal));
						break;

					default:
						break;
				}
			}

			List<string[]> output = new List<string[]>();

			List<string> headerList = new List<string>();
			headerList.Add ("ID");
			foreach (ExportColumn exportColumn in exportData.exportColumns)
			{
				headerList.Add (exportColumn.GetHeader (speechManager.GetLanguageNameArray ()));
			}
			output.Add (headerList.ToArray ());
		
			foreach (SpeechLine line in exportLines)
			{
				List<string> rowList = new List<string>();
				rowList.Add (line.lineID.ToString ());
				foreach (ExportColumn exportColumn in exportData.exportColumns)
				{
					string cellText = exportColumn.GetCellText (line);
					rowList.Add (cellText);
				}
				output.Add (rowList.ToArray ());
			}

			string fileContents;
			switch (format)
			{
				case ExportFormat.CSV:
				default:
					fileContents = CSVReader.CreateCSVGrid (output);
					break;

				case ExportFormat.XML:
					fileContents = SMLReader.CreateXMLGrid (output, exportData.maxXMLRows);
					break;
			}

			if (!string.IsNullOrEmpty (fileContents) && Serializer.SaveFile (fileName, fileContents))
			{
				int numLines = exportLines.Count;
				ACDebug.Log (numLines.ToString () + " line" + ((numLines != 1) ? "s" : string.Empty) + " exported.");
			}

			#endif
		}


		[Serializable]
		public class ExportColumn
		{

			[SerializeField] private ColumnType columnType;
			[SerializeField] private int language;
			public enum ColumnType { DisplayText, Type, AssociatedObject, Scene, Description, TagID, TagName, SpeechOrder, AudioFilename, AudioFilePresence };


			public ExportColumn ()
			{
				columnType = ColumnType.DisplayText;
				language = 0;
			}


			public ExportColumn (ColumnType _columnType)
			{
				columnType = _columnType;
				language = 0;
			}


			public ExportColumn (int _language)
			{
				columnType = ColumnType.DisplayText;
				language = _language;
			}


			public void ShowFieldSelector (int i)
			{
				columnType = (ColumnType) EditorGUILayout.EnumPopup ("Column #" + (i+1).ToString () + ":", columnType);

				if (columnType == ColumnType.AudioFilePresence && KickStarter.speechManager.referenceSpeechFiles == ReferenceSpeechFiles.ByAssetBundle)
				{
					EditorGUILayout.HelpBox ("Presence from asset bundles cannot be determined in Edit mode - files will be searched for in Resources folders.", MessageType.Warning);
				}
				else if (columnType == ColumnType.AudioFilePresence && KickStarter.speechManager.referenceSpeechFiles == ReferenceSpeechFiles.ByAddressable)
				{
					EditorGUILayout.HelpBox ("Presence of Addressable assets cannot be determined in Edit mode.", MessageType.Warning);
				}
			}


			public void ShowLanguageSelector (string[] languages)
			{
				if (columnType == ColumnType.DisplayText)
				{
					language = Mathf.Clamp (language, 0, languages.Length - 1);
					language = EditorGUILayout.Popup ("Language:", language, languages);
				}
			}


			public string GetHeader (string[] languages)
			{
				if (columnType == ColumnType.DisplayText)
				{
					if (language > 0)
					{
						if (languages != null && languages.Length > language)
						{
							return languages[language];
						}
						return ("Invalid language");
					}
					return ("Original text");
				}
				return columnType.ToString ();
			}


			public int GetLanguageIndex ()
			{
				if (columnType == ColumnType.DisplayText)
				{
					return language;
				}
				return -1;
			}


			public string GetCellText (SpeechLine speechLine)
			{
				string cellText = " ";

				switch (columnType)
				{
					case ColumnType.DisplayText:
						if (language > 0)
						{
							int translation = language-1;
							if (speechLine.translationText != null && speechLine.translationText.Count > translation)
							{
								cellText = speechLine.translationText[translation];
							}
						}
						else
						{
							cellText = speechLine.text;
						}
						break;

					case ColumnType.Type:
						cellText = speechLine.textType.ToString ();
						break;

					case ColumnType.AssociatedObject:
						if (speechLine.isPlayer && string.IsNullOrEmpty (speechLine.owner) && speechLine.textType == AC_TextType.Speech)
						{
							cellText = "Player";
						}
						else
						{
							cellText = speechLine.owner;
						}
						break;

					case ColumnType.Scene:
						cellText = speechLine.scene;
						break;

					case ColumnType.Description:
						cellText = speechLine.description;
						break;

					case ColumnType.TagID:
						cellText = speechLine.tagID.ToString ();
						break;

					case ColumnType.TagName:
						SpeechTag speechTag = KickStarter.speechManager.GetSpeechTag (speechLine.tagID);
						cellText = (speechTag != null) ? speechTag.label : "";
						break;

					case ColumnType.SpeechOrder:
						cellText = speechLine.OrderIdentifier;
						if (cellText == "-0001")
						{
							cellText = string.Empty;
						}
						break;

					case ColumnType.AudioFilename:
						if (speechLine.textType == AC_TextType.Speech)
						{
							if (speechLine.SeparatePlayerAudio ())
							{
								string result = string.Empty;
								for (int j = 0; j < KickStarter.settingsManager.players.Count; j++)
								{
									if (KickStarter.settingsManager.players[j].EditorPrefab != null)
									{
										string overrideName = KickStarter.settingsManager.players[j].EditorPrefab.name;
										result += speechLine.GetFilename (overrideName) + ";";
									}
								}
								cellText = result;
							}
							else
							{
								cellText = speechLine.GetFilename ();
							}
						}
						break;

					case ColumnType.AudioFilePresence:
						if (speechLine.textType == AC_TextType.Speech)
						{
							bool hasAllAudio = speechLine.HasAllAudio ();
							if (hasAllAudio)
							{
								cellText = "Has all audio";
							}
							else
							{
								if (speechLine.HasAudio (0))
								{
									string missingLabel = "Missing ";
									for (int i=1; i<KickStarter.speechManager.Languages.Count; i++)
									{
										if (!speechLine.HasAudio (i))
										{
											missingLabel += KickStarter.speechManager.Languages[i].name + ", ";
										}
									}
									if (missingLabel.EndsWith (", "))
									{
										missingLabel = missingLabel.Substring (0, missingLabel.Length-2);
									}
									cellText = missingLabel;
								}
								else
								{
									cellText = "Missing main audio";
								}
							}
						}
						break;
				}


				if (string.IsNullOrEmpty (cellText))
				{
					cellText = " ";
				}
				return RemoveLineBreaks (cellText);
			}


			private string RemoveLineBreaks (string text)
			{
				if (text.Length == 0) return " ";
				//text = text.Replace("\r\n", "[break]").Replace("\n", "[break]");
				text = text.Replace("\r\n", "[break]");
				text = text.Replace("\n", "[break]");
				text = text.Replace("\r", "[break]");
				return text;
			}

		}



		[Serializable]
		public class ExportWizardData
		{

			public List<ExportColumn> exportColumns = new List<ExportColumn> ();
			public bool filterByType = false;
			public bool filterByScene = false;
			public bool filterByText = false;
			public bool filterByTag = false;
			public bool excludeLinesWithTranslations = false;
			public string textFilter;
			public FilterSpeechLine filterSpeechLine = FilterSpeechLine.Text;
			public AC_TextTypeFlags textTypeFilters = (AC_TextTypeFlags) ~0;
			public int tagFilter;
			public int sceneFilter;
			public bool doRowSorting = false;
			public RowSorting rowSorting = RowSorting.ByID;
			public int maxXMLRows = 250;


			public ExportWizardData ()
			{
				exportColumns.Clear ();
				exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.Type));
				exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.DisplayText));
			}


			public ExportWizardData (SpeechManager speechManager, int forLanguage)
			{
				exportColumns.Clear ();
				exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.Type));
				exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.DisplayText));

				if (speechManager != null && forLanguage > 0 && speechManager.Languages != null && speechManager.Languages.Count > forLanguage)
				{
					exportColumns.Add (new ExportColumn (forLanguage));
				}
			}

		}
		
		
	}
	
}

#endif
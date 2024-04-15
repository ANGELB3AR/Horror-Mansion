#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AC.SML;

namespace AC
{
	
	/** Provides an EditorWindow to manage the export of inventory items */
	public class InvItemExportWizardWindow : EditorWindow
	{

		private ExportWizardData exportData = new ExportWizardData ();
		private const string ExportDataBackupKey = "InvItemExportWizardBackup";
		private InventoryManager inventoryManager;
		private int sideMenuIndex = -1;
		private Vector2 scroll;


		public void _Init (InventoryManager _inventoryManager)
		{
			inventoryManager = _inventoryManager;
			exportData = new ExportWizardData ();

			string exportDataBackup = EditorPrefs.GetString (ExportDataBackupKey, string.Empty);
			if (ACEditorPrefs.RetainExportFieldData && !string.IsNullOrEmpty (exportDataBackup))
			{
				EditorJsonUtility.FromJsonOverwrite (exportDataBackup, exportData);

				if (exportData == null)
				{
					exportData = new ExportWizardData ();
				}
			}
		}


		/** Initialises the window. */
		public static void Init (InventoryManager _inventoryManager)
		{
			if (_inventoryManager == null) return;

			InvItemExportWizardWindow window = (InvItemExportWizardWindow) GetWindow (typeof (InvItemExportWizardWindow));

			window.titleContent.text = "Inventory item exporter";
			window.position = new Rect (300, 200, 350, 500);
			window._Init (_inventoryManager);
			window.minSize = new Vector2 (300, 180);
		}
		
		
		private void OnGUI ()
		{
			if (inventoryManager == null)
			{
				EditorGUILayout.HelpBox ("An Inventory Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			if (inventoryManager.items == null || inventoryManager.items.Count == 0)
			{
				EditorGUILayout.HelpBox ("No inventory items are available to export.", MessageType.Warning);
				return;
			}
			
			if (exportData.exportColumns == null)
			{
				exportData.exportColumns = new List<ExportColumn>();
				exportData.exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.LabelField ("Inventory item exporter", CustomStyles.managerHeader);
			scroll = GUILayout.BeginScrollView (scroll);

			EditorGUILayout.HelpBox ("Choose the fields to export as columns below, then click 'Export CSV'.", MessageType.Info);
			EditorGUILayout.Space ();

			ShowColumnsGUI ();

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Export", CustomStyles.subHeader);
			EditorGUILayout.BeginHorizontal ();
			if (exportData.exportColumns.Count == 0)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button ("CSV", GUILayout.Width (position.width / 2f - 5)))
			{
				Export (ExportFormat.CSV);
			}
			if (GUILayout.Button ("SpreadsheetML", GUILayout.Width (position.width / 2f - 5)))
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
			for (int i = 0; i <  exportData.exportColumns.Count; i++)
			{
				CustomGUILayout.BeginVertical ();

				EditorGUILayout.BeginHorizontal ();
				exportData.exportColumns[i].ShowFieldSelector (i);
				if (GUILayout.Button ("", CustomStyles.IconCog))
				{
					SideMenu (i);
				}
				EditorGUILayout.EndHorizontal ();

				CustomGUILayout.EndVertical ();
			}

			if (GUILayout.Button ("Add new column"))
			{
				exportData.exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.Space ();
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
			
			if (inventoryManager == null || exportData.exportColumns == null || exportData.exportColumns.Count == 0 || inventoryManager.items == null || inventoryManager.items.Count == 0) return;

			string suggestedFilename = "";
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

			suggestedFilename += "Inventory." + extension;
			
			string fileName = EditorUtility.SaveFilePanel ("Export inventory items", "Assets", suggestedFilename, extension);
			if (fileName.Length == 0)
			{
				return;
			}

			List<InvItem> exportItems = new List<InvItem>();
			foreach (InvItem item in inventoryManager.items)
			{
				exportItems.Add (new InvItem (item));
			}

			List<string[]> output = new List<string[]>();

			List<string> headerList = new List<string>();
			headerList.Add ("ID");
			foreach (ExportColumn exportColumn in exportData.exportColumns)
			{
				if (exportColumn.columnType == ExportColumn.ColumnType.Properties)
				{
					for (int p = 0; p < KickStarter.inventoryManager.invVars.Count; p++)
					{
						headerList.Add (exportColumn.GetHeader (p));
					}
				}
				else
				{
					headerList.Add (exportColumn.GetHeader (0));
				}
			}
			output.Add (headerList.ToArray ());
			
			foreach (InvItem exportItem in exportItems)
			{
				List<string> rowList = new List<string>();
				rowList.Add (exportItem.id.ToString ());
				foreach (ExportColumn exportColumn in exportData.exportColumns)
				{
					if (exportColumn.columnType == ExportColumn.ColumnType.Properties)
					{
						for (int p = 0; p < KickStarter.inventoryManager.invVars.Count; p++)
						{
							string cellText = exportColumn.GetCellText (exportItem, inventoryManager, p);
							rowList.Add (cellText);
						}
					}
					else
					{
						string cellText = exportColumn.GetCellText (exportItem, inventoryManager, 0);
						rowList.Add (cellText);
					}
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
					fileContents = SMLReader.CreateXMLGrid (output);
					break;
			}

			if (!string.IsNullOrEmpty (fileContents) && Serializer.SaveFile (fileName, fileContents))
			{
				ACDebug.Log (exportItems.Count.ToString () + " items exported.");
			}

			//this.Close ();
			#endif
		}


		[Serializable]
		private class ExportColumn
		{

			public ColumnType columnType;
			public enum ColumnType { InternalName, Label, MainGraphic, Category, CategoryID, CarryOnStart, CanCarryMultiple, Properties };


			public ExportColumn ()
			{
				columnType = ColumnType.InternalName;
			}


			public ExportColumn (ColumnType _columnType)
			{
				columnType = _columnType;
			}


			public void ShowFieldSelector (int i)
			{
				columnType = (ColumnType) EditorGUILayout.EnumPopup ("Column #" + (i+1).ToString () + ":", columnType);
			}


			public string GetHeader (int propertyIndex)
			{
				if (columnType == ColumnType.Properties)
				{
					return KickStarter.inventoryManager.invVars[propertyIndex].label;
				}
				return columnType.ToString ();
			}


			public string GetCellText (InvItem invItem, InventoryManager inventoryManager, int propertyIndex)
			{
				string cellText = " ";

				switch (columnType)
				{
					case ColumnType.InternalName:
						cellText = invItem.label;
						break;

					case ColumnType.Label:
						cellText = invItem.altLabel;
						break;

					case ColumnType.MainGraphic:
						cellText = (invItem.tex) ? invItem.tex.name : "";
						break;

					case ColumnType.Category:
						if (invItem.binID >= 0)
						{
							InvBin invBin = inventoryManager.GetCategory (invItem.binID);
							cellText = (invBin != null) ? invBin.label : "";
						}
						break;

					case ColumnType.CategoryID:
						cellText = (invItem.binID >= 0) ? invItem.binID.ToString () : "";
						break;

					case ColumnType.CarryOnStart:
						cellText = (invItem.carryOnStart) ? "True" : "False";
						break;

					case ColumnType.CanCarryMultiple:
						cellText = (invItem.canCarryMultiple) ? "True" : "False";
						break;

					case ColumnType.Properties:
						cellText = invItem.vars[propertyIndex].GetDisplayValue ();
						break;
				}

				if (cellText == "") cellText = " ";
				return RemoveLineBreaks (cellText);
			}


			private string RemoveLineBreaks (string text)
			{
				if (text.Length == 0) return " ";
				text = text.Replace("\r\n", "[break]");
				text = text.Replace("\n", "[break]");
				text = text.Replace("\r", "[break]");
				return text;
			}

		}


		[Serializable]
		private class ExportWizardData
		{

			public List<ExportColumn> exportColumns = new List<ExportColumn> ();


			public ExportWizardData ()
			{
				exportColumns.Clear();
				exportColumns.Add(new ExportColumn (ExportColumn.ColumnType.InternalName));
			}

		}

	}
	
}

#endif
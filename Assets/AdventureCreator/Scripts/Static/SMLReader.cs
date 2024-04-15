using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AC.SML
{

	public class SMLReader
	{

		private const string xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?><?mso-application progid=\"Excel.Sheet\"?>";
		private const string workbookHeader = "<Workbook xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:x2=\"urn:schemas-microsoft-com:office:excel2\" xmlns:html=\"http://www.w3.org/TR/REC-html40\" xmlns:dt=\"uuid:C2F41010-65B3-11d1-A29F-00AA00C14882\">";


		static public string[,] SplitXMLGrid (string xmlText)
		{
			try
			{
				xmlText = xmlText.Replace ("<ss:", "<");
				xmlText = xmlText.Replace ("</ss:", "</");

				var ms = new MemoryStream (Encoding.UTF8.GetBytes (xmlText));
				var reader = new XmlTextReader (ms) { Namespaces = false };
				var serializer = new XmlSerializer (typeof (WorkbookXml));

				WorkbookXml result = (WorkbookXml) serializer.Deserialize (reader);

				int numWorksheets = result.Worksheets.Length;
				if (numWorksheets == 0)
				{
					return new string[0,0];
				}

				List<string[]> outputGrid = new List<string[]> ();
				for (int w = 0; w < result.Worksheets.Length; w++)
				{
					int numRows = result.Worksheets[w].Table.Rows.Length;
					int numCols = result.Worksheets[w].Table.Rows[0].Cells.Length;

					for (int r = 0; r < numRows; r++)
					{
						if (r == 0 && w != 0) continue;

						RowXml row = result.Worksheets[w].Table.Rows[r];
						string[] lineArray = new string[numCols];

						for (int c = 0; c < numCols; c++)
						{
							string data = row.Cells[c].Data;
							data = data.Replace ("&lt;", "<");
							data = data.Replace ("&gt;", ">");

							lineArray[c] = data;
						}

						outputGrid.Add (lineArray);
					}
				}

				string[,] outputGridArray = new string[outputGrid[0].Length, outputGrid.Count];

				for (int r = 0; r < outputGrid.Count; r++)
				{
					string[] rowData = outputGrid[r];
					for (int c = 0; c < rowData.Length; c++)
					{
						outputGridArray[c, r] = rowData[c];
					}
				}

				return outputGridArray;
			}
			catch (Exception e)
			{
				ACDebug.LogWarning ("Error importing XML file, exception: " + e);
				return null;
			}
		}


		public static string CreateXMLGrid (List<string[]> contents, int maxRows = 500)
		{
			List<List<string[]>> contentsArray = new List<List<string[]>> { contents };
			return CreateXMLGrid (contentsArray, maxRows);
		}


		public static string CreateXMLGrid (List<List<string[]>> contentsArray, int maxRows = 500)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (xmlHeader);
			sb.AppendLine ();
			sb.Append (workbookHeader);

			if (maxRows > 1)
			{
				for (int i = 0; i < contentsArray.Count; i++)
				{
					if (contentsArray[i].Count > maxRows)
					{
						List<string[]> newSheet = new List<string[]> ();
						newSheet.Add (contentsArray[i][0]);

						var range = contentsArray[i].GetRange (maxRows, contentsArray[i].Count - maxRows);
						newSheet.AddRange (range);
						contentsArray[i].RemoveRange (maxRows, contentsArray[i].Count - maxRows);
						contentsArray.Insert (i + 1, newSheet);
					}
				}
			}

			int numSheets = contentsArray.Count;
			for (int w = 0; w < numSheets; w++)
			{
				List<string[]> contents = contentsArray[w];
				int numRows = contents.Count;
				int numCols = contents[0].Length;

				string sheetName = "Sheet" + (w+1).ToString ();

				sb.Append ("<Worksheet ss:Name=\"").Append (sheetName).Append ("\">");
				sb.AppendLine ();
				sb.Append ("<ss:Names />");
				sb.AppendLine ();
				sb.Append ("<ss:Table ss:DefaultRowHeight=\"12.75\" ss:DefaultColumnWidth=\"66\" ss:ExpandedRowCount=\"").Append (numRows.ToString ()).Append ("\" ss:ExpandedColumnCount=\"").Append (numCols.ToString ()).Append ("\">");
				sb.AppendLine ();

				for (int row = 0; row < numRows; row++)
				{
					string rowIndex = (row+1).ToString ();

					sb.Append ("<Row ss:Index=\"").Append (rowIndex).Append ("\">");
					sb.AppendLine ();

					for (int col = 0; col < numCols; col++)
					{
						string cellText = contents[row][col];
						cellText = cellText.Replace ("<", "&lt;");
						cellText = cellText.Replace (">", "&gt;");
						sb.Append ("<Cell><Data ss:Type=\"String\">").Append (cellText).Append ("</Data></Cell>");
						sb.AppendLine ();
					}

					sb.Append ("</Row>");
					sb.AppendLine ();
				}

				sb.Append ("</ss:Table>");
				sb.AppendLine ();
				sb.Append ("</Worksheet>");
				sb.AppendLine ();
			}

			sb.Append ("</Workbook>");
			sb.AppendLine ();

			return sb.ToString ();
		}

	}


	[XmlRoot ("Workbook")]
	public class WorkbookXml
	{
		[XmlAnyAttribute] public XmlAttribute[] XAttributes { get; set; }
		[XmlElement (ElementName = "Worksheet")] public WorksheetXml[] Worksheets { get; set; }
	}


	public class WorksheetXml
	{
		[XmlAttribute ("Names")] public string Names { get; set; }
		[XmlElement (ElementName = "Table")] public TableXml Table { get; set; }
	}


	public class TableXml
	{
		[XmlElement (ElementName = "Row")] public RowXml[] Rows { get; set; }
	}


	public class RowXml
	{
		[XmlElement (ElementName = "Cell")] public CellXml[] Cells { get; set; }
	}


	public class CellXml
	{
		[XmlElement ("Data")] public string Data { get; set; }
	}

}
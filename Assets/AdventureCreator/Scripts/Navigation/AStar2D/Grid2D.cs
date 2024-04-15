using System.Collections.Generic;
using UnityEngine;

namespace AC.AStar2D
{

	public class Grid2D
	{

		#region Variables

		private readonly Collider2D Collider;
		private Node[,] nodes;
		private int gridSizeX, gridSizeY;
		private readonly float cellWidth, cellHeight;

		#endregion


		#region Constructors

		public Grid2D (Collider2D _collider, float _cellWidth, float _cellHeight)
		{
			Collider = _collider;
			cellWidth = _cellWidth;
			cellHeight = _cellHeight;

			Rebuild ();
		}

		#endregion


		#region PublicFunctions

		public void Rebuild ()
		{
			gridSizeX = Mathf.RoundToInt (Collider.bounds.size.x / cellWidth);
			gridSizeY = Mathf.RoundToInt (Collider.bounds.size.y / cellHeight);

			nodes = new Node[gridSizeX, gridSizeY];

			Vector2 bottomLeft = (Vector2) Collider.bounds.center - (Vector2.right * Collider.bounds.size.x / 2f) - (Vector2.up * Collider.bounds.size.y / 2f);

			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					Vector2 position = bottomLeft + Vector2.right * ((x * cellWidth) + (cellWidth * 0.5f)) + Vector2.up * ((y * cellHeight) + (cellHeight * 0.5f));
					nodes[x, y] = new Node (position, x, y, IsPointInside (position));
				}
			}
		}


		public void DrawGizmos ()
		{
			Gizmos.color = Color.gray;
			foreach (Node node in nodes)
			{
				if (!node.IsWalkable) continue;
				Gizmos.DrawWireCube (node.Position, new Vector2 (cellWidth, cellHeight));
			}
		}


		public Node GridToNode (int gridX, int gridY)
		{
			if (nodes == null)
			{
				Rebuild ();
			}

			return nodes[gridX, gridY];
		}


		public Node PositionToNode (Vector2 position)
		{
			Vector2 bottomLeft = (Vector2) Collider.bounds.center - (Vector2.right * Collider.bounds.size.x / 2f) - (Vector2.up * Collider.bounds.size.y / 2f);
			float percentX = (position.x - bottomLeft.x) / Collider.bounds.size.x;
			float percentY = (position.y - bottomLeft.y) / Collider.bounds.size.y;

			percentX = Mathf.Clamp01 (percentX);
			percentY = Mathf.Clamp01 (percentY);

			int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
			int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);
			
			Node node = nodes[x, y];
			if (node.IsWalkable)
			{
				return nodes[x, y];
			}

			return GetNearestOnMesh (node);
		}


		public int PositionToGridX (Vector2 position)
		{
			Vector2 bottomLeft = (Vector2) Collider.bounds.center - (Vector2.right * Collider.bounds.size.x / 2f) - (Vector2.up * Collider.bounds.size.y / 2f);
			float percentX = (position.x - bottomLeft.x) / Collider.bounds.size.x;

			percentX = Mathf.Clamp01 (percentX);

			return Mathf.RoundToInt ((gridSizeX - 1) * percentX);
		}


		public int PositionToGridY (Vector2 position)
		{
			Vector2 bottomLeft = (Vector2) Collider.bounds.center - (Vector2.right * Collider.bounds.size.x / 2f) - (Vector2.up * Collider.bounds.size.y / 2f);
			float percentY = (position.y - bottomLeft.y) / Collider.bounds.size.y;

			percentY = Mathf.Clamp01 (percentY);

			return Mathf.RoundToInt ((gridSizeY - 1) * percentY);
		}


		private Node GetNearestOnMesh (Node node)
		{
			if (node.IsWalkable)
			{
				return node;
			}

			for (int d = 1; d <= Mathf.Max (nodes.GetLength (0), nodes.GetLength (1)); d++)
			{
				for (int i = 1; i <= 8; i++)
				{
					int nX, nY;
					switch (i)
					{
						default:
						case 1:
							nX = node.GridX;
							nY = node.GridY + d;
							break;

						case 2:
							nX = node.GridX + d;
							nY = node.GridY;
							break;

						case 3:
							nX = node.GridX;
							nY = node.GridY - d;
							break;

						case 4:
							nX = node.GridX - d;
							nY = node.GridY;
							break;

						case 5:
							nX = node.GridX + d;
							nY = node.GridY + d;
							break;

						case 6:
							nX = node.GridX + d;
							nY = node.GridY - d;
							break;

						case 7:
							nX = node.GridX - d;
							nY = node.GridY - d;
							break;

						case 8:
							nX = node.GridX - d;
							nY = node.GridY + d;
							break;
					}

					if (nX < 0 || nX >= nodes.GetLength (0)) continue;
					if (nY < 0 || nY >= nodes.GetLength (1)) continue;

					if (nodes[nX, nY].IsWalkable)
					{
						return nodes[nX, nY];
					}
				}
			}

			Debug.LogWarning ("Error cannot find nearest node to position " + node.Position);
			return null;
		}


		public int GetNeighbours (Node node, ref Node[] neighbourArray)
		{
			int numNeighbours = 0;

			for (int x = -1; x <= 1; x++)
			{
				for	(int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0) continue;

					int neighbourX = node.GridX + x;
					int neighbourY = node.GridY + y;

					if (neighbourX >= 0 && neighbourX < gridSizeX && neighbourY >= 0 && neighbourY < gridSizeY)
					{
						neighbourArray[numNeighbours] = nodes[neighbourX, neighbourY];
						numNeighbours++;
					}
				}
			}

			return numNeighbours;
		}


		public bool IsPointInside (Vector2 pos)
		{
			return Collider.OverlapPoint (pos);
		}


		public void ClearCharHoles ()
		{
			foreach (Node node in nodes)
			{
				node.EvadeCharacter = false;
			}
		}


		public void AddCharHole (Vector3 centre, float radius, float yScale)
		{
			Node centreNode = PositionToNode (centre);
			if (centreNode == null) return;

			int farLeftX = PositionToGridX (centre - radius * Vector3.right);
			int farLeftY = PositionToGridY (centre - radius * Vector3.right);
			
			int farRightX = PositionToGridX (centre + radius * Vector3.right);
			int farRightY = PositionToGridY (centre + radius * Vector3.right);

			for (int x = farLeftX; x <= farRightX; x++)
			{
				nodes[x, centreNode.GridY].EvadeCharacter = true;
			}

			int farTopY = PositionToGridY (centre + (radius * yScale * Vector3.up));
			int farBottomY = PositionToGridY (centre - (radius * yScale * Vector3.up));

			for (int y = farBottomY; y <= farTopY; y++)
			{
				nodes[centreNode.GridX, y].EvadeCharacter = true;
			}

			// Left fill
			if (centreNode.GridX != farLeftX)
			{
				for (int x = farLeftX; x <= centreNode.GridX; x++)
				{
					float yLerp = (float) (x - farLeftX) / (float) (centreNode.GridX - farLeftX);
					int maxY = (int) Mathf.Lerp (farLeftY, farTopY, yLerp);
					int minY = (int) Mathf.Lerp (farLeftY, farBottomY, yLerp);

					for (int y = minY; y <= maxY; y++)
					{
						if (x < 0 || x >= nodes.GetLength (0)) continue;
						if (y < 0 || y >= nodes.GetLength (1)) continue;

						nodes[x, y].EvadeCharacter = true;
					}
				}
			}

			// Right fill
			if (farRightX != centreNode.GridX)
			{
				for (int x = centreNode.GridX; x <= farRightX; x++)
				{
					float yLerp = (float) (x - centreNode.GridX) / (float) (farRightX - centreNode.GridX);
					int maxY = (int) Mathf.Lerp (farTopY, farRightY, yLerp);
					int minY = (int) Mathf.Lerp (farBottomY, farRightY, yLerp);

					for (int y = minY; y <= maxY; y++)
					{
						if (x < 0 || x >= nodes.GetLength (0)) continue;
						if (y < 0 || y >= nodes.GetLength (1)) continue;
						
						nodes[x, y].EvadeCharacter = true;
					}
				}
			}
		}

		#endregion


		#region PrivateFunctions

		private Vector3 InverseTransformPoint (Vector2 position)
		{
			return Collider.transform.InverseTransformPoint (position);
		}


		public bool Approximately (float f1, float f2)
		{
			if (f1 > f2)
			{
				float t = f1;
				f1 = f2;
				f2 = t;
			}

			return (f2 - f1) < 0.0001f;
		}


		private bool IntersectLineSegment2D (Vector2 p1, Vector2 p2start, Vector2 p2end)
		{
			if (p1 == p2start || p1 == p2end)
			{
				return true;
			}

			if ((p1.x < p2start.x && p1.x < p2end.x) ||
				(p1.x > p2start.x && p1.x > p2end.x) ||
				(p1.y < p2start.y && p1.y < p2end.y) ||
				(p1.y > p2start.y && p1.y > p2end.y))
			{
				return false;
			}

			if (p1.y == p2start.y && p1.y == p2end.y && p1.x >= Mathf.Min (p2start.x, p2end.x) && p1.x <= Mathf.Max (p2start.x, p2end.x))
			{
				return true;
			}

			if (p1.x == p2start.x && p1.x == p2end.x && p1.y >= Mathf.Min (p2start.y, p2end.y) && p1.y <= Mathf.Max (p2start.y, p2end.y))
			{
				return true;
			}

			float gradient = (p2end.y - p2start.y) / (p2end.x - p2start.x);
			float intercept = p2start.y;

			float xShift = p2start.x;
			p2start.x -= xShift;
			p1.x -= xShift;
			p2end.x -= xShift;

			float y = gradient * p1.x + intercept;
			return Approximately (y, p1.y);
		}



		private float IntersectLineSegments2D (Vector2 p1start, Vector2 p1end, Vector2 p2start, Vector2 p2end)
		{
			if (p1start == p2start || p1start == p2end)
			{
				return 0f;
			}

			if (p1end == p2start || p1end == p2end)
			{
				return 1f;
			}

			if ((p1start.x < p2start.x && p1start.x < p2end.x && p1end.x < p2start.x && p1end.x < p2end.x) ||
				(p1start.x > p2start.x && p1start.x > p2end.x && p1end.x > p2start.x && p1end.x > p2end.x) ||
				(p1start.y < p2start.y && p1start.y < p2end.y && p1end.y < p2start.y && p1end.y < p2end.y) ||
				(p1start.y > p2start.y && p1start.y > p2end.y && p1end.y > p2start.y && p1end.y > p2end.y))
			{
				return -1f;
			}

			Vector2 line1Relative = p1end - p1start;
			Vector2 line2Relative = p2end - p2start;
			Vector2 startRelative = p2start - p1start;

			float cross_rs = CrossProduct2D (line1Relative, line2Relative);

			if (Approximately (cross_rs, 0f))
			{
				if (Approximately (CrossProduct2D (startRelative, line1Relative), 0f))
				{
					float rdotr = Vector2.Dot (line1Relative, line1Relative);
					float sdotr = Vector2.Dot (line2Relative, line1Relative);

					float t0 = Vector2.Dot (startRelative, line1Relative / rdotr);
					float t1 = t0 + sdotr / rdotr;
					if (sdotr < 0)
					{
						Swap (ref t0, ref t1);
					}

					if (t0 <= 1 && t1 >= 0)
					{
						float line1Amount = Mathf.Lerp (Mathf.Max (0, t0), Mathf.Min (1, t1), 0.5f);
						return line1Amount;
					}
				}
			}
			else
			{
				float line1Amount = CrossProduct2D (startRelative, line2Relative) / cross_rs;
				float line2Amount = CrossProduct2D (startRelative, line1Relative) / cross_rs;
				if (line1Amount >= 0f && line1Amount <= 1f && line2Amount >= 0f && line2Amount <= 1f)
				{
					return line1Amount;
				}
			}

			return -1f;
		}


		private void Swap<T> (ref T lhs, ref T rhs)
		{
			T temp = lhs;
			lhs = rhs;
			rhs = temp;
		}


		private float CrossProduct2D (Vector2 a, Vector2 b)
		{
			return a.x * b.y - b.x * a.y;
		}

		#endregion


		#region GetSet

		public int MaxSize { get { return gridSizeX * gridSizeY; } }

		#endregion

	}

}
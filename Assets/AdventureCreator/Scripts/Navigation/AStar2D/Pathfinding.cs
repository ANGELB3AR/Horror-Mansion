using System.Collections.Generic;
using UnityEngine;

namespace AC.AStar2D
{

	public class Pathfinding
	{

		#region Variables

		private Node[] neighbourCache = new Node[MaxNeighbours];
		private const int MaxNeighbours = 8;

		#endregion


		#region PublicFunctions

		public Vector3[] FindPath (Vector3 startPosition, Vector3 targetPosition, Grid2D grid)
		{
			Node startNode = grid.PositionToNode (startPosition);
			Node targetNode = grid.PositionToNode (targetPosition);

			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst ();
				closedSet.Add (currentNode);

				if (currentNode == targetNode)
				{
					List<Node> nodes = RetracePath (startNode, targetNode);
					return NodesToPoints (grid, startNode, nodes);
				}

				int numNeighbours = grid.GetNeighbours (currentNode, ref neighbourCache);
				for (int i = 0; i < numNeighbours; i++)
				{
					Node neighbour = neighbourCache[i];
					if (!neighbour.IsWalkable) continue;

					int tentativeGCost = currentNode.gCost + GetDistance (currentNode, neighbour);
					if (closedSet.Contains (neighbour) && tentativeGCost >= neighbour.gCost) continue;
					
					int newCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = GetDistance (neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains (neighbour))
						{
							openSet.Add (neighbour);
						}
					}
				}
			}
			return new Vector3[0];
		}

		#endregion


		#region PrivateFunctions

		private int GetDistance (Node nodeA, Node nodeB)
		{
			int distanceX = Mathf.Abs (nodeA.GridX - nodeB.GridX);
			int distanceY = Mathf.Abs (nodeA.GridY - nodeB.GridY);

			if (distanceX > distanceY)
			{
				return 14 * distanceY + 10 * (distanceX - distanceY);
			}
			return 14 * distanceX + 10 * (distanceY - distanceX);
		}


		private List<Node> RetracePath (Node startNode, Node endNode)
		{ 
			List<Node> path = new List<Node> ();

			Node currentNode = endNode;
			while (currentNode != startNode)
			{
				path.Add (currentNode);
				currentNode = currentNode.parent;
			}

			path.Reverse ();
			return path;
		}


		public bool Approximately (float f1, float f2)
		{
			if (f1 > f2)
			{
				float t = f1;
				f1 = f2;
				f2 = t;
			}

			return (f2 - f1) < 0.1f;
		}


		private void DrawPath (List<Node> nodes, Color color)
		{
			for (int i = 0; i < nodes.Count -1; i++)
			{
				Debug.DrawLine (nodes[i].Position, nodes[i+1].Position, color);
			}
		}


		private void DrawPath (List<Vector3> nodes, Color color)
		{
			for (int i = 0; i < nodes.Count -1; i++)
			{
				Debug.DrawLine (nodes[i], nodes[i+1], color);
			}
		}


		private void PullStrings (Grid2D grid, ref List<Node> nodes)
		{
			if (nodes.Count <= 2)
			{
				return;
			}

			for (int i = 0; i < nodes.Count - 2; i++)
			{
				if (PathIsClear (grid, nodes[i], nodes[i+2]))
				{
					nodes.RemoveAt (i+1);
					i--;
				}
			}
		}


		private bool PathIsClear (Grid2D grid, Node start, Node end)
		{
			if (start == end)
			{
				return start.IsWalkable;
			}

			if (start.GridX == end.GridX)
			{
				int minY = Mathf.Min (start.GridY, end.GridY);
				int maxY = Mathf.Max (start.GridY, end.GridY);

				for (int y = minY; y <= maxY; y++)
				{
					if (!grid.GridToNode (start.GridX, y).IsWalkable)
					{
						return false;
					}
				}
				return true;
			}

			if (start.GridY == end.GridY)
			{
				int minX = Mathf.Min (start.GridX, end.GridX);
				int maxX = Mathf.Max (start.GridX, end.GridX);

				for (int x = minX; x <= maxX; x++)
				{
					if (!grid.GridToNode (x, start.GridY).IsWalkable)
					{
						return false;
					}
				}
				return true;
			}
			
			if (end.GridX < start.GridX)
			{
				// Make sure start is on left
				Node temp = new Node (end);
				end = start;
				start = temp;
			}

			float m = ((float) end.GridY - (float) start.GridY) / ((float) end.GridX - (float) start.GridX);
			float c = (float) start.GridY - (m * (float) start.GridX);

			for (int x = start.GridX + 1; x < end.GridX - 1; x++)
			{
				float y1 = m * ((float) x - 0.5f) + c;
				float y2 = m * ((float) x + 0.5f) + c;

				if (y1 > y2)
				{
					// Make sure y1 < y2
					float temp = y1;
					y1 = y2;
					y2 = temp;
				}
				
				int _y1 = Mathf.FloorToInt (y1);
				int _y2 = Mathf.CeilToInt (y2);
				
				for (int y = _y1; y <= _y2; y++)
				{
					Node node = grid.GridToNode (x, y);
					if (node == null || !node.IsWalkable)
					{
						return false;
					}
				}
			}
			return true;
		}


		private Vector3[] NodesToPoints (Grid2D grid, Node startNode, List<Node> nodes)
		{
			//DrawPath (nodes, Color.red);

			if (nodes.Count > 0)
			{
				float startSqrDistance = (startNode.Position - nodes[0].Position).sqrMagnitude;
				if (startSqrDistance <= (KickStarter.settingsManager.GetDestinationThreshold () * KickStarter.settingsManager.GetDestinationThreshold ()))
				{
					nodes.RemoveAt (0);
				}
			}

			nodes.Insert (0, startNode);
			PullStrings	(grid, ref nodes);

			if (nodes[0] == startNode)
			{
				nodes.RemoveAt (0);
			}
			
			Vector3[] points = new Vector3[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				points[i] = nodes[i].Position;
			}

			return points;
		}

		#endregion

	}

}
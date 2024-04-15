using UnityEngine;

namespace AC.AStar2D
{

	public class Node : IHeapItem<Node>
	{

		#region Variables

		public readonly Vector2 Position;
		private bool isWalkable;
		private bool evadeCharacter;
		public readonly int GridX;
		public readonly int GridY;

		public Node parent;
		public int gCost, hCost;

		private int heapIndex;

		#endregion


		#region Constructors

		public Node (Vector2 position, int gridX, int gridY, bool _isWalkable)
		{
			Position = position;
			GridX = gridX;
			GridY = gridY;
			isWalkable = _isWalkable;
		}


		public Node (Node node)
		{
			Position = node.Position;
			GridX = node.GridX;
			GridY = node.GridY;
			isWalkable = node.isWalkable;
			evadeCharacter = node.evadeCharacter;
			parent = node.parent;
			gCost = node.gCost;
			hCost = node.hCost;
			heapIndex = node.heapIndex;
		}

		#endregion


		#region PublicFunctions

		public int CompareTo (Node nodeToCompare)
		{
			int compare = FCost.CompareTo (nodeToCompare.FCost);
			if (compare == 0)
			{
				compare = hCost.CompareTo (nodeToCompare.hCost);
			}
			return -compare;
		}


		public override string ToString ()
		{
			return "[" + GridX + ", " + GridY + "]";
		}

		#endregion


		#region GetSet

		public int FCost { get { return gCost + hCost; } }
		public bool EvadeCharacter { set { evadeCharacter = value; } }

		public bool IsWalkable
		{
			get
			{
				return isWalkable && !evadeCharacter;
			}
			set
			{
				isWalkable = value;
			}
		}


		public int HeapIndex
		{
			get
			{
				return heapIndex;
			}
			set
			{
				heapIndex = value;
			}
		}

		#endregion

	}

}
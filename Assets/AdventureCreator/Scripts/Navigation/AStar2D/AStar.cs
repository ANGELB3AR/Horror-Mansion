using UnityEngine;

namespace AC.AStar2D
{

	public class AStar : MonoBehaviour
	{

		#region Variables

		[SerializeField] private float cellWidth = 0.2f;
		[SerializeField] private Collider2D navMesh = null;
		private readonly Pathfinding pathfinding = new Pathfinding ();
		private Grid2D grid;

		#endregion

		#region UnityStandards

		private void Start ()
		{
			grid = new Grid2D (navMesh, CellWidth, CellHeight);
		}


		private void OnDrawGizmosSelected ()
		{
			DrawGizmos (navMesh);
		}

		#endregion


		#region PublicFunctions

		public Vector3[] FindPath (Vector3 startPosition, Vector3 targetPosition)
		{
			return pathfinding.FindPath (startPosition, targetPosition, grid);
		}

		#endregion


		#region PrivateFunctions

		private void DrawGizmos (Collider2D _collider)
		{
			if (_collider == null) return;

			Gizmos.DrawWireCube (_collider.bounds.center, _collider.bounds.size);

			if (grid != null)
			{
				grid.DrawGizmos ();
				return;
			}

			int gridSizeX = Mathf.RoundToInt (_collider.bounds.size.x / CellWidth);
			int gridSizeY = Mathf.RoundToInt (_collider.bounds.size.y / CellHeight);

			Vector2 bottomLeft = (Vector2) _collider.bounds.center - (Vector2.right * _collider.bounds.size.x / 2f) - (Vector2.up * _collider.bounds.size.y / 2f);

			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					Vector2 position = bottomLeft + Vector2.right * (x * CellWidth + CellWidth * 0.5f) + Vector2.up * (y * CellHeight + CellHeight * 0.5f);
					Gizmos.DrawWireCube (position, new Vector2 (CellWidth, CellHeight));
				}
			}
		}

		#endregion


		#region GetSet

		public float CellWidth { get { return cellWidth; } }
		public float CellHeight { get { return cellWidth; } }
		public Grid2D Grid { get { return grid; } }

		#endregion

	}

}
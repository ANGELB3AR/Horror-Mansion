/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RandomMarker.cs"
 * 
 *	A component used to create reference transforms, as needed by the "Character: Move to point" Action and pthers.
 *	When a position is requested, it will pick a random point within its collider, provided it has attached either a BoxCollider, SphereCollider, BoxCollider2D, or CircleCollider2D.
 * 
 */
 
using UnityEngine;

namespace AC
{

	/**
	 * A component used to create reference transforms, as needed by the "Character: Move to point" Action and pthers.
	 * When a position is requested, it will pick a random point within its collider, provided it has attached either a BoxCollider, SphereCollider, BoxCollider2D, or CircleCollider2D.
	 */
	public class RandomMarker : Marker
	{

		#region GetSet

		public override Vector3 Position
		{
			get
			{
				BoxCollider boxCollider = GetComponent<BoxCollider> ();
				if (boxCollider)
				{
					Vector3 extents = boxCollider.size / 2f;
					Vector3 localPoint = new Vector3
					(
						Random.Range (-extents.x, extents.x),
						Random.Range (-extents.y, extents.y),
						Random.Range (-extents.z, extents.z)
					);
					localPoint += boxCollider.center;

					return Transform.TransformPoint (localPoint);
				}

				SphereCollider sphereCollider = GetComponent<SphereCollider> ();
				if (sphereCollider)
				{
					Vector3 localPoint = new Vector3
					(
						Random.Range (0f, sphereCollider.radius * Mathf.Sqrt (Random.Range (0f, 1f))),
						Random.Range (0f, sphereCollider.radius * Mathf.Sqrt (Random.Range (0f, 1f))),
						Random.Range (0f, sphereCollider.radius * Mathf.Sqrt (Random.Range (0f, 1f)))
					);
					localPoint += sphereCollider.center;

					return Transform.TransformPoint (localPoint);
				}

				BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D> ();
				if (boxCollider2D)
				{
					Vector2 extents = boxCollider2D.size * 0.5f;
					Vector2 localPoint = new Vector2 (Random.Range (-extents.x, extents.x), Random.Range (-extents.y, extents.y));
					localPoint += boxCollider2D.offset;

					Vector2 randomPoint = Transform.TransformPoint (localPoint);
					return new Vector3 (randomPoint.x, randomPoint.y, Transform.position.z);
				}

				CircleCollider2D circleCollider2D = GetComponent<CircleCollider2D> ();
				if (circleCollider2D)
				{
					float angle = Random.Range (0, Mathf.PI * 2);
					float distance = circleCollider2D.radius * Mathf.Sqrt (Random.Range (0f, 1f)); // sqrt for even distribution
			
					Vector2 localPoint = new Vector2 (distance * Mathf.Cos (angle), distance * Mathf.Sin (angle));
					localPoint += circleCollider2D.offset;
			
					Vector2 randomPoint = Transform.TransformPoint (localPoint);
					return new Vector3 (randomPoint.x, randomPoint.y, Transform.position.z);
				}
				
				ACDebug.LogWarning ("RandomMaker " + gameObject.name + " cannot get a random position - a BoxCollider, BoxCollider2D, SphereCollider or CircleCollider2D is required.", this);
				return Transform.position;
			}
			set
			{
				Transform.position = value;
			}
		}

		#endregion

	}

}
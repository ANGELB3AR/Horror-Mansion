﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"EditorZoomArea.cs"
 * 
 *	This script was written by Martin Ecker, and
 *  is freely available at: http://martinecker.com/martincodes/unity-editor-window-zooming/
 * 
 */

using UnityEngine;

namespace AC
{

	public static class RectExtensions
	{

		public static Vector2 TopLeft (this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}


		public static Vector2 BottomLeft (this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMax);
		}


		public static Vector2 TopRight (this Rect rect)
		{
			return new Vector2(rect.xMax, rect.yMin);
		}


		public static Vector2 BottomRight (this Rect rect)
		{
			return new Vector2(rect.xMax, rect.yMax);
		}


		public static Rect ScaleSizeBy (this Rect rect, float scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}


		public static Rect ScaleSizeBy (this Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}


		public static Rect ScaleSizeBy (this Rect rect, Vector2 scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}


		public static Rect ScaleSizeBy (this Rect rect, Vector2 scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale.x;
			result.xMax *= scale.x;
			result.yMin *= scale.y;
			result.yMax *= scale.y;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}

	}


	public class EditorZoomArea
	{

		private const float kEditorWindowTabHeight = 21.0f;
		private static Matrix4x4 _prevGuiMatrix;


		public static void Begin (float zoomScale, Rect screenCoordsArea)
		{
			GUI.EndGroup ();

			Rect clippedArea = screenCoordsArea.ScaleSizeBy (1.0f / zoomScale, screenCoordsArea.TopLeft ());
			clippedArea.y += kEditorWindowTabHeight;
			GUI.BeginGroup (clippedArea);
			
			_prevGuiMatrix = GUI.matrix;
			Matrix4x4 translation = Matrix4x4.TRS (clippedArea.TopLeft (), Quaternion.identity, Vector3.one);
			Matrix4x4 scale = Matrix4x4.Scale (new Vector3 (zoomScale, zoomScale, 1.0f));
			GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

			//GUI.BeginGroup (new Rect (0, 0, CanvasWidth / zoom, CanvasHeight / zoom - 44));

			//return clippedArea;
		}

		
		public static void End (Vector2 originalWindowSize)
		{
			//GUI.EndGroup ();

			GUI.matrix = _prevGuiMatrix;
			GUI.EndGroup ();

			GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, ACScreen.width, ACScreen.height));
		}

	}

}
using UnityEngine;

namespace LunraGames
{
	public static class RectTransformExtensions
	{
		static Vector3 WorldCorner(this RectTransform transform, int corner)
		{
			Vector3[] corners = new Vector3[4];
			transform.GetWorldCorners(corners);
			return corners[corner];
		}

		public static Vector3 MinWorldCorner(this RectTransform transform)
		{
			return transform.WorldCorner(1);
		}

		public static Vector3 MaxWorldCorner(this RectTransform transform)
		{
			return transform.WorldCorner(3);
		}
	}
}
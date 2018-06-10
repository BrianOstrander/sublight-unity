using UnityEngine;

namespace LunraGames
{
	public static class Vector3Extensions
	{
		public static Vector3 NewX(this Vector3 vector3, float x)
		{
			return new Vector3(x, vector3.y, vector3.z);
		}

		public static Vector3 NewY(this Vector3 vector3, float y)
		{
			return new Vector3(vector3.x, y, vector3.z);
		}

		public static Vector3 NewZ(this Vector3 vector3, float z)
		{
			return new Vector3(vector3.x, vector3.y, z);
		}

		/// <summary>
		/// Flattens the y to zero, and normalizes the result, perfect for an overhead camera
		/// </summary>
		/// <returns>The normalized result.</returns>
		/// <param name="vector3">Vector3.</param>
		public static Vector3 FlattenY(this Vector3 vector3)
		{
			return new Vector3(vector3.x, 0f, vector3.z).normalized;
		}
	}
}
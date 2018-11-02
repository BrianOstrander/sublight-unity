using UnityEngine;

namespace LunraGames
{
	public static class Vector3Extensions
	{
		public static bool Approximately(this Vector3 vector3, Vector3 other)
		{
			return Mathf.Approximately(vector3.x, other.x) && Mathf.Approximately(vector3.y, other.y) && Mathf.Approximately(vector3.z, other.z);
		}

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

		public static Vector3[] Add(this Vector3[] vector3s, Vector3 addition)
		{
			var result = new Vector3[vector3s.Length];
			for (var i = 0; i < vector3s.Length; i++) result[i] = vector3s[i] + addition;
			return result;
		}
	}
}
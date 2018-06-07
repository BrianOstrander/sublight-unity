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
	}
}
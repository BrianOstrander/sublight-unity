using UnityEngine;
using UnityEditor;
using System.IO;

namespace LunraGamesEditor
{
	public static class AssetDatabaseExtensions
	{
		public static string GetAbsoluteAssetPath(Object assetObject)
		{
			return Path.Combine(Project.Root.FullName, AssetDatabase.GetAssetPath(assetObject));
		}
	}
}
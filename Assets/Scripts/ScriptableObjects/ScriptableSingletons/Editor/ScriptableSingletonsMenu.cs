using UnityEditor;
using LunraGamesEditor;

namespace LunraGames.SpaceFarm
{
	public static class ScriptableSingletonsMenu
	{
		[MenuItem("Assets/Create/Singletons/Default Shader Globals")]
		static void CreateDefaultShaderGlobals()
		{
			AssetDatabaseExtensions.CreateObject<DefaultShaderGlobals>("DefaultShaderGlobals");
		}

		[MenuItem("Assets/Create/Singletons/Default Views")]
		static void CreateDefaultViews()
		{
			AssetDatabaseExtensions.CreateObject<DefaultViews>("DefaultViews");
		}
	}
}
using UnityEditor;
using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public static class ColorStyleObjectsMenu
	{
		[MenuItem("Assets/Create/Color Style")]
		static void CreateStyle()
		{
			AssetDatabaseExtensions.CreateObject<ColorStyleObject>("New Color Style");
		}
	}
}
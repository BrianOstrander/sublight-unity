using UnityEditor;
using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public static class CurveStyleObjectsMenu
	{
		[MenuItem("Assets/Create/CurveStyle")]
		static void CreateStyle()
		{
			AssetDatabaseExtensions.CreateObject<CurveStyleObject>("New Curve Style");
		}
	}
}
using UnityEditor;
using LunraGamesEditor;

namespace LunraGames.SpaceFarm
{
	public static class XButtonObjectsMenu
	{
		[MenuItem("Assets/Create/XButton Style")]
		static void CreateStyle()
		{
			AssetDatabaseExtensions.CreateObject<XButtonStyleObject>("New XButton Style");
		}

		[MenuItem("Assets/Create/XButton Sounds")]
		static void CreateSounds()
		{
			AssetDatabaseExtensions.CreateObject<XButtonSoundObject>("New XButton Sounds");
		}
	}
}
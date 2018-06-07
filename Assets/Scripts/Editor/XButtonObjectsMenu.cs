using UnityEditor;
using UnityEngine;
using System.IO;
using LunraGamesEditor;

namespace LunraGames.SpaceFarm
{
	public static class XButtonObjectsMenu
	{
		[MenuItem("Assets/Create/XButton Style")]
		static void CreateStyle()
		{
			CreateObject<XButtonStyleObject>("New XButton Style");
		}

		[MenuItem("Assets/Create/XButton Sounds")]
		static void CreateSounds()
		{
			CreateObject<XButtonSoundObject>("New XButton Sounds");
		}

		static void CreateObject<T>(string name)
			where T : ScriptableObject
		{
			var directory = SelectionExtensions.Directory();
			if (directory == null) 
			{
				EditorUtilityExtensions.DialogInvalid(Strings.Dialogs.Messages.SelectValidDirectory);
				return;
			}

			var config = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(config, Path.Combine(directory, name + ".asset"));
			Selection.activeObject = config;
		}
	}
}
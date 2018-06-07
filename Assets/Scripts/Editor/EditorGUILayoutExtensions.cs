using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LunraGames.SpaceFarm
{
	public class EditorGUILayoutExtensions {

		public static XButtonSoundBlock SoundBlockField(XButtonSoundBlock block) {
			GUILayout.BeginVertical();
			{
				block.DisabledSound = (AudioClip)EditorGUILayout.ObjectField("Disabled Sound", block.DisabledSound, typeof(AudioClip), false);
				block.EnteredSound = (AudioClip)EditorGUILayout.ObjectField("Entered Sound", block.EnteredSound, typeof(AudioClip), false);
				block.PressedSound = (AudioClip)EditorGUILayout.ObjectField("Pressed Sound", block.PressedSound, typeof(AudioClip), false);
				block.ExitedSound = (AudioClip)EditorGUILayout.ObjectField("Exited Sound", block.ExitedSound, typeof(AudioClip), false);
				block.HighlightedSound = (AudioClip)EditorGUILayout.ObjectField("Highlighted Sound", block.HighlightedSound, typeof(AudioClip), false);
			}
			GUILayout.EndVertical();
			return block;
		}
	}
}
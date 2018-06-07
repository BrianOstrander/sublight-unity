using UnityEngine;
using System;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public struct XButtonSoundBlock 
	{
		public AudioClip DisabledSound;
		public AudioClip PressedSound;
		public AudioClip EnteredSound;
		public AudioClip ExitedSound;
		public AudioClip HighlightedSound;
	}
}
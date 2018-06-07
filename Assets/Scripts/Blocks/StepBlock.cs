using System;

using UnityEngine.Video;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public class StepBlock
	{
		public int Index;
		public string PrimaryText;
		public string SecondaryText;
		public VideoClip Video;

		public StepBlock(int index = 0)
		{
			Index = index;
		}
	}
}
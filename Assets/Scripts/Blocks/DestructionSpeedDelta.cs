﻿using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct DestructionSpeedDelta
	{
		[JsonProperty] public readonly float Speed;
		[JsonProperty] public readonly DayTime Start;

		public DestructionSpeedDelta(float speed, DayTime start)
		{
			Speed = speed;
			Start = start;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GalaxyLabelBlock
	{
		public string Name;
		public LanguageStringModel Text;
		public GalaxyLabelTypes LabelType;
		public float Size;
		public Vector2 Begin;
		public Vector2 End;

	}
}
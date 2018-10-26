using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct FocusTransform
	{
		public readonly TweenBlock<float> Zoom;
		public readonly TweenBlock<float> NudgeZoom;
		public readonly LanguageStringModel FromScaleName;
		public readonly LanguageStringModel ToScaleName;
		public readonly Func<string> GetFromUnitCount;
		public readonly Func<string> GetToUnitCount;
		public readonly LanguageStringModel FromUnitType;
		public readonly LanguageStringModel ToUnitType;

		public FocusTransform(
			TweenBlock<float> zoom,
			TweenBlock<float> nudgeZoom,
			LanguageStringModel fromScaleName,
			LanguageStringModel toScaleName,
			Func<string> getFromUnitCount,
			Func<string> getToUnitCount,
			LanguageStringModel fromUnitType,
			LanguageStringModel toUnitType
		)
		{
			Zoom = zoom;
			NudgeZoom = nudgeZoom;
			FromScaleName = fromScaleName;
			ToScaleName = toScaleName;
			GetFromUnitCount = getFromUnitCount;
			GetToUnitCount = getToUnitCount;
			FromUnitType = fromUnitType;
			ToUnitType = toUnitType;
		}
	}
}
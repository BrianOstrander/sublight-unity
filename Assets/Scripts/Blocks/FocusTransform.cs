using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct FocusTransform
	{
		public readonly TweenBlock<float> Zoom;
		public readonly TweenBlock<float> NudgeZoom;

		public UniverseScales FromScale { get; private set; }
		public UniverseScales ToScale { get; private set; }
		public LanguageStringModel FromScaleName { get; private set; }
		public LanguageStringModel ToScaleName { get; private set; }
		public Func<string> GetFromUnitCount { get; private set; }
		public Func<string> GetToUnitCount { get; private set; }
		public LanguageStringModel FromUnitType { get; private set; }
		public LanguageStringModel ToUnitType { get; private set; }

		public FocusTransform(
			TweenBlock<float> zoom,
			TweenBlock<float> nudgeZoom,
			UniverseScales fromScale,
			UniverseScales toScale,
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

			FromScale = fromScale;
			ToScale = toScale;
			FromScaleName = fromScaleName;
			ToScaleName = toScaleName;
			GetFromUnitCount = getFromUnitCount;
			GetToUnitCount = getToUnitCount;
			FromUnitType = fromUnitType;
			ToUnitType = toUnitType;
		}

		public FocusTransform Duplicate(
			TweenBlock<float>? zoom = null,
			TweenBlock<float>? nudgeZoom = null
		)
		{
			return new FocusTransform(
				zoom.HasValue ? zoom.Value : Zoom,
				nudgeZoom.HasValue ? nudgeZoom.Value : NudgeZoom,
				FromScale,
				ToScale,
				FromScaleName,
				ToScaleName,
				GetFromUnitCount,
				GetToUnitCount,
				FromUnitType,
				ToUnitType
			);
		}

		public void SetLanguage(
			UniverseScales fromScale,
			UniverseScales toScale,
			LanguageStringModel fromScaleName,
			LanguageStringModel toScaleName,
			Func<string> getFromUnitCount,
			Func<string> getToUnitCount,
			LanguageStringModel fromUnitType,
			LanguageStringModel toUnitType
		)
		{
			FromScale = fromScale;
			ToScale = toScale;
			FromScaleName = fromScaleName;
			ToScaleName = toScaleName;
			GetFromUnitCount = getFromUnitCount;
			GetToUnitCount = getToUnitCount;
			FromUnitType = fromUnitType;
			ToUnitType = toUnitType;
		}
	}
}
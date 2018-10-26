using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct ZoomBlock
	{
		public enum States
		{
			Unknown = 0,
			Complete,
			ZoomingUp,
			ZoomingDown
		}

		public readonly float FromZoom;
		public readonly float ToZoom;
		public readonly float Zoom;
		public readonly States State;
		public readonly LanguageStringModel FromScaleName;
		public readonly LanguageStringModel ToScaleName;
		public readonly Func<string> GetFromUnitCount;
		public readonly Func<string> GetToUnitCount;
		public readonly LanguageStringModel FromUnitType;
		public readonly LanguageStringModel ToUnitType;
		public readonly float Progress;

		public ZoomBlock(
			float fromZoom,
			float toZoom,
			float zoom,
			LanguageStringModel fromScaleName,
			LanguageStringModel toScaleName,
			Func<string> getFromUnitCount,
			Func<string> getToUnitCount,
			LanguageStringModel fromUnitType,
			LanguageStringModel toUnitType,
			float progress = 1f
		)
		{
			FromZoom = fromZoom;
			ToZoom = toZoom;
			Zoom = zoom;
			FromScaleName = fromScaleName;
			ToScaleName = toScaleName;
			GetFromUnitCount = getFromUnitCount;
			GetToUnitCount = getToUnitCount;
			FromUnitType = fromUnitType;
			ToUnitType = toUnitType;

			if (Mathf.Approximately(ToZoom, Zoom)) State = States.Complete;
			else if (FromZoom < ToZoom) State = States.ZoomingUp;
			else State = States.ZoomingDown;

			Progress = progress;
		}
	}
}
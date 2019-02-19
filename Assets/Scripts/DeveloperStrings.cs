using UnityEngine;

namespace LunraGames.SubLight
{
	public static class DeveloperStrings
	{
		public class RatioColor
		{
			public Color Minimum;
			public Color Maximum;
			public bool Reflect;

			public RatioColor(
				Color minimum,
				Color maximum,
				bool reflect = false
			)
			{
				Minimum = minimum;
				Maximum = maximum;
				Reflect = reflect;
			}

			public Color Evaluate(float normal)
			{
				if (Reflect)
				{
					if (normal < 0.5f) normal = 1f - (normal / 0.5f);
					else if (0.5f < normal) normal = (normal - 0.5f) / 0.5f;
					else normal = 0f;
				}

				return Color.Lerp(Minimum, Maximum, normal);
			}
		}

		public enum RatioThemes
		{
			Unknown = 0,
			Raw = 10,
			ProgressBar = 20
		}

		static void RatioShared
		(
			float normal,
			RatioColor color,
			out string colorTagBegin,
			out string colorTagEnd
		)
		{
			colorTagBegin = string.Empty;
			colorTagEnd = string.Empty;

			if (color != null)
			{
				colorTagBegin = "<color=#" + ColorUtility.ToHtmlStringRGB(color.Evaluate(normal)) + ">";
				colorTagEnd = "</color>";
			}
		}

		public static string GetRatio(
			float value,
			float minimum,
			float maximum,
			RatioThemes theme = RatioThemes.Raw,
			RatioColor color = null,
			string format = null,
			int progressBarLimit = 10
		)
		{
			var delta = maximum - minimum;
			var normal = (value - minimum) / delta;
			var colorTagBegin = string.Empty;
			var colorTagEnd = string.Empty;

			RatioShared(
				normal,
				color,
				out colorTagBegin,
				out colorTagEnd
			);

			if (string.IsNullOrEmpty(format)) format = "N0";

			switch (theme)
			{
				case RatioThemes.Unknown:
				case RatioThemes.Raw:
					return colorTagBegin + value.ToString(format) + " / " + maximum.ToString(format) + colorTagEnd;
				case RatioThemes.ProgressBar:
					var progressBarResult = colorTagBegin+"|";

					progressBarResult += "==";
					progressBarLimit -= 2;

					var progressBarHasReachedLimit = false;
					for (var i = 0; i < progressBarLimit; i++)
					{
						var previousLimit = progressBarHasReachedLimit;
						progressBarHasReachedLimit = Mathf.FloorToInt(progressBarLimit * normal) <= i;
						if (progressBarHasReachedLimit != previousLimit) progressBarResult += colorTagEnd;

						progressBarResult += "=";
					}
					progressBarResult += "|";
					if (!progressBarHasReachedLimit) progressBarResult += colorTagEnd;

					return progressBarResult;
				default:
					Debug.LogError("Unrecognized RatioTheme: " + theme);
					return string.Empty;
			}
		}
	}
}
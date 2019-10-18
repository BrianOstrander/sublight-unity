using UnityEngine;
using UnityEngine.UI;

namespace LunraGames.SubLight.Views
{
	public class SystemCurveIdleMoodProgressLeaf : MonoBehaviour
	{
		public CanvasGroup HashGroup;
		public FloatRange Range;
		public float RangeOffset;
		public RectTransform Slider;

		public Graphic[] PrimaryColors;
	}
}
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

namespace LunraGames.SubLight
{
	public static class GraphicExtensions
	{
		/// <summary>
		/// Correctly sets the color of a graphic, even if it's a TextMeshProUGUI component.
		/// </summary>
		/// <remarks>
		/// Hopefully this can be removed when a fix is applied to TMPro, but until then, 
		/// use this to set the color of graphics that may or may not be TMPro elements.
		/// </remarks>
		/// <param name="graphic">Graphic.</param>
		/// <param name="color">Color.</param>
		public static void SetColor(this Graphic graphic, Color color)
		{
			//if (graphic is TextMeshProUGUI) (graphic as TextMeshProUGUI).color = color;
			//else if (graphic is TextMeshPro) (graphic as TextMeshPro).color = color;
			//else graphic.color = color;
		}
	}
}
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ToolbarButtonLeaf : MonoBehaviour
	{
		public GameObject ButtonLabelArea;
		public TextMeshProUGUI ButtonLabel;
		public XButton Button;
		public Image ButtonImage;

		public CanvasGroup HighlightedArea;

		public CanvasGroup UnHighlightedSelectedArea;
		public CanvasGroup UnHighlightedUnSelectedArea;

		public Transform ScalableArea;

		public XButtonLeaf HaloLeaf;
		public XButtonLeaf BackgroundLeaf;
	}
}
using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public abstract class ConversationLeaf : MonoBehaviour
	{
		public TextMeshProUGUI MessageLabel;

		public RectTransform RootCanvas;
		public RectTransform SizeArea;
		public CanvasGroup CanvasGroup;
		public CanvasGroup Group;
	}
}
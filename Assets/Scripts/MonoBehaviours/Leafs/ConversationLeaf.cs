using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public abstract class ConversationLeaf : MonoBehaviour
	{
		public TextMeshProUGUI MessageLabel;
		public RectTransform RootArea;
		public RectTransform SizeArea;
		public CanvasGroup Group;
	}
}
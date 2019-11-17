using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ModuleSwapEntryLeaf : MonoBehaviour
	{
		public TextMeshProUGUI NameLabel;
		public TextMeshProUGUI TypeLabel;
		public TextMeshProUGUI SeverityLabel;
		
		public Image SeverityBackground;
		
		public CanvasGroup DownControlGroup;
		public CanvasGroup UpControlGroup;
		
		public XButton Button;
	}
}
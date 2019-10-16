using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ModuleBrowserEntryLeaf : MonoBehaviour
	{
		public TextMeshProUGUI NameLabel;
		public TextMeshProUGUI TypeLabel;
		public Graphic[] SeverityGraphicsPrimary;
		public Graphic[] SeverityGraphicsSecondary;

		public XButton Button;
	}
}
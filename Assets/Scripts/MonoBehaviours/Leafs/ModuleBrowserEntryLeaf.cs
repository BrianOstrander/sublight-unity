using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ModuleBrowserEntryLeaf : MonoBehaviour
	{
		public TextMeshProUGUI NameLabel;
		public TextMeshProUGUI TypeLabel;
		public TextMeshProUGUI SeverityLabel;
		public XButtonLeaf SeverityPrimary;
		public XButtonLeaf SeveritySecondary;
		public GameObject SelectedArea;

		public XButton Button;
	}
}
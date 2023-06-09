using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ModuleSwapModuleEntryLeaf : MonoBehaviour
	{
		public TextMeshProUGUI NameLabel;
		public TextMeshProUGUI TypeLabel;
		public TextMeshProUGUI SeverityLabel;
		
		public Image Background;

		public GameObject[] UpControls;
		public GameObject[] DownControls;
		public GameObject[] ActiveControls;
		public GameObject[] InactiveControls;
		
		public XButton Button;
		
		public Graphic[] PrimaryColors;
		public Graphic[] SecondaryColors;
	}
}
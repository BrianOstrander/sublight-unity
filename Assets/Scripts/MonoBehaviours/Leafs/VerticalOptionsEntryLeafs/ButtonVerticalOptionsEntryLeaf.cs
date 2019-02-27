using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ButtonVerticalOptionsEntryLeaf : VerticalOptionsEntryLeaf
	{
		public TextMeshProUGUI PrimaryLabel;
		public TextMeshProUGUI SecondaryLabel;
		public XButton Button;

		public GameObject[] DisabledAreas;
		public GameObject[] EnabledAreas;

		public XButtonLeaf[] LabelAreas;
		public XButtonLeaf[] BackgroundAreas;
		public XButtonLeaf[] HighlightAreas;

		public Graphic[] LabelNormalAreas;
	}
}

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ButtonVerticalOptionsEntryLeaf : VerticalOptionsEntryLeaf
	{
		public TextMeshProUGUI Label;
		public XButton Button;

		public GameObject[] DisabledAreas;
		public GameObject[] EnabledAreas;

		public XButtonLeaf[] LabelAreas;
		public XButtonLeaf[] BackgroundAreas;
		public XButtonLeaf[] HighlightAreas;
	}
}

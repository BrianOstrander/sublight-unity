using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ButtonOptionsMenuEntryLeaf : OptionsMenuEntryLeaf
	{
		public TextMeshProUGUI Label;
		public XButton Button;
		public CanvasGroup Group;

		public XButtonLeaf LabelArea;
		public XButtonLeaf BulletArea;
		public XButtonLeaf BulletDisabledArea;
		public XButtonLeaf UnderlineArea;
		public XButtonLeaf BackgroundArea;
	}
}

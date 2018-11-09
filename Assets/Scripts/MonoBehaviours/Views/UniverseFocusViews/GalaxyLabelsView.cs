using UnityEngine;

using LunraGames;

namespace LunraGames.SubLight.Views
{
	public struct GalaxyLabelBlock
	{
		public string Text;
		public string GroupId;
		public Vector2 BeginAnchorNormalized;
		public Vector2 EndAnchorNormalized;
		public TextCurveBlock CurveInfo;
	}

	public class GalaxyLabelsView : UniverseScaleView, IGalaxyLabelsView
	{
		struct LabelBlock
		{
			public GalaxyLabelBlock Block;
			public GalaxyLabelLeaf Instance;
		}

		[SerializeField]
		GalaxyLabelLeaf labelPrefab;
		[SerializeField]
		GameObject labelArea;

		LabelBlock[] labelBlocks = new LabelBlock[0];

		public GalaxyLabelBlock[] Labels
		{
			set
			{
				labelArea.transform.ClearChildren();
				if (value == null || value.Length == 0)
				{
					labelBlocks = new LabelBlock[0];
					return;
				}
				labelBlocks = new LabelBlock[value.Length];

				for (var i = 0; i < value.Length; i++)
				{
					var curr = value[i];
					var result = new LabelBlock();
					result.Block = curr;
					result.Instance = labelArea.InstantiateChild(labelPrefab, setActive: true);
					result.Instance.GetComponent<RectTransform>().sizeDelta = Vector2.one;
					var label = result.Instance.Label;
					label.SetBeginEndAnchorNormalized(curr.BeginAnchorNormalized, curr.EndAnchorNormalized);
					label.CurveInfo = curr.CurveInfo;
					label.Text = curr.Text;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			labelPrefab.gameObject.SetActive(false);
			Labels = null;
		}
	}

	public interface IGalaxyLabelsView : IUniverseScaleView
	{
		GalaxyLabelBlock[] Labels { set; }
	}
}
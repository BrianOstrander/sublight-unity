﻿using UnityEngine;

using LunraGames;

namespace LunraGames.SubLight.Views
{
	public struct GalaxyLabelBlock
	{
		public string Text;
		public string GroupId;
		public UniverseScales Scale;
		public int SliceLayer;
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
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float yMinimum;
		[SerializeField]
		float ySeparation;
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		GalaxyLabelLeaf labelPrefab;
		[SerializeField]
		GameObject labelArea;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		[SerializeField]
		Color[] galacticColorsBySliceLayer = new Color[0];
		[SerializeField]
		Color[] quadrantColorsBySliceLayer = new Color[0];

		Color GetGalacticColor(int sliceLayer)
		{
			if (galacticColorsBySliceLayer.Length == 0) return Color.white;
			return galacticColorsBySliceLayer[Mathf.Min(sliceLayer, galacticColorsBySliceLayer.Length - 1)];
		}

		Color GetQuadrantColor(int sliceLayer)
		{
			if (quadrantColorsBySliceLayer.Length == 0) return Color.white;
			return quadrantColorsBySliceLayer[Mathf.Min(sliceLayer, quadrantColorsBySliceLayer.Length - 1)];
		}

		public GalaxyLabelBlock[] Labels
		{
			set
			{
				labelArea.transform.ClearChildren();
				if (value == null || value.Length == 0)
				{
					return;
				}

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
					label.YOffset = yMinimum + (curr.SliceLayer * ySeparation);
					label.Text = curr.Text;

					switch(curr.Scale)
					{
						case UniverseScales.Galactic:
							label.Color = GetGalacticColor(curr.SliceLayer).NewA(0f);
							break;
						case UniverseScales.Quadrant:
							label.Color = GetQuadrantColor(curr.SliceLayer).NewA(0f);
							break;
						default:
							Debug.LogError("Unrecognized scale " + curr.Scale);
							break;
					}
				}
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			labelPrefab.gameObject.SetActive(false);
			Labels = null;
			DefaultOpacity = 0f;
			ClearOpacity();
		}
	}

	public interface IGalaxyLabelsView : IUniverseScaleView
	{
		GalaxyLabelBlock[] Labels { set; }
	}
}
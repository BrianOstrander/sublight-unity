﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

using LunraGames;
using UnityEngine.EventSystems;

namespace LunraGames.SubLight.Views
{
	public class TextCurve : MonoBehaviour
	{
		[Serializable]
		struct LabelFont
		{
			public GalaxyLabelStyles LabelStyle;
			public TMP_FontAsset Font;
			public FontStyles Style;
		}

		[SerializeField]
		string text;
		[SerializeField]
		TextCurveBlock block;
		[SerializeField]
		bool flipNormals;
		[SerializeField]
		Vector3 beginAnchorLocal;
		[SerializeField]
		Vector3 endAnchorLocal;

		[SerializeField]
		int samplingMinimum = 16;

		[SerializeField]
		LabelFont[] fonts = new LabelFont[0];
		[SerializeField]
		TextMeshProUGUI labelPrefab;
		[SerializeField]
		GameObject labelArea;

		bool isStale;

		public TextCurveBlock CurveInfo
		{
			set
			{
				isStale = true;
				block = value;
			}
		}

		Vector3 BeginAnchorWorld
		{
			get { return transform.TransformPoint(beginAnchorLocal); }
			set { beginAnchorLocal = transform.InverseTransformPoint(value); isStale = true; }
		}

		Vector3 EndAnchorWorld
		{
			get { return transform.TransformPoint(endAnchorLocal); }
			set { endAnchorLocal = transform.InverseTransformPoint(value); isStale = true; }
		}

		public void SetBeginEndAnchorNormalized(Vector2 begin, Vector2 end)
		{
			var rectTransform = GetComponent<RectTransform>();
			Vector3 minWorldCorner;
			Vector3 maxWorldCorner;
			rectTransform.MinMaxWorldCorner(out minWorldCorner, out maxWorldCorner);
			var delta = maxWorldCorner - minWorldCorner;

			BeginAnchorWorld = minWorldCorner + new Vector3(delta.x * begin.x, 0f, delta.z * (1f - begin.y));
			EndAnchorWorld = minWorldCorner + new Vector3(delta.x * end.x, 0f, delta.z * (1f - end.y));
		}

		public string Text
		{
			set
			{
				isStale = value != text;
				text = value;
				if (isStale && enabled && gameObject.activeInHierarchy) UpdateText();
			}
			get { return text; }
		}

		void OnEnable()
		{
			UpdateText();
		}

		public void UpdateText(bool force = false)
		{
			if (!force)
			{
				if (!isStale) return;
			}
			isStale = false;

			labelArea.transform.ClearChildren(destroyImmediate: Application.isEditor && !Application.isPlaying);

			if (string.IsNullOrEmpty(Text)) return;

			var totalWidth = 0f;
			var characterCount = Text.Length;
			var labels = new List<TextMeshProUGUI>();
			var colorableLabels = new List<TextMeshProUGUI>();
			var fontBlock = fonts.FirstOrDefault(f => f.LabelStyle == block.LabelStyle);

			foreach (var currChar in Text)
			{
				var currLabel = labelArea.InstantiateChild(labelPrefab, setActive: true);

				currLabel.font = fontBlock.Font;
				currLabel.fontStyle = fontBlock.Style;
				currLabel.fontSize = block.FontSize;

				labels.Add(currLabel);

				currLabel.name = labelPrefab.name + " - " + currChar;
				if (char.IsWhiteSpace(currChar))
				{
					currLabel.text = "-";
					currLabel.color = Color.clear;
				}
				else
				{
					currLabel.text = currChar.ToString();
					colorableLabels.Add(currLabel);
				}
			}

			Canvas.ForceUpdateCanvases();

			var labelWidths = new List<float>();

			foreach (var currLabel in labels)
			{
				var size = currLabel.rectTransform.WorldCornerSize();
				labelWidths.Add(size.x);
				totalWidth += size.x;
			}

			if (1 < characterCount) totalWidth -= labelWidths.Last();

			var maxWidth = block.EvaluateLength(BeginAnchorWorld, EndAnchorWorld, samplingMinimum, characterCount);

			var widthRatio = totalWidth / maxWidth;

			var fontScalar = 1f;
			bool isShrinking = 1f < widthRatio;

				if (isShrinking)
			{
				fontScalar = 1f / widthRatio;
				totalWidth = maxWidth;
				Debug.Log(widthRatio+" - "+fontScalar);
			}

			// center it within the length of the curve
			var charAreaScalar = 1f - widthRatio;
			var buffer = (charAreaScalar * 0.5f);

			if (1 < characterCount) buffer -= ((totalWidth / (characterCount - 1)) * 0.25f);

			var currLength = 0f;

			for (var i = 0; i < characterCount; i++)
			{
				var currLabel = labels[i];
				currLabel.fontSize = currLabel.fontSize * fontScalar;

				currLength += labelWidths[i] * 0.5f * fontScalar;

				var progress = (currLength / totalWidth);

				if (!isShrinking) progress = buffer + (progress * widthRatio);

				Vector3 normal;
				var position = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, progress, flipNormals, out normal);
				currLabel.transform.position = position;
				currLabel.transform.LookAt(position + Vector3.down, normal);

				currLength += labelWidths[i] * 0.5f * fontScalar;
			}
		}

		void OnDrawGizmos()
		{
			var currBegin = block.Begin(BeginAnchorWorld, EndAnchorWorld);
			var currEnd = block.End(BeginAnchorWorld, EndAnchorWorld);

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(currBegin, 0.05f);
			Gizmos.DrawLine(currBegin, currBegin + (block.CurveUp(BeginAnchorWorld, EndAnchorWorld) * 0.15f));
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(currEnd, 0.05f);

			var currSampling = Mathf.Max(1, samplingMinimum);
			var lastPoint = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, 0f);

			var normalLength = transform.lossyScale.magnitude * 5f;

			for (var i = 0f; i < currSampling; i++)
			{
				var scalar = (i + 1f) / currSampling;
				Vector3 normal;
				var currPoint = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, scalar, flipNormals, out normal);
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(lastPoint, currPoint);
				Gizmos.color = Color.yellow.NewA(0.5f);
				Gizmos.DrawLine(currPoint, currPoint + (normal * normalLength));
				lastPoint = currPoint;
			}
		}
	}
}
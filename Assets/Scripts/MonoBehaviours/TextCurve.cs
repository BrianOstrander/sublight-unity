using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class TextCurve : MonoBehaviour
	{
		[Serializable]
		struct LabelFont
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public GalaxyLabelStyles LabelStyle;
			public TMP_FontAsset Font;
			public FontStyles Style;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		string text;
		[SerializeField]
		float yOffset;
		[SerializeField]
		Color color = Color.white;
		[SerializeField]
		TextCurveBlock block;
		[SerializeField]
		bool flipNormals;
		[SerializeField]
		bool cameraAligned;
		[SerializeField]
		float cameraDotThreshold;

		[SerializeField]
		Vector2 beginAnchorNormalized;
		[SerializeField]
		Vector2 endAnchorNormalized;

		[SerializeField]
		int samplingMinimum = 16;

		[SerializeField]
		LabelFont[] fonts = new LabelFont[0];
		[SerializeField]
		TextMeshProUGUI labelPrefab;
		[SerializeField]
		GameObject labelArea;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Vector3 beginAnchorLocal;
		Vector3 endAnchorLocal;
		bool isStale;
		Vector3 centerNormal;
		bool lastFlipNormals;

		float lastLossyScale;
		int staleDelay;

		bool wasOversized;

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
			set { beginAnchorLocal = transform.InverseTransformPoint(value); }
		}

		Vector3 EndAnchorWorld
		{
			get { return transform.TransformPoint(endAnchorLocal); }
			set { endAnchorLocal = transform.InverseTransformPoint(value); }
		}

		public float YOffset
		{
			get { return yOffset; }
			set { yOffset = value; isStale = true; }
		}

		public void SetBeginEndAnchorNormalized(Vector2 begin, Vector2 end)
		{
			beginAnchorNormalized = begin;
			endAnchorNormalized = end;
			isStale = true;
		}

		public Color Color
		{
			set
			{
				isStale = true;
				color = value;
			}
			get { return color; }
		}

		public string Text
		{
			set
			{
				isStale = isStale || value != text;
				text = value;
				if (Application.isPlaying || !Application.isEditor) return;
				if (isStale && enabled && gameObject.activeInHierarchy) UpdateText();
			}
			get { return text; }
		}

		void LateUpdate()
		{
			if (!Mathf.Approximately(lastLossyScale, transform.lossyScale.sqrMagnitude))
			{
				staleDelay = 1;
				isStale = true;
				lastLossyScale = transform.lossyScale.sqrMagnitude;
			}
			else if (cameraAligned && lastFlipNormals != GetFlipNormalsValue())
			{
				staleDelay = 0;
				isStale = true;
			}

			if (isStale)
			{
				if (0 < staleDelay)
				{
					staleDelay--;
					return;
				}
				UpdateText();
			}
		}

		public void UpdateText(bool force = false)
		{
			if (!force)
			{
				if (!isStale) return;
			}
			labelPrefab.gameObject.SetActive(false);
			isStale = false;

			var rectTransform = GetComponent<RectTransform>();
			Vector3 minWorldCorner;
			Vector3 maxWorldCorner;
			rectTransform.MinMaxWorldCorner(out minWorldCorner, out maxWorldCorner);
			var worldCornerDeltas = maxWorldCorner - minWorldCorner;

			BeginAnchorWorld = minWorldCorner + new Vector3(worldCornerDeltas.x * beginAnchorNormalized.x, YOffset, worldCornerDeltas.z * (1f - beginAnchorNormalized.y));
			EndAnchorWorld = minWorldCorner + new Vector3(worldCornerDeltas.x * endAnchorNormalized.x, YOffset, worldCornerDeltas.z * (1f - endAnchorNormalized.y));

			block.Evaluate(BeginAnchorWorld, EndAnchorWorld, 0.5f, flipNormals, out centerNormal);
			lastFlipNormals = GetFlipNormalsValue();
			// heeeere

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
				currLabel.color = Color;

				if (char.IsWhiteSpace(currChar))
				{
					currLabel.GetComponent<GraphicRadialLimiter>().enabled = false;
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

			wasOversized = isShrinking;

			if (isShrinking)
			{
				fontScalar = 1f / widthRatio;
				totalWidth = maxWidth;
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
				var position = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, progress, lastFlipNormals, out normal);
				currLabel.transform.position = position;
				currLabel.transform.LookAt(position + Vector3.down, normal);

				currLength += labelWidths[i] * 0.5f * fontScalar;
			}
		}

		bool GetFlipNormalsValue()
		{
			if (!cameraAligned || !Application.isPlaying) return flipNormals;

			if (Vector3.Dot(centerNormal, App.V.CameraForward.FlattenY()) < cameraDotThreshold)
			{
				// Camera is past the point at which we should flip the text.
				return !flipNormals;
			}
			return flipNormals;
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
			var lastPoint = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, 0f, flipNormals);

			var normalLength = transform.lossyScale.magnitude * 5f;

			for (var i = 0f; i < currSampling; i++)
			{
				var scalar = (i + 1f) / currSampling;
				Vector3 normal;
				var currPoint = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, scalar, flipNormals, out normal);
				Gizmos.color = wasOversized ? Color.red : Color.yellow;
				Gizmos.DrawLine(lastPoint, currPoint);
				Gizmos.color = (wasOversized ? Color.red : Color.yellow).NewA(0.5f);
				Gizmos.DrawLine(currPoint, currPoint + (normal * normalLength));
				lastPoint = currPoint;
			}

			if (!Application.isPlaying || App.V == null) return;

			Gizmos.color = Color.magenta;
			var currCenter = block.Evaluate(BeginAnchorWorld, EndAnchorWorld, 0.5f, lastFlipNormals);
			Gizmos.DrawLine(currCenter, currCenter + (centerNormal * normalLength * 2f));

			var camFlattening = App.V.CameraForward.FlattenY();
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(currCenter, currCenter + (camFlattening * normalLength * 2f));
		}
	}
}
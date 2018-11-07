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
		const float NormalSampleDelta = 0.05f;

		[SerializeField]
		string text;
		[SerializeField]
		AnimationCurve curve = AnimationCurveExtensions.Constant();
		[SerializeField]
		int samplingMinimum = 16;
		[SerializeField]
		float curveMaximum = 1f;
		[SerializeField]
		Vector3 beginAnchor;
		[SerializeField]
		Vector3 endAnchor;
		[SerializeField]
		bool flipAnchors;
		[SerializeField]
		bool flipBoundary;

		[SerializeField]
		TextMeshProUGUI labelPrefab;
		[SerializeField]
		GameObject labelArea;

		bool isStale;

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

		Vector3 Delta { get { return End - Begin; } }
		Vector3 Normal { get { return Delta.normalized; } }

		Vector3 BoundaryUp
		{
			get
			{
				return Quaternion.AngleAxis(flipBoundary ? 90f : -90f, Vector3.up) * Normal;
			}
		}

		Vector3 Begin { get { return flipAnchors ? endAnchor : beginAnchor; } }
		Vector3 End { get { return flipAnchors ? beginAnchor : endAnchor; } }

		Vector3 EvaluateLine(float progress)
		{
			return Begin + (Delta * progress);
		}

		Vector3 Evaluate(float progress)
		{
			return EvaluateLine(progress) + (BoundaryUp * (curve.Evaluate(progress) * curveMaximum));
		}

		Vector3 Evaluate(float progress, out Vector3 normal)
		{
			var beginSample = Evaluate(Mathf.Max(0f, progress - NormalSampleDelta));
			var endSample = Evaluate(Mathf.Min(1f, progress + NormalSampleDelta));
			normal = Quaternion.AngleAxis(-90f, Vector3.up) * (endSample - beginSample).normalized;
			return Evaluate(progress);
		}

		float EvaluateLength(int count)
		{
			count = Mathf.Max(samplingMinimum, count);
			var totalLength = 0f;
			var lastPoint = Evaluate(0f);
			for (var i = 0f; i < count; i++)
			{
				var currPoint = Evaluate((i + 1f) / count);
				totalLength += Vector3.Distance(lastPoint, currPoint);
				lastPoint = currPoint;
			}

			return totalLength;
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

			if (string.IsNullOrEmpty(text)) return;

			var totalWidth = 0f;
			var characterCount = text.Length;
			var labels = new List<TextMeshProUGUI>();
			var colorableLabels = new List<TextMeshProUGUI>();

			foreach (var currChar in text)
			{
				var currLabel = labelArea.InstantiateChild(labelPrefab, setActive: true);

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

			var maxWidth = EvaluateLength(characterCount);

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
				var position = Evaluate(progress, out normal);
				currLabel.transform.position = position;
				currLabel.transform.LookAt(position + Vector3.down, normal);

				currLength += labelWidths[i] * 0.5f * fontScalar;
			}
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Begin, 0.05f);
			Gizmos.DrawLine(Begin, Begin + (BoundaryUp * 0.15f));
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(End, 0.05f);

			var currSampling = Mathf.Max(1, samplingMinimum);
			var lastPoint = Evaluate(0f);

			for (var i = 0f; i < currSampling; i++)
			{
				var scalar = (i + 1f) / currSampling;
				Vector3 normal;
				var currPoint = Evaluate(scalar, out normal);
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(lastPoint, currPoint);
				Gizmos.color = Color.yellow.NewA(0.5f);
				Gizmos.DrawLine(currPoint, currPoint + (normal * 0.1f));
				lastPoint = currPoint;
			}
		}
	}
}
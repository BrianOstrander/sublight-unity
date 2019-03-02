using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class TransitHistoryLineView : UniverseScaleView, ITransitHistoryLineView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		MeshRenderer baseRenderer;
		[SerializeField]
		MeshRenderer lineRenderer;
		[SerializeField]
		Transform lineTransform;
		[SerializeField]
		Transform linePivot;

		[SerializeField]
		Gradient distanceColor;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float beginNormal;

		public void SetPoints(
			Vector3 begin,
			Vector3 end
		)
		{
			begin = begin.NewY(0f);
			end = end.NewY(0f);

			var delta = (end - begin);

			var beginOffset = lineTransform.localPosition.magnitude;
			var endOffset = beginOffset;

			var distance = Mathf.Max(delta.magnitude - (beginOffset + endOffset), 0f) * 0.5f;

			lineTransform.localScale = lineTransform.localScale.NewY(distance);
			linePivot.LookAt(linePivot.position + delta, Vector3.up);
		}

		public void SetDistance(
			float begin,
			float end
		)
		{
			beginNormal = begin;

			var beginColor = distanceColor.Evaluate(begin);
			var endColor = distanceColor.Evaluate(end);

			baseRenderer.material.SetColor(ShaderConstants.HoloTextureColorAlphaMasked.PrimaryColor, beginColor);

			lineRenderer.material.SetColor(ShaderConstants.HoloTextureColorAlphaMaskedLine.PrimaryColor, beginColor);
			lineRenderer.material.SetColor(ShaderConstants.HoloTextureColorAlphaMaskedLine.SecondaryColor, endColor);
		}

		protected override void OnOpacityStack(float opacity)
		{

		}

		public override void Reset()
		{
			base.Reset();

			SetDistance(1f, 1f);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(PositionArea.position, 0.1f);
			Gizmos.DrawLine(linePivot.position, linePivot.position + linePivot.forward);

#if UNITY_EDITOR
			Handles.color = Color.cyan;
			Handles.Label(linePivot.position + (Vector3.up * 0.2f), "Begin: " + beginNormal.ToString("N2"));
#endif
		}
	}

	public interface ITransitHistoryLineView : IUniverseScaleView
	{
		void SetPoints(
			Vector3 begin,
			Vector3 end
		);

		void SetDistance(
			float begin,
			float end
		);
	}
}
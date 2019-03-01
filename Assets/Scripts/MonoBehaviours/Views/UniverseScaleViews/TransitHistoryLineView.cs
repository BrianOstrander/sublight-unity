using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class TransitHistoryLineView : UniverseScaleView, ITransitHistoryLineView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		MeshRenderer lineRenderer;
		[SerializeField]
		Transform lineTransform;
		[SerializeField]
		Transform linePivot;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetPoints(
			Vector3 previous,
			Vector3 next
		)
		{
			previous = previous.NewY(0f);
			next = next.NewY(0f);

			var begin = Vector3.zero;
			var delta = (next - previous);

			var beginOffset = lineTransform.localPosition.magnitude;
			var endOffset = beginOffset;

			var distance = Mathf.Max(delta.magnitude - (beginOffset + endOffset), 0f) * 0.5f;

			lineTransform.localScale = lineTransform.localScale.NewY(distance);
			linePivot.LookAt(linePivot.position + delta, Vector3.up);
			//line.SetPositions(new Vector3[] { begin, begin + delta });
		}

		protected override void OnOpacityStack(float opacity)
		{

		}

		public override void Reset()
		{
			base.Reset();

		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(PositionArea.position, 0.1f);
			Gizmos.DrawLine(linePivot.position, linePivot.position + linePivot.forward);
		}
	}

	public interface ITransitHistoryLineView : IUniverseScaleView
	{
		void SetPoints(
			Vector3 previous,
			Vector3 next
		);
	}
}
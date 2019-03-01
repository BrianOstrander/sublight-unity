using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class TransitHistoryLineView : UniverseScaleView, ITransitHistoryLineView
	{

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		LineRenderer line;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetPoints(
			Vector3 previous,
			Vector3 next
		)
		{
			previous = previous.NewY(0f);
			next = next.NewY(0f);

			var begin = Vector3.zero;
			var delta = next - previous;

			line.SetPositions(new Vector3[] { begin, begin + delta });
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
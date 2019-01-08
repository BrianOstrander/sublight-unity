using UnityEngine;
using UnityEngine.UI;

namespace LunraGames.SubLight
{
	public class OpacityByDistance : MonoBehaviour
	{
		[SerializeField]
		Graphic graphic;
		[SerializeField]
		Transform origin;
		[SerializeField]
		Vector3 begin;
		[SerializeField]
		Vector3 end;
		[SerializeField]
		CurveStyleBlock opacity = CurveStyleBlock.Default;

		void Update()
		{
			var fromBegin = Vector3.Distance(transform.position, origin.position + begin);
			var fromEnd = Vector3.Distance(transform.position, origin.position + end);
			var total = Mathf.Min(Vector3.Distance(begin, end), fromBegin + fromEnd);
			graphic.color = graphic.color.NewA(opacity.Curve.Evaluate(Mathf.Min(fromBegin, total) / total));
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(origin.position + begin, 0.1f);
			Gizmos.DrawLine(origin.position + begin, origin.position + end);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(origin.position + end, 0.2f);
		}
	}
}
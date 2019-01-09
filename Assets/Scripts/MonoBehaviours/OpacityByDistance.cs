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
		float begin;
		[SerializeField]
		float end;
		[SerializeField]
		CurveStyleBlock opacity = CurveStyleBlock.Default;

		void Update()
		{
			var fromBegin = Vector3.Distance(origin.position.NewY(transform.position.y), origin.position.NewY(origin.position.y + begin));
			var fromEnd = Vector3.Distance(origin.position.NewY(transform.position.y), origin.position.NewY(origin.position.y + end));
			var total = Mathf.Min(Mathf.Abs(begin - end), fromBegin + fromEnd);
			graphic.color = graphic.color.NewA(opacity.Curve.Evaluate(Mathf.Min(fromBegin, total) / total));
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(origin.position.NewY(origin.position.y + begin), 0.1f);
			Gizmos.DrawLine(origin.position.NewY(origin.position.y + begin), origin.position.NewY(origin.position.y + end));
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(origin.position.NewY(origin.position.y + end), 0.2f);
		}
	}
}
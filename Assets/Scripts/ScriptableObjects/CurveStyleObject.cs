using UnityEngine;

namespace LunraGames.SubLight
{
	public class CurveStyleObject : ScriptableObject
	{
		[SerializeField]
		AnimationCurve curve = AnimationCurveExtensions.Constant();

		public AnimationCurve Curve { get { return curve; } }
	}
}
using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct CurveStyleBlock
	{
		public static CurveStyleBlock Default
		{
			get
			{
				return new CurveStyleBlock
				{
					globalStyle = null,
					curve = AnimationCurveExtensions.Constant()
				};
			}
		}

		[SerializeField]
		CurveStyleObject globalStyle;
		[SerializeField]
		AnimationCurve curve;

		public CurveStyleObject GlobalStyle { get { return globalStyle; } set { globalStyle = value; } }
		public AnimationCurve Curve { get { return globalStyle == null ? curve : globalStyle.Curve; } }

		public static implicit operator AnimationCurve(CurveStyleBlock b)
		{
			return b.Curve;
		}
	}
}
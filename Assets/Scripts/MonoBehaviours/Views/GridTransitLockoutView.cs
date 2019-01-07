using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridTransitLockoutView : View, IGridTransitLockoutView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float prepareDuration;
		[SerializeField]
		Vector2 transitDuration;
		[SerializeField]
		float finalizeDuration;

		[SerializeField]
		AnimationCurve transitDistanceCurve;
		[SerializeField]
		AnimationCurve transitTimeCurve;
		[SerializeField]
		AnimationCurve transitVelocityCurve;

		[Header("Animation Ranges")]
		[SerializeField]
		Vector2 groupScaleRange;
		[SerializeField]
		Vector2 groupHorizontalOffsetRange;

		[Header("Animation Curves")]
		[SerializeField]
		AnimationCurve groupOpacityCurve;
		[SerializeField]
		AnimationCurve groupScaleCurve;
		[SerializeField]
		AnimationCurve groupHorizontalOffsetCurve;
		[SerializeField]
		AnimationCurve shadowOpacityCurve;

		[Header("Animation Objects")]
		[SerializeField]
		CanvasGroup groupOpacity;
		[SerializeField]
		Transform groupScale;
		[SerializeField]
		Transform groupHorizontalOffset;
		[SerializeField]
		CanvasGroup shadowOpacity;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float PrepareDuration { get { return prepareDuration; } }
		public Vector2 TransitDuration { get { return transitDuration; } }
		public float FinalizeDuration { get { return finalizeDuration; } }

		public AnimationCurve TransitDistanceCurve { get { return transitDistanceCurve; } }
		public AnimationCurve TransitTimeCurve { get { return transitTimeCurve; } }
		public AnimationCurve TransitVelocityCurve { get { return transitVelocityCurve; } }

		public float AnimationProgress
		{
			set
			{
				groupOpacity.alpha = groupOpacityCurve.Evaluate(value);
				groupScale.localScale = Vector3.one * (groupScaleRange.x + ((groupScaleRange.y - groupScaleRange.x) * groupScaleCurve.Evaluate(value)));
				groupHorizontalOffset.localPosition = groupHorizontalOffset.localPosition.NewX(groupHorizontalOffsetRange.x + ((groupHorizontalOffsetRange.y - groupHorizontalOffsetRange.x) * groupHorizontalOffsetCurve.Evaluate(value)));
				shadowOpacity.alpha = shadowOpacityCurve.Evaluate(value);
			}
		}

		public override void Reset()
		{
			base.Reset();

			AnimationProgress = 0f;
		}

		#region Events

		#endregion
	}

	public interface IGridTransitLockoutView : IView
	{
		float PrepareDuration { get; }
		Vector2 TransitDuration { get; }
		float FinalizeDuration { get; }

		AnimationCurve TransitDistanceCurve { get; }
		AnimationCurve TransitTimeCurve { get; }
		AnimationCurve TransitVelocityCurve { get; }

		/// <summary>
		/// A value from 0 to 3.
		/// </summary>
		/// <value>The animation progress.</value>
		float AnimationProgress { set; }
	}
}
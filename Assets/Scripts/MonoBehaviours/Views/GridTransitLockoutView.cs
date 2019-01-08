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
		float transitWarmupDuration;
		[SerializeField]
		float transitCooldownDuration;
		[SerializeField]
		Vector2 transitDuration;
		[SerializeField]
		float finalizeDuration;

		[SerializeField]
		TextMeshProUGUI transitTitleLabel;
		[SerializeField]
		TextMeshProUGUI transitDescriptionLabel;
		[SerializeField]
		GridTransitLockoutTimeStampLeaf transitTimeStamp;

		[SerializeField]
		string zeroTransitTimeHexColor;

		[SerializeField]
		AnimationCurve transitDistanceCurve;
		[SerializeField]
		AnimationCurve transitTimeCurve;
		[SerializeField]
		int transitTimeWarmupDays;
		[SerializeField]
		int transitTimeCooldownDays;
		[SerializeField]
		AnimationCurve transitVelocityCurve;


		[Header("Animation Ranges")]
		[SerializeField]
		Vector2 groupScaleRange;
		[SerializeField]
		Vector2 groupHorizontalOffsetRange;
		[SerializeField]
		Vector2 detailProgressRange;
		[SerializeField]
		Vector2 detailProgressScaleRange;
		[SerializeField]
		Vector2 detailPinwheelSpeedRange;

		[Header("Animation Curves")]
		[SerializeField]
		AnimationCurve groupOpacityCurve;
		[SerializeField]
		AnimationCurve groupScaleCurve;
		[SerializeField]
		AnimationCurve groupHorizontalOffsetCurve;
		[SerializeField]
		AnimationCurve detailOpacityCurve;
		[SerializeField]
		AnimationCurve detailProgressCurve;
		[SerializeField]
		AnimationCurve detailProgressCoverOpacityCurve;
		[SerializeField]
		AnimationCurve detailProgressCoverScaleCurve;
		[SerializeField]
		AnimationCurve detailPinWheelSpeedCurve;

		[Header("Animation Objects")]
		[SerializeField]
		CanvasGroup groupOpacity;
		[SerializeField]
		Transform groupScale;
		[SerializeField]
		Transform groupHorizontalOffset;
		[SerializeField]
		CanvasGroup detailOpacity;
		[SerializeField]
		RawImage detailProgress;
		[SerializeField]
		Image detailProgressCover;
		[SerializeField]
		RawImage detailPinWheel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float pinwheelRotation;
		float pinwheelSpeed;

		public float PrepareDuration { get { return prepareDuration; } }
		public float GetTransitDuration(float scalar = 0f) { return transitCooldownDuration + transitWarmupDuration + (transitDuration.x + ((transitDuration.y - transitDuration.x) * scalar)); }
		public float FinalizeDuration { get { return finalizeDuration; } }

		public AnimationCurve TransitDistanceCurve { get { return transitDistanceCurve; } }
		public AnimationCurve TransitVelocityCurve { get { return transitVelocityCurve; } }

		public float AnimationProgress
		{
			set
			{
				groupOpacity.alpha = groupOpacityCurve.Evaluate(value);
				groupScale.localScale = Vector3.one * (groupScaleRange.x + ((groupScaleRange.y - groupScaleRange.x) * groupScaleCurve.Evaluate(value)));
				groupHorizontalOffset.localPosition = groupHorizontalOffset.localPosition.NewX(groupHorizontalOffsetRange.x + ((groupHorizontalOffsetRange.y - groupHorizontalOffsetRange.x) * groupHorizontalOffsetCurve.Evaluate(value)));
				detailOpacity.alpha = detailOpacityCurve.Evaluate(value);
				detailProgress.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstantVanish.Vanish, detailProgressRange.x + ((detailProgressRange.y - detailProgressRange.x) * detailProgressCurve.Evaluate(value)));
				pinwheelSpeed = detailPinWheelSpeedCurve.Evaluate(value);
				detailPinWheel.material.SetFloat(ShaderConstants.HoloPinWheel.Speed, pinwheelSpeed);

				detailProgressCover.transform.localScale = Vector2.one * (detailProgressScaleRange.x + ((detailProgressScaleRange.y - detailProgressScaleRange.x) * detailProgressCoverScaleCurve.Evaluate(value)));
				detailProgressCover.color = detailProgressCover.color.NewA(detailProgressCoverOpacityCurve.Evaluate(value));
				detailPinWheel.color = detailPinWheel.color = detailProgressCover.color.NewA(1f - detailProgressCoverOpacityCurve.Evaluate(value));
			}
		}

		public string TransitTitle { set { transitTitleLabel.text = value ?? string.Empty; } }
		public string TransitDescription { set { transitDescriptionLabel.text = value ?? string.Empty; } }

		public void SetTimeStamp(DayTime remaining, DayTime total)
		{
			var remainingYears = 0;
			var remainingMonths = 0;
			var remainingDays = 0;
			remaining.GetValues(out remainingYears, out remainingMonths, out remainingDays);

			var totalYears = 0;
			var totalMonths = 0;
			var totalDays = 0;
			total.GetValues(out totalYears, out totalMonths, out totalDays);

			var yearFormat = string.Empty;

			for (var i = 0; i < totalYears.ToString().Length; i++)
			{
				if (i != 0 && (i % 3) == 0) yearFormat = "0," + yearFormat;
				else yearFormat = "0" + yearFormat;
			}

			var remainingYearsText = ApplyZeroColor(remainingYears.ToString(yearFormat));
			var remainingMonthsText = ApplyZeroColor(remainingMonths.ToString("00"), remainingYears == 0);
			var remainingDaysText = ApplyZeroColor(remainingDays.ToString("00"), remainingYears == 0 && remainingMonths == 0);

			var zeroColon = "<color=" + zeroTransitTimeHexColor + ">:</color>";

			transitTimeStamp.YearColon.text = remainingYears == 0 ? zeroColon : ":";
			transitTimeStamp.MonthColon.text = (remainingYears == 0 && remainingMonths == 0) ? zeroColon : ":";

			transitTimeStamp.YearLabel.text = remainingYearsText;
			transitTimeStamp.MonthLabel.text = remainingMonthsText;
			transitTimeStamp.DayLabel.text = remainingDaysText;

			//transitTimeStamp.YearLabel.text = totalYears.ToString("N0");
			//transitTimeStamp.MonthLabel.text = totalMonths.ToString("00");
			//transitTimeStamp.DayLabel.text = totalDays.ToString("00");
		}

		public void GetTimeProgress(
			float transitProgress,
			float scalar,
			RelativeDayTime duration,
			out float timeProgress,
			out RelativeDayTime elapsed
		)
		{
			var totalDuration = GetTransitDuration(scalar);

			var warmupMaximum = transitWarmupDuration / totalDuration;
			var middleMaximum = 1f - (transitCooldownDuration / totalDuration);

			var warmup = new RelativeDayTime
			{
				ShipTime = new DayTime(Mathf.Min(transitTimeWarmupDays, duration.ShipTime.TotalTime * 0.3f)),
				GalacticTime = new DayTime(Mathf.Min(transitTimeWarmupDays, duration.GalacticTime.TotalTime * 0.3f))
			};

			var cooldown = new RelativeDayTime
			{
				ShipTime = new DayTime(Mathf.Min(transitTimeCooldownDays, duration.ShipTime.TotalTime * 0.3f)),
				GalacticTime = new DayTime(Mathf.Min(transitTimeCooldownDays, duration.GalacticTime.TotalTime * 0.3f))
			};

			var middle = duration - (warmup + cooldown);

			if (transitProgress < warmupMaximum)
			{
				elapsed = warmup * transitTimeCurve.Evaluate(transitProgress / warmupMaximum);
			}
			else if (transitProgress < middleMaximum)
			{
				var middleProgress = 1f + ((transitProgress - warmupMaximum) / (middleMaximum - warmupMaximum));
				elapsed = warmup + (middle * (transitTimeCurve.Evaluate(middleProgress) - 1f));
			}
			else
			{
				var cooldownProgress = 2f + ((transitProgress - middleMaximum) / (1f - middleMaximum));
				elapsed = warmup + middle + (cooldown * (transitTimeCurve.Evaluate(cooldownProgress) - 2f));
			}
			timeProgress = elapsed.ShipTime.TotalTime / duration.ShipTime.TotalTime;
		}

		string ApplyZeroColor(string value, bool apply = true)
		{
			if (!apply) return value;
			var result = "<color=" + zeroTransitTimeHexColor + ">";
			var wasZero = true;
			foreach (var c in value)
			{
				if (!wasZero || c == ',')
				{
					result += c;
					continue;
				}

				var lastWasZero = wasZero;
				wasZero = c == '0';
				if (lastWasZero != wasZero) result += "</color>" + c;
				else result += c;
			}
			if (wasZero) result += "</color>";

			return result;
		}

		public override void Reset()
		{
			base.Reset();

			detailProgress.material = new Material(detailProgress.material);
			detailPinWheel.material = new Material(detailPinWheel.material);

			pinwheelRotation = 0f;
			pinwheelSpeed = 0f;

			AnimationProgress = 0f;

			TransitTitle = string.Empty;
			TransitDescription = string.Empty;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			pinwheelRotation = pinwheelRotation + ((detailPinwheelSpeedRange.x + ((detailPinwheelSpeedRange.y - detailPinwheelSpeedRange.x) * pinwheelSpeed)) * delta);
			pinwheelRotation = pinwheelRotation % 360f;
			detailPinWheel.material.SetFloat(ShaderConstants.HoloPinWheel.Rotation, pinwheelRotation);
		}

		#region Events

		#endregion
	}

	public interface IGridTransitLockoutView : IView
	{
		float PrepareDuration { get; }
		float GetTransitDuration(float scalar = 0f);
		float FinalizeDuration { get; }

		AnimationCurve TransitDistanceCurve { get; }
		AnimationCurve TransitVelocityCurve { get; }

		void GetTimeProgress(
			float transitProgress,
			float scalar,
			RelativeDayTime duration,
			out float timeProgress,
			out RelativeDayTime elapsed
		);

		/// <summary>
		/// A value from 0 to 3.
		/// </summary>
		/// <value>The animation progress.</value>
		float AnimationProgress { set; }

		string TransitTitle { set; }
		string TransitDescription { set; }
		void SetTimeStamp(DayTime remaining, DayTime total);
	}
}
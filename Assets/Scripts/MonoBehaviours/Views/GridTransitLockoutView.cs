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
		TextMeshProUGUI unlockLeftLabel;
		[SerializeField]
		TextMeshProUGUI unlockRightLabel;
		[SerializeField]
		TextMeshProUGUI[] unlockLeftStatusLabels;
		[SerializeField]
		TextMeshProUGUI[] unlockRightStatusLabels;

		[SerializeField]
		string zeroTransitTimeHexColor;
		[SerializeField]
		float unlockStatusLabelListInterval;
		[SerializeField]
		float unlockStatusLabelListIntervalDuration;
		[SerializeField]
		Color unlockProgressBarEmptyColor;
		[SerializeField]
		Color unlockProgressBarFullColor;

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
		[SerializeField]
		Vector2 detailToUnlockScaleRange;
		[SerializeField]
		Vector2 unlockProgressRange;
		[SerializeField]
		Vector2 unlockProgressAnimationRange;
		[SerializeField]
		Vector2 unlockStatusLabelListRange;
		[SerializeField]
		Vector2 unlockStatusLabelListCooldownRange;

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
		AnimationCurve detailProgressBackgroundOpacityCurve;
		[SerializeField]
		AnimationCurve detailProgressCoverOpacityCurve;
		[SerializeField]
		AnimationCurve detailProgressCoverScaleCurve;
		[SerializeField]
		AnimationCurve detailPinWheelSpeedCurve;
		[SerializeField]
		AnimationCurve detailToUnlockScaleCurve;
		[SerializeField]
		AnimationCurve detailGroupOpacityByScaleCurve;
		[SerializeField]
		AnimationCurve unlockOpacityCurve;
		[SerializeField]
		AnimationCurve[] unlockProgressCurves;

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
		CanvasGroup detailGroupOpacity;
		[SerializeField]
		RawImage detailProgress;
		[SerializeField]
		RawImage detailProgressDrop;
		[SerializeField]
		Image detailProgressBackground;
		[SerializeField]
		Image detailProgressCover;
		[SerializeField]
		RawImage detailPinWheel;
		[SerializeField]
		RectTransform detailToUnlockScale;
		[SerializeField]
		CanvasGroup[] unlockOpacities;
		[SerializeField]
		RectTransform unlockProgress;
		[SerializeField]
		Graphic unlockProgressBarEmpty;
		[SerializeField]
		Graphic unlockProgressBarFull;
		[SerializeField]
		RectTransform unlockLeftStatusLabelList;
		[SerializeField]
		RectTransform unlockRightStatusLabelList;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float pinwheelRotation;
		float pinwheelSpeed;

		AnimationCurve unlockLeftProgressCurve;
		AnimationCurve unlockRightProgressCurve;

		float unlockLeftProgressLastValue;
		float unlockLeftStatusListCooldownRemaining;
		float unlockLeftStatusListTarget;

		float unlockRightProgressLastValue;
		float unlockRightStatusListCooldownRemaining;
		float unlockRightStatusListTarget;

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

				var detailProgressValue = detailProgressRange.x + ((detailProgressRange.y - detailProgressRange.x) * detailProgressCurve.Evaluate(value));
				detailProgress.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstantVanish.Vanish, detailProgressValue);
				detailProgressDrop.material.SetFloat(ShaderConstants.HoloDistanceFieldColorConstantVanish.Vanish, detailProgressValue);
				pinwheelSpeed = detailPinWheelSpeedCurve.Evaluate(value);
				detailPinWheel.material.SetFloat(ShaderConstants.HoloPinWheel.Speed, pinwheelSpeed);

				detailProgressBackground.color = detailProgressBackground.color.NewA(detailProgressBackgroundOpacityCurve.Evaluate(value));
				detailProgressCover.transform.localScale = Vector2.one * (detailProgressScaleRange.x + ((detailProgressScaleRange.y - detailProgressScaleRange.x) * detailProgressCoverScaleCurve.Evaluate(value)));
				detailProgressCover.color = detailProgressCover.color.NewA(detailProgressCoverOpacityCurve.Evaluate(value));
				detailPinWheel.color = detailPinWheel.color = detailProgressCover.color.NewA(1f - detailProgressCoverOpacityCurve.Evaluate(value));

				var detailToUnlockScaleCurrent = detailToUnlockScaleCurve.Evaluate(value);
				detailToUnlockScale.localScale = Vector3.one * (detailToUnlockScaleRange.x + ((detailToUnlockScaleRange.y - detailToUnlockScaleRange.x) * detailToUnlockScaleCurrent));

				detailGroupOpacity.alpha = detailGroupOpacityByScaleCurve.Evaluate(1f - detailToUnlockScaleCurrent);

				var unlockOpacityCurrent = unlockOpacityCurve.Evaluate(value);
				foreach (var entry in unlockOpacities) entry.alpha = unlockOpacityCurrent;

				if (unlockProgressAnimationRange.x <= value)
				{
					// Animate unlock progress bar here
					var unlockProgressBarRange = unlockProgressAnimationRange.y - unlockProgressAnimationRange.x;
					var barValue = Mathf.Min(value - unlockProgressAnimationRange.x, unlockProgressBarRange) / unlockProgressBarRange;
					var barRange = unlockProgressRange.y - unlockProgressRange.x;
					var leftValue = unlockProgressRange.x + (unlockLeftProgressCurve.Evaluate(barValue) * barRange);
					var rightValue = unlockProgressRange.x + (unlockRightProgressCurve.Evaluate(barValue) * barRange);

					unlockProgress.offsetMin = new Vector2(leftValue, 0f);
					unlockProgress.offsetMax = new Vector2(-rightValue, 0f);

					if (!Mathf.Approximately(unlockLeftProgressLastValue, leftValue) && Mathf.Approximately(0f, unlockLeftStatusListCooldownRemaining))
					{
						unlockLeftProgressLastValue = leftValue;
						unlockLeftStatusListTarget = Mathf.Max(0f, unlockLeftStatusListTarget - unlockStatusLabelListInterval);
						unlockLeftStatusListCooldownRemaining = unlockStatusLabelListCooldownRange.x + ((unlockStatusLabelListCooldownRange.y - unlockStatusLabelListCooldownRange.x) * NumberDemon.DemonUtility.NextFloat);
					}

					if (!Mathf.Approximately(unlockRightProgressLastValue, rightValue) && Mathf.Approximately(0f, unlockRightStatusListCooldownRemaining))
					{
						unlockRightProgressLastValue = rightValue;
						unlockRightStatusListTarget = Mathf.Max(0f, unlockRightStatusListTarget - unlockStatusLabelListInterval);
						unlockRightStatusListCooldownRemaining = unlockStatusLabelListCooldownRange.x + ((unlockStatusLabelListCooldownRange.y - unlockStatusLabelListCooldownRange.x) * NumberDemon.DemonUtility.NextFloat);
					}

					if (Mathf.Approximately(leftValue, 0f) && Mathf.Approximately(rightValue, 0f))
					{
						unlockProgressBarEmpty.color = unlockProgressBarFullColor;
						unlockProgressBarFull.color = unlockProgressBarFull.color.NewA(0f);
					}
				}
			}
		}

		public string TransitTitle { set { transitTitleLabel.text = value ?? string.Empty; } }
		public string TransitDescription { set { transitDescriptionLabel.text = value ?? string.Empty; } }
		public string UnlockLeftTitle { set { unlockLeftLabel.text = value ?? string.Empty; } }
		public string UnlockRightTitle { set { unlockRightLabel.text = value ?? string.Empty; } }

		public string[] UnlockLeftStatuses { set { SetStatuses(unlockLeftStatusLabels, value ?? new string[0]); } }
		public string[] UnlockRightStatuses { set { SetStatuses(unlockRightStatusLabels, value ?? new string[0]); } }

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

		void SetStatuses(TextMeshProUGUI[] labels, params string[] statuses)
		{
			for (var i = 0; i < labels.Length; i++)
			{
				var currLabel = labels[(labels.Length - 1) - i];
				currLabel.text = i < statuses.Length ? statuses[i] : string.Empty;
			}
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
			detailProgressDrop.material = new Material(detailProgressDrop.material);
			detailPinWheel.material = new Material(detailPinWheel.material);

			pinwheelRotation = 0f;
			pinwheelSpeed = 0f;

			AnimationProgress = 0f;

			TransitTitle = string.Empty;
			TransitDescription = string.Empty;
			UnlockLeftTitle = string.Empty;
			UnlockRightTitle = string.Empty;

			UnlockLeftStatuses = null;
			UnlockRightStatuses = null;

			var progressCurveOptions = new List<int>();
			for (var i = 0; i < unlockProgressCurves.Length; i++) progressCurveOptions.Add(i);
			var leftIndex = progressCurveOptions.Random();
			progressCurveOptions.RemoveAt(leftIndex);
			var rightIndex = progressCurveOptions.Random();

			unlockLeftProgressCurve = unlockProgressCurves[leftIndex];
			unlockRightProgressCurve = unlockProgressCurves[rightIndex];

			unlockLeftStatusListTarget = unlockStatusLabelListRange.x;
			unlockRightStatusListTarget = unlockStatusLabelListRange.x;

			unlockProgressBarEmpty.color = unlockProgressBarEmptyColor;
			unlockProgressBarFull.color = unlockProgressBarFullColor;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			pinwheelRotation = pinwheelRotation + ((detailPinwheelSpeedRange.x + ((detailPinwheelSpeedRange.y - detailPinwheelSpeedRange.x) * pinwheelSpeed)) * delta);
			pinwheelRotation = pinwheelRotation % 360f;
			detailPinWheel.material.SetFloat(ShaderConstants.HoloPinWheel.Rotation, pinwheelRotation);

			unlockLeftStatusListCooldownRemaining = Mathf.Max(0f, unlockLeftStatusListCooldownRemaining - delta);
			unlockRightStatusListCooldownRemaining = Mathf.Max(0f, unlockRightStatusListCooldownRemaining - delta);

			if (unlockLeftStatusListTarget - unlockLeftStatusLabelList.offsetMin.y < 0f)
			{
				unlockLeftStatusLabelList.offsetMin = new Vector2(0f, Mathf.Max(unlockLeftStatusListTarget, unlockLeftStatusLabelList.offsetMin.y - (delta * unlockStatusLabelListInterval * (1f / unlockStatusLabelListIntervalDuration))));
			}

			if (unlockRightStatusListTarget - unlockRightStatusLabelList.offsetMin.y < 0f)
			{
				unlockRightStatusLabelList.offsetMin = new Vector2(0f, Mathf.Max(unlockRightStatusListTarget, unlockRightStatusLabelList.offsetMin.y - (delta * unlockStatusLabelListInterval * (1f / unlockStatusLabelListIntervalDuration))));
			}
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
		string UnlockLeftTitle { set; }
		string UnlockRightTitle { set; }

		string[] UnlockLeftStatuses { set; }
		string[] UnlockRightStatuses { set; }
	}
}
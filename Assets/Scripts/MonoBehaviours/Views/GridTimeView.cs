using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public struct GridTimeBlock
	{
		public static GridTimeBlock Default
		{
			get
			{
				var referenceFrameNames = new Dictionary<ReferenceFrames, string>();
				foreach (var referenceFrame in EnumExtensions.GetValues<ReferenceFrames>()) referenceFrameNames[referenceFrame] = null;
				return new GridTimeBlock
				{
					ReferenceFrameNames = referenceFrameNames
				};
			}
		}

		public string Title;
		public string SubTitle;
		public string Tooltip;
		public Dictionary<ReferenceFrames, string> ReferenceFrameNames;
		public Action<ReferenceFrames> ReferenceFrameSelection;
		public Action TitleClick;
		public bool IsTransit;
	}

	public struct GridTimeStampBlock
	{
		public static GridTimeStampBlock Default
		{
			get
			{
				var absoluteTimes = new Dictionary<ReferenceFrames, DayTime>();
				var deltaTimes = new Dictionary<ReferenceFrames, DayTime>();
				foreach (var referenceFrame in EnumExtensions.GetValues<ReferenceFrames>())
				{
					absoluteTimes[referenceFrame] = DayTime.Zero;
					deltaTimes[referenceFrame] = DayTime.Zero;
				}

				return new GridTimeStampBlock
				{
					AbsoluteTimes = absoluteTimes,
					DeltaTimes = deltaTimes
				};
			}
		}

		public Dictionary<ReferenceFrames, DayTime> AbsoluteTimes;
		public Dictionary<ReferenceFrames, DayTime> DeltaTimes;
	}

	public class GridTimeView : View, IGridTimeView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI subTitleLabel;
		[SerializeField]
		TextMeshProUGUI tooltipLabel;
		[SerializeField]
		GridTimeReferenceButtonLeaf shipButton;
		[SerializeField]
		GridTimeReferenceButtonLeaf galacticButton;
		[SerializeField]
		XButtonStyleObject referenceButtonLabelStyle;
		[SerializeField]
		XButtonStyleObject referenceButtonBackgroundStyle;
		[SerializeField]
		XButtonStyleObject referenceButtonActiveLabelStyle;
		[SerializeField]
		XButtonStyleObject referenceButtonActiveBackgroundStyle;
		[SerializeField]
		XButton timeAreaButton;
		[SerializeField]
		GridTimeStampLeaf absoluteArea;
		[SerializeField]
		GridTimeStampLeaf deltaArea;
		[SerializeField]
		ParticleSystem timeStampTransitionParticles;
		[SerializeField]
		ParticleSystem timeStampReferenceFrameParticles;

		[Header("Chronometer Time Stamp Style")]
		[SerializeField]
		XButtonStyleBlock chronometerAbsoluteTimeStampStyle = XButtonStyleBlock.Default;
		[Header("Transit Time Stamp Styles")]
		[Header("--Absolute")]
		[SerializeField]
		XButtonStyleBlock transitAbsoluteTimeStampStyle = XButtonStyleBlock.Default;
		[Header("--Delta")]
		[SerializeField]
		XButtonStyleBlock transitDeltaTimeStampStyle = XButtonStyleBlock.Default;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		bool isReferenceFrameStale;
		bool isConfigurationStale;
		bool isTimeStampStale;

		ReferenceFrames referenceFrame;
		public ReferenceFrames ReferenceFrame
		{
			get { return referenceFrame; }
			set
			{
				referenceFrame = value;
				isReferenceFrameStale = true;
			}
		}

		GridTimeBlock configuration = GridTimeBlock.Default;
		public GridTimeBlock Configuration
		{
			private get { return configuration; }
			set
			{
				configuration = value;
				isConfigurationStale = true;
			}
		}

		GridTimeStampBlock timeStamp = GridTimeStampBlock.Default;
		public GridTimeStampBlock TimeStamp
		{
			private get { return timeStamp; }
			set
			{
				timeStamp = value;
				isTimeStampStale = true;
			}
		}

		ParticleSystem.EmitParams ParticleWithOpacity(ParticleSystem particleSystem)
		{
			var emit = new ParticleSystem.EmitParams();
			emit.startColor = particleSystem.main.startColor.color.NewA(particleSystem.main.startColor.color.a * OpacityStack);
			return emit;
		}

		public void TimeStampTransition()
		{
			timeStampTransitionParticles.Emit(ParticleWithOpacity(timeStampTransitionParticles), 1);
		}

		public void ReferenceFrameTransition()
		{
			timeStampReferenceFrameParticles.Emit(ParticleWithOpacity(timeStampReferenceFrameParticles), 1);
		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);


			if (isConfigurationStale)
			{
				titleLabel.text = Configuration.Title ?? string.Empty;
				subTitleLabel.text = Configuration.SubTitle ?? string.Empty;
				tooltipLabel.text = Configuration.Tooltip ?? string.Empty;

				if (Configuration.IsTransit)
				{
					absoluteArea.ButtonLeaf.LocalStyle = transitAbsoluteTimeStampStyle;
					deltaArea.ButtonLeaf.LocalStyle = transitDeltaTimeStampStyle;
					deltaArea.gameObject.SetActive(true);
				}
				else
				{
					absoluteArea.ButtonLeaf.LocalStyle = chronometerAbsoluteTimeStampStyle;
					deltaArea.gameObject.SetActive(false);
				}
				timeAreaButton.ForceApplyState();
			}

			if (isConfigurationStale || isReferenceFrameStale)
			{
				ApplyStyles(shipButton, Configuration.ReferenceFrameNames[ReferenceFrames.Ship], ReferenceFrame == ReferenceFrames.Ship);
				ApplyStyles(galacticButton, Configuration.ReferenceFrameNames[ReferenceFrames.Galactic], ReferenceFrame == ReferenceFrames.Galactic);
			}

			if (isReferenceFrameStale || isTimeStampStale || isConfigurationStale)
			{
				ApplyTimeStamp(TimeStamp.AbsoluteTimes[ReferenceFrame], absoluteArea);
				ApplyTimeStamp(TimeStamp.DeltaTimes[ReferenceFrame], deltaArea);
			}

			isConfigurationStale = false;
			isReferenceFrameStale = false;
			isTimeStampStale = false;
		}

		public override void Reset()
		{
			base.Reset();

			Configuration = GridTimeBlock.Default;
			ReferenceFrame = ReferenceFrames.Ship;
			TimeStamp = GridTimeStampBlock.Default;
		}

		void ApplyTimeStamp(DayTime time, GridTimeStampLeaf leaf)
		{
			var years = 0;
			var months = 0;
			var days = 0;
			time.GetValues(out years, out months, out days);

			leaf.YearLabel.text = years.ToString("N0");
			leaf.MonthLabel.text = months.ToString("00");
			leaf.DayLabel.text = days.ToString("00");
		}
		
		void ApplyStyles(GridTimeReferenceButtonLeaf leaf, string text, bool isActive)
		{
			leaf.Label.text = text;
			leaf.LabelLeaf.GlobalStyle = isActive ? referenceButtonActiveLabelStyle : referenceButtonLabelStyle;
			leaf.BackgroundLeaf.GlobalStyle = isActive ? referenceButtonActiveBackgroundStyle : referenceButtonBackgroundStyle;

			leaf.Button.ForceApplyState();
		}

		#region Events
		public void OnTitleClick()
		{
			if (Configuration.TitleClick != null) Configuration.TitleClick();
		}

		public void OnReferenceFrameEnter()
		{
			if (Configuration.IsTransit) ReferenceFrameTransition();
		}

		public void OnReferenceFrameExit()
		{
			//if (Configuration.IsTransit) ReferenceFrameTransition();
		}

		public void OnShipClick()
		{
			if (ReferenceFrame == ReferenceFrames.Ship) return;
			if (Configuration.ReferenceFrameSelection != null) Configuration.ReferenceFrameSelection(ReferenceFrames.Ship);
			ReferenceFrame = ReferenceFrames.Ship;
			ReferenceFrameTransition();
		}

		public void OnGalacticClick()
		{
			if (ReferenceFrame == ReferenceFrames.Galactic) return;
			if (Configuration.ReferenceFrameSelection != null) Configuration.ReferenceFrameSelection(ReferenceFrames.Galactic);
			ReferenceFrame = ReferenceFrames.Galactic;
			ReferenceFrameTransition();
		}
		#endregion
	}

	public interface IGridTimeView : IView
	{
		ReferenceFrames ReferenceFrame { set; }
		GridTimeBlock Configuration { set; }
		GridTimeStampBlock TimeStamp { set; }
		void TimeStampTransition();
		void ReferenceFrameTransition();
	}
}
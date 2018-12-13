using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public struct GridTimeBlock
	{
		public string Title;
		public string SubTitle;
		public string Tooltip;
		public Dictionary<ReferenceFrames, string> ReferenceFrames;
		public Action<ReferenceFrames> ReferenceFrameSelection;
		public Action TitleClick;
		public bool IsDelta;
	}

	public struct GridTimeStampBlock
	{
		public Dictionary<ReferenceFrames, DayTime> AbsoluteTimes;
		public Dictionary<ReferenceFrames, DayTime> DeltaTimes;
	}

	public class GridTimeView : View, IGridTimeView
	{
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
		GridTimeStampLeaf absoluteArea;
		[SerializeField]
		GridTimeStampLeaf deltaArea;
		[Header("Absolute Style")]
		[SerializeField]
		XButtonStyleBlock absoluteStyle = XButtonStyleBlock.Default;
		[Header("Delta Styles")]
		[Header("--Absolute")]
		[SerializeField]
		XButtonStyleBlock deltaAbsoluteStyle = XButtonStyleBlock.Default;
		[Header("--Delta")]
		[SerializeField]
		XButtonStyleBlock deltaDeltaStyle = XButtonStyleBlock.Default;

		ReferenceFrames referenceFrame;
		public ReferenceFrames ReferenceFrame
		{
			get { return referenceFrame; }
			set
			{
				referenceFrame = value;
				if (Configuration.ReferenceFrames != null)
				{
					ApplyStyles(shipButton, Configuration.ReferenceFrames[ReferenceFrames.Ship], ReferenceFrame == ReferenceFrames.Ship);
					ApplyStyles(galacticButton, Configuration.ReferenceFrames[ReferenceFrames.Galactic], ReferenceFrame == ReferenceFrames.Galactic);
				}
			}
		}

		GridTimeBlock configuration;
		public GridTimeBlock Configuration
		{
			get { return configuration; }
			set
			{
				configuration = value;
				titleLabel.text = value.Title ?? string.Empty;
				subTitleLabel.text = value.SubTitle ?? string.Empty;
				tooltipLabel.text = value.Tooltip ?? string.Empty;

				ReferenceFrame = ReferenceFrame; // Uh weird... but just go with it...

				if (value.IsDelta)
				{
					absoluteArea.ButtonLeaf.LocalStyle = deltaAbsoluteStyle;
					deltaArea.ButtonLeaf.LocalStyle = deltaDeltaStyle;
					deltaArea.gameObject.SetActive(true);
				}
				else
				{
					absoluteArea.ButtonLeaf.LocalStyle = absoluteStyle;
					deltaArea.gameObject.SetActive(false);
				}
			}
		}

		public GridTimeStampBlock TimeStamp
		{
			set
			{

			}
		}

		protected override void OnOpacityStack(float opacity)
		{
		
		}

		public override void Reset()
		{
			base.Reset();

			Configuration = new GridTimeBlock();
			TimeStamp = new GridTimeStampBlock();
		}

		void ApplyStyles(GridTimeReferenceButtonLeaf leaf, string text, bool isActive)
		{
			leaf.Label.text = text;
			leaf.LabelLeaf.GlobalStyle = isActive ? referenceButtonActiveLabelStyle : referenceButtonLabelStyle;
			leaf.BackgroundLeaf.GlobalStyle = isActive ? referenceButtonActiveBackgroundStyle : referenceButtonBackgroundStyle;
		}

		#region Events
		public void OnTitleClick()
		{
			if (Configuration.TitleClick != null) Configuration.TitleClick();
		}

		public void OnShipClick()
		{
			if (Configuration.ReferenceFrameSelection == null || ReferenceFrame == ReferenceFrames.Ship) return;
			Configuration.ReferenceFrameSelection(ReferenceFrames.Ship);
			ReferenceFrame = ReferenceFrames.Ship;
		}

		public void OnGalacticClick()
		{
			if (Configuration.ReferenceFrameSelection == null || ReferenceFrame == ReferenceFrames.Galactic) return;
			Configuration.ReferenceFrameSelection(ReferenceFrames.Galactic);
			ReferenceFrame = ReferenceFrames.Galactic;
		}
		#endregion
	}

	public interface IGridTimeView : IView
	{
		ReferenceFrames ReferenceFrame { set; }
		GridTimeBlock Configuration { set; }
		GridTimeStampBlock TimeStamp { set; }
	}
}
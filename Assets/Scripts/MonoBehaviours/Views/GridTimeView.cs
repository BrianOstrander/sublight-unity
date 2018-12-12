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
		public ReferenceFrames ReferenceFrameCurrent;
	}

	public struct GridTimeStampBlock
	{
		public bool IsDelta;
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

		public ReferenceFrames ReferenceFrame { set; private get; }
		public GridTimeBlock Configuration
		{
			set
			{

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

		
		}

		#region Events
		public void OnTitleClick()
		{

		}

		public void OnTimeClick()
		{

		}

		public void OnShipClick()
		{

		}

		public void OnGalacticClick()
		{

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
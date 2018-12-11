using System.Collections.Generic;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridScaleView : View, IGridScaleView
	{
		[SerializeField]
		TextMeshProUGUI scaleNameLabel;
		[SerializeField]
		TextMeshProUGUI scaleLabel;

		[SerializeField]
		TextMeshProUGUI unitCountLabel;
		[SerializeField]
		TextMeshProUGUI unitTypeLabel;

		[SerializeField]
		int renderQueue;
		[SerializeField]
		MeshRenderer gridScaleMesh;
		[SerializeField]
		MeshRenderer gridUnitScaleMesh;
		[SerializeField]
		Transform rotateTargetPrimary;
		[SerializeField]
		Transform rotateTargetSecondary;
		[SerializeField]
		Vector3 rotation;

		[SerializeField]
		AnimationCurve toRightProgressCurve;
		[SerializeField]
		AnimationCurve toRightAlphaCurve;
		[SerializeField]
		AnimationCurve toLeftProgressCurve;
		[SerializeField]
		AnimationCurve toLeftAlphaCurve;

		[SerializeField]
		float scaleNudgeIntensity;
		[SerializeField]
		float scaleLabelNudgeIntensity;
		[SerializeField]
		AnimationCurve scaleNudgeCurve;

		[SerializeField]
		Transform scaleLabelArea;

		[SerializeField]
		CanvasGroup[] opacityAreas;

		public string ScaleNameText { set { scaleNameLabel.text = value ?? string.Empty; } }
		public string ScaleText { set { scaleLabel.text = value ?? string.Empty; } }
		public string UnitCountText { set { unitCountLabel.text = value ?? string.Empty; } }
		public string UnitTypeText { set { unitTypeLabel.text = value ?? string.Empty; } }

		public void Zoom(float zoom, float unitProgress, bool unitToRight)
		{
			gridScaleMesh.material.renderQueue = renderQueue;
			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, zoom + 1f);

			var progressCurve = unitToRight ? toRightProgressCurve : toLeftProgressCurve;
			var alphaCurve = unitToRight ? toRightAlphaCurve : toLeftAlphaCurve;

			gridUnitScaleMesh.material.renderQueue = renderQueue;
			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.Progress, progressCurve.Evaluate(unitProgress));
			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.FullProgress, alphaCurve.Evaluate(unitProgress));
			//gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.ProgressToRight, unitToRight ? 1f : 0f);
		}

		public void Nudge(float zoom, float progress, bool isUp)
		{
			var nudge = scaleNudgeCurve.Evaluate(progress);
			var scaleOffset = nudge * (isUp ? scaleNudgeIntensity : -scaleNudgeIntensity);
			var scaleLabelOffset = nudge * (isUp ? scaleLabelNudgeIntensity : -scaleLabelNudgeIntensity);

			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, zoom + scaleOffset + 1f);
			scaleLabelArea.localScale = Vector3.one * (1f + scaleLabelOffset);
		}

		public Color HoloColor
		{
			set
			{
				gridScaleMesh.material.SetColor(ShaderConstants.HoloGridScale.ColorTint, value);
				gridUnitScaleMesh.material.SetColor(ShaderConstants.HoloGridUnitScale.ColorTint, value);
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Alpha, opacity);
			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.Alpha, opacity);
			foreach (var area in opacityAreas) area.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			ScaleNameText = null;
			ScaleText = null;
			UnitCountText = null;
			UnitTypeText = null;

			HoloColor = Color.white;

			rotateTargetPrimary.localRotation = Quaternion.Euler(rotation);
			rotateTargetSecondary.localRotation = Quaternion.Euler(-rotation);
		}
	}

	public interface IGridScaleView : IView, IHoloColorView
	{
		string ScaleNameText { set; }
		string ScaleText { set; }
		string UnitCountText { set; }
		string UnitTypeText { set; }
		void Zoom(float zoom, float unitProgress, bool unitToRight);
		void Nudge(float zoom, float progress, bool isUp);
	}
}
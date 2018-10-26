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
		Material gridScaleMaterial;
		[SerializeField]
		MeshRenderer gridScaleMesh;
		[SerializeField]
		Material gridUnitScaleMaterial;
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

		public string ScaleNameText { set { scaleNameLabel.text = value ?? string.Empty; } }
		public string ScaleText { set { scaleLabel.text = value ?? string.Empty; } }
		public string UnitCountText { set { unitCountLabel.text = value ?? string.Empty; } }
		public string UnitTypeText { set { unitTypeLabel.text = value ?? string.Empty; } }

		public void Zoom(float zoom, float unitProgress, bool unitToRight)
		{
			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, zoom + 1f);

			var progressCurve = unitToRight ? toRightProgressCurve : toLeftProgressCurve;
			var alphaCurve = unitToRight ? toRightAlphaCurve : toLeftAlphaCurve;

			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.Progress, progressCurve.Evaluate(unitProgress));
			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.FullProgress, alphaCurve.Evaluate(unitProgress));
			//gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.ProgressToRight, unitToRight ? 1f : 0f);
		}

		public Color HoloColor
		{
			set
			{
				gridScaleMesh.material.SetColor(ShaderConstants.HoloGridScale.ColorTint, value);
				gridUnitScaleMesh.material.SetColor(ShaderConstants.HoloGridUnitScale.ColorTint, value);
			}
		}

		public override void Reset()
		{
			base.Reset();

			gridScaleMesh.material = new Material(gridScaleMaterial);
			gridUnitScaleMesh.material = new Material(gridUnitScaleMaterial);

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
	}
}
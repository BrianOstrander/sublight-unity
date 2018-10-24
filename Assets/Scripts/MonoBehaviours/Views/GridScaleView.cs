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
		AnimationCurve gridUnitScaleFullCurve;

		public string ScaleText { set { scaleLabel.text = value ?? string.Empty; } }

		public ZoomInfoBlock ZoomInfo
		{
			set
			{
				gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, value.Zoom);
				
				var unitZoom = Mathf.Approximately(value.Zoom, 5f) ? 1f : value.UnitProgress;
				
				gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.Progress, unitZoom);
				
				gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.FullProgress, gridUnitScaleFullCurve.Evaluate(unitZoom));

				if (value.ScaleName == null) scaleNameLabel.text = string.Empty;
				else scaleNameLabel.text = value.ScaleName.Value.Value ?? string.Empty;

				if (value.UnitAmountFormatted == null) unitCountLabel.text = string.Empty;
				else unitCountLabel.text = value.UnitAmountFormatted() ?? string.Empty;
				
				if (value.UnitName == null) unitTypeLabel.text = string.Empty;
				else unitTypeLabel.text = value.UnitName.Value.Value ?? string.Empty;
			}
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

			ScaleText = null;

			HoloColor = Color.white;

			rotateTargetPrimary.localRotation = Quaternion.Euler(rotation);
			rotateTargetSecondary.localRotation = Quaternion.Euler(-rotation);
		}
	}

	public interface IGridScaleView : IView, IHoloColorView
	{
		string ScaleText { set; }
		ZoomInfoBlock ZoomInfo { set; }
	}
}
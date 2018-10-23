using System.Collections.Generic;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridScaleView : View, IGridScaleView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI unitLabel;

		[SerializeField]
		Material gridScaleMaterial;
		[SerializeField]
		MeshRenderer gridScaleMesh;
		[SerializeField]
		Material gridUnitScaleMaterial;
		[SerializeField]
		MeshRenderer gridUnitScaleMesh;
		[SerializeField]
		Transform rotateTarget;
		[SerializeField]
		Vector3 rotation;

		[SerializeField]
		AnimationCurve gridUnitScaleFullCurve;

		public string UnitText { set { unitLabel.text = value ?? string.Empty; } }
		public void SetZoom(float zoom, string scaleName = null) //, string unitName = null)
		{
			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, zoom);

			var unitZoom = Mathf.Approximately(zoom, 5f) ? 1f : zoom - Mathf.Floor(zoom);

			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.Progress, unitZoom);

			gridUnitScaleMesh.material.SetFloat(ShaderConstants.HoloGridUnitScale.FullProgress, gridUnitScaleFullCurve.Evaluate(unitZoom));

			titleLabel.text = scaleName ?? string.Empty;
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

			UnitText = null;
			SetZoom(1f);
			HoloColor = Color.white;

			rotateTarget.localRotation = Quaternion.Euler(rotation);
		}
	}

	public interface IGridScaleView : IView, IHoloColorView
	{
		string UnitText { set; }
		void SetZoom(float zoom, string scaleName = null);
	}
}
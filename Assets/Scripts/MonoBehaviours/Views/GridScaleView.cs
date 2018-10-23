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
		Transform rotateTarget;
		[SerializeField]
		Vector3 rotation;

		public string UnitText { set { unitLabel.text = value ?? string.Empty; } }
		public void SetZoom(float zoom, string name = null)
		{
			gridScaleMesh.material.SetFloat(ShaderConstants.HoloGridScale.Zoom, zoom);
			titleLabel.text = name ?? string.Empty;
		}

		public Color HoloColor { set { gridScaleMesh.material.SetColor(ShaderConstants.HoloGridScale.ColorTint, value); } }

		public override void Reset()
		{
			base.Reset();

			gridScaleMesh.material = new Material(gridScaleMaterial);

			UnitText = null;
			SetZoom(1f);
			HoloColor = Color.white;

			rotateTarget.localRotation = Quaternion.Euler(rotation);
		}
	}

	public interface IGridScaleView : IView, IHoloColorView
	{
		string UnitText { set; }
		void SetZoom(float zoom, string name = null);
	}
}
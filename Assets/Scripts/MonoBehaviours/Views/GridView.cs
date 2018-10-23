using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		[SerializeField]
		float zoomMaximum;
		[SerializeField]
		float zoomSensitivity;
		[SerializeField]
		AnimationCurve revealCurve;
		[SerializeField]
		AnimationCurve zoomCurve;
		[SerializeField]
		AnimationCurve zoomRadiusProgress;
		[SerializeField]
		AnimationCurve zoomAlpha;
		[SerializeField]
		AnimationCurve zoomSensitivityCurve;

		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		MeshRenderer gridMesh;

		public bool Highlighted { get; private set; }

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, revealCurve.Evaluate(scalar));
		}

		public float UpdateZoom(float current, float delta = 0f)
		{
			var sensitivity = zoomSensitivity * (zoomSensitivityCurve.Evaluate(current));
			var zoom = Mathf.Clamp(current + (delta * sensitivity), 0f, zoomMaximum);

			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.Zoom, zoomCurve.Evaluate(zoom) % 1f);
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.Alpha, zoomAlpha.Evaluate(zoom));
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, zoomRadiusProgress.Evaluate(zoom));

			return zoom;
		}

		public Color HoloColor { set { gridMesh.material.SetColor(ShaderConstants.HoloGrid.GridColor, value); } }

		public override void Reset()
		{
			base.Reset();

			gridMesh.material = new Material(gridMaterial);

			Highlighted = false;
			SetRadius(0f, true);
			UpdateZoom(1f);
			HoloColor = Color.white;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			SetRadius(scalar, true);
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			SetRadius(scalar, false);
		}

		#region Events
		public void OnEnter()
		{
			Highlighted = true;
		}

		public void OnExit()
		{
			Highlighted = false;
		}
		#endregion
	}

	public interface IGridView : IView, IHoloColorView
	{
		void SetRadius(float scalar, bool showing);
		float UpdateZoom(float current, float delta = 0f);
		bool Highlighted { get; }
	}
}
using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		[SerializeField]
		AnimationCurve revealCurve;

		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		MeshRenderer gridMesh;

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, revealCurve.Evaluate(scalar));
		}

		public Color HoloColor { set { gridMesh.material.SetColor(ShaderConstants.HoloGrid.GridColor, value); } }

		public override void Reset()
		{
			base.Reset();

			gridMesh.material = new Material(gridMaterial);

			SetRadius(0f, true);
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
	}

	public interface IGridView : IView, IHoloColorView
	{
		void SetRadius(float scalar, bool showing);
	}
}
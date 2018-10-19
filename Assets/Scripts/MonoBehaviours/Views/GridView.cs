using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		[SerializeField]
		AnimationCurve RevealRadius;

		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		MeshRenderer gridMesh;
		[SerializeField]
		Color baseGridColor;

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;

			//gridMesh.material.SetFloat(ShaderConstants.HoloLip.LipMin, MinGridAnimation.Evaluate(scalar));
		}

		public Color HoloColor { set { gridMesh.material.SetColor(ShaderConstants.HoloIrisGrid.GridColor, baseGridColor.NewHsva(value.GetH(), value.GetS())); } }

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
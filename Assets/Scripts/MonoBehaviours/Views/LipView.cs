using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class LipView : View, ILipView
	{
		[SerializeField]
		AnimationCurve MinLipAnimation;
		[SerializeField]
		AnimationCurve MaxLipAnimation;

		[SerializeField]
		Material lipMaterial;
		[SerializeField]
		MeshRenderer lipMesh;
		[SerializeField]
		Color baseLipColor;

		public void SetLips(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;

			lipMesh.material.SetFloat(ShaderConstants.HoloLip.LipMin, MinLipAnimation.Evaluate(scalar));
			lipMesh.material.SetFloat(ShaderConstants.HoloLip.LipMax, MaxLipAnimation.Evaluate(scalar));
		}

		public Color HoloColor { set { lipMesh.material.SetColor(ShaderConstants.HoloLip.LipColor, baseLipColor.NewHsva(value.GetH(), value.GetS())); } }

		public override void Reset()
		{
			base.Reset();

			lipMesh.material = new Material(lipMaterial);

			SetLips(0f, true);
			HoloColor = Color.white;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			SetLips(scalar, true);
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			SetLips(scalar, false);
		}
	}

	public interface ILipView : IView, IHoloColorView
	{
		void SetLips(float scalar, bool showing);
	}
}
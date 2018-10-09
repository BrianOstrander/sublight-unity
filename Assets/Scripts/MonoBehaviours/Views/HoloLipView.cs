using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class HoloLipView : View, IHoloLipView
	{
		[SerializeField]
		AnimationCurve MinLipAnimation;
		[SerializeField]
		AnimationCurve MaxLipAnimation;

		[SerializeField]
		Material lipMaterial;
		[SerializeField]
		MeshRenderer lipMesh;

		Material currentMaterial;

		void SetLips(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;

			lipMesh.material.SetFloat(ShaderConstants.HoloLip.LipMin, MinLipAnimation.Evaluate(scalar));
			lipMesh.material.SetFloat(ShaderConstants.HoloLip.LipMax, MaxLipAnimation.Evaluate(scalar));
		}

		public override void Reset()
		{
			base.Reset();

			currentMaterial = new Material(lipMaterial);
			lipMesh.material = currentMaterial;

			SetLips(0f, true);
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

	public interface IHoloLipView : IView
	{

	}
}
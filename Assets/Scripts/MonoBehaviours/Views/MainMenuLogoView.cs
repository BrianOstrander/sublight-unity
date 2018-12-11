using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class MainMenuLogoView : View, IMainMenuLogoView
	{
		[SerializeField]
		MeshRenderer MaskMesh;

		protected override void OnOpacityStack(float opacity)
		{
			MaskMesh.material.SetFloat(ShaderConstants.HoloMask.Opacity, opacity);
		}
	}

	public interface IMainMenuLogoView : IView
	{

	}
}
using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class MainMenuLogoView : View, IMainMenuLogoView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		MeshRenderer MaskMesh;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		protected override void OnOpacityStack(float opacity)
		{
			MaskMesh.material.SetFloat(ShaderConstants.HoloMask.Opacity, opacity);
		}
	}

	public interface IMainMenuLogoView : IView
	{

	}
}
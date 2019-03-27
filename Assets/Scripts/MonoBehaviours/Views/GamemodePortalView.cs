using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GamemodePortalView : View, IGamemodePortalView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public Color HoloColor { set { } } // lipMesh.material.SetColor(ShaderConstants.HoloLipAdditive.LipColor, value); } }

		public override void Reset()
		{
			base.Reset();

			HoloColor = Color.white;
		}
	}

	public interface IGamemodePortalView : IView, IHoloColorView
	{

	}
}
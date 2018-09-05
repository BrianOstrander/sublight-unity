using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class FocusCameraView : View, IFocusCameraView
	{
		[SerializeField]
		Camera camera;

		RenderTexture texture;

		public RenderTexture Texture
		{
			get
			{
				if (texture == null) camera.targetTexture = texture = new RenderTexture(Screen.width, Screen.height, 16);
				return texture;
			}
		}

		public void SetCameraEnabled(bool isEnabled)
		{
			camera.enabled = isEnabled;
		}

		public override void Reset()
		{
			base.Reset();

			camera.targetTexture = null;
			texture = null;

			SetCameraEnabled(true);
		}
	}

	public interface IFocusCameraView : IView
	{
		RenderTexture Texture { get; }
		void SetCameraEnabled(bool isEnabled);
	}
}
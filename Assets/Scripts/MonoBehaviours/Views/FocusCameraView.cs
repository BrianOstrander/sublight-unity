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

		public bool CameraEnabled
		{
			get { return camera.enabled; }
			set { camera.enabled = value; }
		}

		public override void Reset()
		{
			base.Reset();

			camera.targetTexture = null;
			texture = null;

			CameraEnabled = true;
		}
	}

	public interface IFocusCameraView : IView
	{
		RenderTexture Texture { get; }
		bool CameraEnabled { get; set; }
	}
}
using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class FocusCameraView : View, IFocusCameraView
	{
		[SerializeField]
		Camera focusCamera;

		protected Camera FocusCamera { get { return focusCamera; } }

		public RenderTexture Texture { set { focusCamera.targetTexture = value; } }

		public bool CameraEnabled
		{
			get { return focusCamera.enabled; }
			set { focusCamera.enabled = value; }
		}

		public override void Reset()
		{
			base.Reset();

			CameraEnabled = true;
		}
	}

	public interface IFocusCameraView : IView
	{
		RenderTexture Texture { set; }
		bool CameraEnabled { get; set; }
	}
}
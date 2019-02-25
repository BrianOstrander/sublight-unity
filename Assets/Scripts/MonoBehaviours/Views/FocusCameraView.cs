using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class FocusCameraView : View, IFocusCameraView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Camera focusCamera;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		protected Camera FocusCamera { get { return focusCamera; } }

		public RenderTexture Texture { set { focusCamera.targetTexture = value; } }

		public bool CameraEnabled
		{
			get { return focusCamera.enabled; }
			set { focusCamera.enabled = value; }
		}

		public float FieldOfView
		{
			get { return focusCamera.fieldOfView; }
			set { focusCamera.fieldOfView = value; }
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
		float FieldOfView { get; set; }
	}
}
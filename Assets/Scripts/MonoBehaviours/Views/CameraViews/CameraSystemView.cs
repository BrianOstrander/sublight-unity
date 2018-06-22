using UnityEngine;
using UnityEngine.EventSystems;

namespace LunraGames.SpaceFarm.Views
{
	public class CameraSystemView : View, ICameraSystemView
	{
		[SerializeField]
		BaseRaycaster raycaster;
		[SerializeField]
		Transform dragRoot;
		[SerializeField]
		Transform dragForward;
		[SerializeField]
		Transform dragAxisRoot;
		[SerializeField]
		Camera voidCamera;

		public RenderTexture VoidTexture { get; private set; }
		public bool Raycasting { set { raycaster.enabled = value; } }
		public Transform DragRoot { get { return dragRoot; } }
		public Transform DragForward { get { return dragForward; } }
		public Transform DragAxisRoot { get { return dragAxisRoot; } }
		public Vector3 DragAxis { get; set; }

		public Vector3 LookingAt
		{
			get
			{
				var plane = new Plane(Vector3.up, Root.position);
				var dist = 0f;
				plane.Raycast(new Ray(DragForward.position, DragForward.forward), out dist);
				return DragForward.position + (DragForward.forward * dist);
			}
			set
			{
				Root.position = value + (Root.position - LookingAt);
			}
		}

		public override void Reset()
		{
			base.Reset();

			Raycasting = true;

			voidCamera.targetTexture = VoidTexture = new RenderTexture(Screen.width, Screen.height, 16);
		}

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(LookingAt, Vector3.up);
		}
	}

	public interface ICameraSystemView : IDragView 
	{
		RenderTexture VoidTexture { get; }
		bool Raycasting { set; }
		Vector3 LookingAt { get; set; }
	}
}
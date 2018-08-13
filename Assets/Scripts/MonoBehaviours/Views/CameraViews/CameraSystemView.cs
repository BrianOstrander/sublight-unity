using UnityEngine;
using UnityEngine.EventSystems;

namespace LunraGames.SubLight.Views
{
	public class CameraSystemView : View, ICameraSystemView
	{
		[SerializeField]
		BaseRaycaster raycaster;
		[SerializeField]
		float dragMoveScalar;
		[SerializeField]
		float dragRotateScalar;
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
		public float DragMoveScalar { get { return dragMoveScalar; } }
		public float DragRotateScalar { get { return dragRotateScalar; } }
		public Transform DragRoot { get { return dragRoot; } }
		public Transform DragForward { get { return dragForward; } }
		public Transform DragAxisRoot { get { return dragAxisRoot; } }
		public Vector3 DragAxis { get; set; }

		public Vector3 UnityPosition
		{
			get
			{
				var plane = new Plane(Vector3.up, Root.position);
				var dist = 0f;
				plane.Raycast(new Ray(DragForward.position, DragForward.forward), out dist);
				// NewY is used since sometimes this was returning very slightly negative values.
				return (DragForward.position + (DragForward.forward * dist)).NewY(0f);
			}
			set
			{
				Root.position = value + (Root.position - UnityPosition);
			}
		}

		public override void Reset()
		{
			base.Reset();

			Raycasting = true;
			UnityPosition = Vector3.zero;
			voidCamera.targetTexture = VoidTexture = new RenderTexture(Screen.width, Screen.height, 16);
		}

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(UnityPosition, Vector3.up);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(UnityPosition, DragForward.position);
			Gizmos.DrawWireCube(DragForward.position, Vector3.one);
		}
	}

	public interface ICameraSystemView : IDragView
	{
		RenderTexture VoidTexture { get; }
		bool Raycasting { set; }
		Vector3 UnityPosition { get; set; }
	}
}
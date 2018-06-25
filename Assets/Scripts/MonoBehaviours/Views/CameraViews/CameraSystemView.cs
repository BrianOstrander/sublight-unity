using UnityEngine;
using UnityEngine.EventSystems;

namespace LunraGames.SpaceFarm.Views
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
				return DragForward.position + (DragForward.forward * dist);
			}
			set
			{
				Root.position = value + (Root.position - UnityPosition);
			}
		}

		public UniversePosition UniversePosition { get; set; }

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
		}
	}

	public interface ICameraSystemView : IDragView, IGridTransform
	{
		RenderTexture VoidTexture { get; }
		bool Raycasting { set; }
	}
}
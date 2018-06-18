﻿using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemCameraView : View, ISystemCameraView
	{
		[SerializeField]
		Transform dragRoot;
		[SerializeField]
		Transform dragForward;
		[SerializeField]
		Transform dragAxisRoot;

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
		}

		//void SetLookingAt(Vector3 position)
		//{
			
		//}

		//Vector3 GetLookingAt()
		//{
			
		//}

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(LookingAt, Vector3.up);
		}
	}

	public interface ISystemCameraView : IDragView 
	{
		Vector3 LookingAt { get; set; }
	}
}
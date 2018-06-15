using UnityEngine;

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
				return Vector3.zero;
			}
			set
			{
				
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
	}

	public interface ISystemCameraView : IDragView 
	{
		Vector3 LookingAt { get; set; }
	}
}
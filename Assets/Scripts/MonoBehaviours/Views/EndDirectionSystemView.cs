using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class EndDirectionSystemView : View, IEndDirectionSystemView
	{
		[SerializeField]
		Transform rotateRoot;

		public Vector3 UnityPosition
		{
			set { Root.position = value; }
			get { return Root.position; }
		}
		public UniversePosition UniversePosition { set; get; }
		public UniversePosition EndPosition { set; get; }

		public override void Reset()
		{
			base.Reset();

			UnityPosition = Vector3.zero;
			UniversePosition = UniversePosition.Zero;
			EndPosition = UniversePosition.Zero;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			var lookRotation = (EndPosition - UniversePosition).Normalized;
			if (lookRotation == Vector3.zero) return;
			rotateRoot.forward = lookRotation;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(UnityPosition, UnityPosition + ((rotateRoot ?? Root).forward * 4f));
		}
	}

	public interface IEndDirectionSystemView : IGridTransform
	{
		UniversePosition EndPosition { set; }
	}
}
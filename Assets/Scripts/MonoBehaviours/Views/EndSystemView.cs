using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class EndSystemView : View, IEndSystemView
	{
		[SerializeField]
		Transform rotateRoot;
		[SerializeField]
		Vector3 rotateSpeed;

		public Vector3 UnityPosition
		{
			set { Root.position = value; }
			get { return Root.position; }
		}
		public UniversePosition UniversePosition { set; get; }
		public Action Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			UnityPosition = Vector3.zero;
			UniversePosition = UniversePosition.Zero;
			Click = ActionExtensions.Empty;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			rotateRoot.Rotate(rotateSpeed);
		}

		#region Events
		public void OnClick() { Click(); }
		#endregion
	}

	public interface IEndSystemView : IGridTransform
	{
		Action Click { set; }
	}
}
using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class SystemDestructionView : View, ISystemDestructionView
	{
		public float Radius { set; private get; }
		public UniversePosition UniversePosition { set; get; }
		public Action<bool> Highlight { set; private get; }
		public Action Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Radius = 0f;
			UniversePosition = UniversePosition.Zero;
			Highlight = ActionExtensions.GetEmpty<bool>();
			Click = ActionExtensions.Empty;
		}

		#region Events
		public void OnEnter() { Highlight(true); }
		public void OnExit() { Highlight(false); }
		public void OnClick() { Click(); }
		#endregion

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;

		}
	}

	public interface ISystemDestructionView : IGridTransform
	{
		float Radius { set; }
		Action<bool> Highlight { set; }
		Action Click { set; }
	}
}
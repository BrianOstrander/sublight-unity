using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public abstract class SystemMapView : View, ISystemMapView
	{
		public abstract SystemTypes SystemType { get; }
		public UniversePosition UniversePosition { set; get; }
		public Action<bool> Highlight { set; private get; }
		public Action Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			UniversePosition = UniversePosition.Zero;
			Highlight = ActionExtensions.GetEmpty<bool>();
			Click = ActionExtensions.Empty;
		}

		#region Events
		public void OnEnter() { Highlight(true); }
		public void OnExit() { Highlight(false); }
		public void OnClick() { Click(); }
		#endregion
	}

	public interface ISystemMapView : IGridTransform
	{
		SystemTypes SystemType { get; }
		Action<bool> Highlight { set; }
		Action Click { set; }
	}
}
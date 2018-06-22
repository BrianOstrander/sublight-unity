using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public enum SystemStates
	{
		Unknown = 0,
		Current = 10,
		InRange = 20,
		OutOfRange = 30,
		Destroyed = 40
	}

	public abstract class SystemView : View, ISystemView
	{

		public abstract SystemTypes SystemType { get; }

		public abstract SystemStates SystemState { set; }
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

	public interface ISystemView : IGridTransform
	{
		SystemTypes SystemType { get; }
		SystemStates SystemState { set; }
		Action<bool> Highlight { set; }
		Action Click { set; }
	}
}
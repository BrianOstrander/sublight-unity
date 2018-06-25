using System;

namespace LunraGames.SpaceFarm.Views
{
	public class DestructionOriginSystemView : View, ISystemDestructionOriginView
	{
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

	public interface ISystemDestructionOriginView : IGridTransform
	{
		Action<bool> Highlight { set; }
		Action Click { set; }
	}
}
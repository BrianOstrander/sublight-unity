using System;

namespace LunraGames.SubLight.Views
{
	public class ShadeView : CanvasView, IShadeView
	{
		public Action Click { set; private get; }

		public override void Reset()
		{
			base.Reset();

			Click = ActionExtensions.Empty;
		}

		#region Events
		public void OnClick() { Click(); }
		#endregion
	}

	public interface IShadeView : ICanvasView
	{
		Action Click { set; }
	}
}
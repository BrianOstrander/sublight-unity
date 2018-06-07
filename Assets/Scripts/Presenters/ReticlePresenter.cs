using System;
using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ReticlePresenter : Presenter<IReticleView>
	{
		DateTime latestClick;

		public ReticlePresenter()
		{
			//App.Callbacks.PointerOrientation += OnOrientation;
			App.Callbacks.Highlight += OnHighlight;
			App.Callbacks.Click += OnClick;
		}

		protected override void UnBind()
		{
			base.UnBind();

			//App.Callbacks.PointerOrientation -= OnOrientation;
			App.Callbacks.Highlight -= OnHighlight;
			App.Callbacks.Click -= OnClick;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			View.Push(ReticleStates.Idle);
			ShowView(instant: true);
		}

		#region Events
		//void OnOrientation(PointerOrientation orientation)
		//{
		//	var position = orientation.Position + (orientation.Rotation * Vector3.forward * 1f);
		//	// Unity rotates the camera for us on the actual device.
		//	View.Position = position;
		//	View.Rotation = orientation.Rotation;
		//}

		void OnHighlight(Highlight highlight)
		{
			switch (highlight.State)
			{
				case Highlight.States.Change:
				case Highlight.States.Begin:
					View.Push(ReticleStates.Highlight);
					break;
				default:
					View.Push(ReticleStates.Idle);
					break;
			}
		}

		void OnClick(Click click)
		{
			var now = DateTime.Now;
			latestClick = now;
			View.Push(ReticleStates.Click, 1, true, () => OnClickEnd(now));
		}

		void OnClickEnd(DateTime time)
		{
			if (DateTime.Compare(time, latestClick) != 0) return;
			OnHighlight(App.Callbacks.LastHighlight);
		}
		#endregion

	}
}
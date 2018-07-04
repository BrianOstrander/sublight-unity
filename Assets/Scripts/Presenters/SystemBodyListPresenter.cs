using System.Collections.Generic;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemBodyListPresenter : Presenter<ISystemBodyListView>
	{
		GameModel model;
		SystemModel destination;

		public SystemBodyListPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.FocusRequest -= OnFocus;
		}

		void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(OnDoneClick, false, false));

			View.Title = Strings.ArrivedIn(destination.Name.Value);

			var bodies = new List<LabelButtonBlock>();

			foreach (var body in destination.Bodies.Value)
			{
				switch(body.Status.Value)
				{
					case BodyStatus.UnVisited:
						body.Status.Value = BodyStatus.Visited;
						break;
				}
				bodies.Add(new LabelButtonBlock(body.Name, () => OnBodyButtonClick(body)));
			}

			View.BodyEntries = bodies.ToArray();

			View.DoneClick = OnDoneClick;
			ShowView(App.OverlayCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.SystemBodies:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var systemBodiesFocus = focus as SystemBodiesFocusRequest;
					destination = systemBodiesFocus.System;
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnBodyButtonClick(BodyModel body)
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			// TODO: probe logic
			App.Log("Focus Body", LogTypes.ToDo);
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			App.Callbacks.PopEscape();
			App.Callbacks.FocusRequest(
				new SystemsFocusRequest(
					destination.Position.Value.SystemZero,
					destination.Position.Value
				)
			);
		}
		#endregion

	}
}
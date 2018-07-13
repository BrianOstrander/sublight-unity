using System.Linq;
using System.Collections.Generic;

using UnityEngine;

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

			View.Title = Strings.ArrivedIn(destination.Name.Value);

			var bodies = new List<LabelButtonBlock>();

			foreach (var body in destination.Bodies.Value)
			{
				bodies.Add(new LabelButtonBlock(body.Name, () => OnBodyButtonClick(body)));
			}

			View.BodyEntries = bodies.ToArray();

			View.DoneClick = OnDoneClick;
			ShowView(App.GameCanvasRoot);
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
					destination = model.Universe.Value.GetSystem(systemBodiesFocus.System);
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

			App.Callbacks.FocusRequest(
				BodyFocusRequest.BodyHook(destination.Position, body.BodyId)
			);

			/*
			if (body.HasEncounter)
			{
				if (model.EncountersCompleted.Value.Contains(body.EncounterId)) Debug.Log("done encounter logic here");
				else 
				{
					App.Callbacks.FocusRequest(
						BodyFocusRequest.BodyHook(destination.Position, body.BodyId) 
					);
				}
			}
			else Debug.Log("no encounter logic here");
			*/

			/*
			switch(body.Status.Value)
			{
				case BodyStatus.NotProbed:
					App.Callbacks.FocusRequest(
						BodyFocusRequest.ProbeList(destination.Position, body.BodyId)
					);
					break;
				case BodyStatus.EncounterNotFound:
					App.Callbacks.FocusRequest(
						BodyFocusRequest.Probing(destination.Position, body.BodyId, body.ProbeId)
					);
					break;
				case BodyStatus.EncounterFound:
					Debug.Log("Lol encounter found logic here");
					break;
				case BodyStatus.EncounterExplored:
					Debug.Log("lol encounter explored logic here");
					break;
				default:
					Debug.LogError("Unhandled BodyStatus: " + body.Status.Value);
					break;
			}
			*/
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
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
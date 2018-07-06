using System.Collections.Generic;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class BodyProbeListPresenter : Presenter<IBodyProbeListView>
	{
		GameModel model;
		SystemModel system;
		BodyModel body;

		public BodyProbeListPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Rations = body.Rations - body.RationsAcquired;
			View.Fuel= body.Fuel - body.FuelAcquired;
			View.BackClick = OnBackClick;

			var buttons = new List<LabelButtonBlock>();
			foreach (var probe in model.Ship.Value.GetInventory<ProbeInventoryModel>())
			{
				if (!probe.IsExplorable(body)) continue;
				buttons.Add(new LabelButtonBlock(probe.Name, () => OnProbeClick(probe)));
			}
			View.ProbeEntries = buttons.ToArray();

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Body:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var bodyFocus = focus as BodyFocusRequest;
					// We also only show up if our view is specified
					if (bodyFocus.View != BodyFocusRequest.Views.ProbeList) goto default;
					system = model.Universe.Value.GetSystem(bodyFocus.System);
					body = system.GetBody(bodyFocus.Body);
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnProbeClick(ProbeInventoryModel probe)
		{
			App.Callbacks.FocusRequest(
				BodyFocusRequest.ProbeDetail(
					system.Position, 
					body.BodyId,
					probe.InstanceId
				)
			);
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(system.Position)
			);
		}
		#endregion

	}
}
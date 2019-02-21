using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridVelocityPresenter : SystemFocusPresenter<IGridVelocityView>
	{
		GameModel model;
		GridVelocityLanguageBlock language;

		public GridVelocityPresenter(
			GameModel model,
			GridVelocityLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			model.Ship.Velocity.Changed += OnVelocity;
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			model.Ship.Velocity.Changed -= OnVelocity;
			model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{
			OnVelocityForced(model.Ship.Velocity.Value);
			View.MultiplierSelection = OnMultiplierSelection;
			View.VelocityUnit = language.Velocity.Value.Value;
			View.ResourceUnit = language.Resource.Value.Value;
			View.ResourceWarning = language.ResourceWarning.Value.Value;

			View.PushOpacity(() => model.Context.TransitState.Value.State == TransitState.States.Active ? 0f : 1f);
		}

		#region Events
		void OnVelocity(TransitVelocity velocity)
		{
			if (!View.Visible) return;

			OnVelocityForced(velocity);
		}

		void OnVelocityForced(TransitVelocity velocity)
		{
			View.SetVelocities(velocity);
		}

		void OnMultiplierSelection(int index)
		{
			model.KeyValues.Set(KeyDefines.Game.PropellantUsage, index);
			model.Ship.SetVelocityMultiplierCurrent(index);
		}

		void OnTransitState(TransitState transitState)
		{
			if (!View.Visible) return;

			switch (transitState.State)
			{
				case TransitState.States.Request:
				case TransitState.States.Complete:
					View.SetOpacityStale();
					break;
			}
		}
		#endregion
	}
}
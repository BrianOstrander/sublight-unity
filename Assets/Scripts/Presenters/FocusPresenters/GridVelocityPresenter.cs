using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridVelocityPresenter : FocusPresenter<IGridVelocityView, SystemFocusDetails>
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

			model.Ship.Value.Velocity.Changed += OnVelocity;
		}

		protected override void OnUnBind()
		{
			model.Ship.Value.Velocity.Changed -= OnVelocity;
		}

		protected override void OnUpdateEnabled()
		{
			OnVelocityForced(model.Ship.Value.Velocity.Value);
			View.MultiplierSelection = OnMultiplierSelection;
			View.VelocityUnit = language.Velocity.Value.Value;
			View.ResourceUnit = language.Resource.Value.Value;
			View.ResourceWarning = language.ResourceWarning.Value.Value;
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
			model.Ship.Value.SetVelocityMultiplierCurrent(index);
		}
		#endregion
	}
}
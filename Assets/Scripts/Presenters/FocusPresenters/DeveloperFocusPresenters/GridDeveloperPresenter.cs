using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridDeveloperPresenter : DeveloperFocusPresenter<ISystemScratchView, SystemFocusDetails>
	{
		protected override DeveloperViews DeveloperView { get { return DeveloperViews.Grid; } }

		public GridDeveloperPresenter(GameModel model) : base(model)
		{
			App.Callbacks.KeyValueRequest += OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed += OnCelestialSystemState;
			Model.Ship.Velocity.Changed += OnVelocity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.KeyValueRequest -= OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;
			Model.Ship.Velocity.Changed -= OnVelocity;
		}

		protected override void OnUpdateEnabled()
		{
			View.Message = CurrentMessage;
		}

		string CurrentMessage
		{
			get
			{
				var result = "< Temporary UI >";

				result += "\n";

				var population = Model.KeyValues.GetFloat(KeyDefines.Game.Population.Key);
				var shipPopulationMinimum = Model.KeyValues.GetFloat(KeyDefines.Game.ShipPopulationMinimum.Key);
				var shipPopulationMaximum = Model.KeyValues.GetFloat(KeyDefines.Game.ShipPopulationMaximum.Key);

				result += "Population: " + population.ToString("N0");
				result += "\n\t" + DeveloperStrings.GetRatio(
					population,
					shipPopulationMinimum,
					shipPopulationMaximum,
					DeveloperStrings.RatioThemes.ProgressBar,
					new DeveloperStrings.RatioColor(Color.green, Color.red, true)
				);

				result += "\n";

				var rations = Model.KeyValues.GetFloat(KeyDefines.Game.Rations.Key);
				var rationsMaximum = Model.KeyValues.GetFloat(KeyDefines.Game.RationsMaximum.Key);

				result += "Rations: " + rations.ToString("N0");
				result += "\n\t" + DeveloperStrings.GetRatio(
					rations,
					0f,
					rationsMaximum,
					DeveloperStrings.RatioThemes.ProgressBar,
					new DeveloperStrings.RatioColor(Color.red, Color.green)
				);

				if (Model.Context.CelestialSystemStateLastSelected.Value.State == CelestialSystemStateBlock.States.Selected)
				{
					result += "\n";

					//var yearsTransit = RelativityUtility.TransitTime()

					result += "Transit Consumption ---";
				}

				return result + "\n";
			}
		}

		#region Events
		void OnKeyValueRequest(KeyValueRequest request)
		{
			OnRefreshMessage();
		}

		void OnCelestialSystemState(CelestialSystemStateBlock celestialSystemState)
		{
			OnRefreshMessage();
		}

		void OnVelocity(TransitVelocity velocity)
		{
			OnRefreshMessage();
		}

		void OnRefreshMessage()
		{
			if (View.Visible) View.Message = CurrentMessage;
		}
		#endregion
	}
}
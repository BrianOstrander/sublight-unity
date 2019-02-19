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
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.KeyValueRequest -= OnKeyValueRequest;
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

				var population = Model.KeyValues.GetFloat(DefinedKeyInstances.Game.Population.Key);
				var populationMinimum = Model.KeyValues.GetFloat(DefinedKeyInstances.Game.PopulationMinimum.Key);
				var populationMaximum = Model.KeyValues.GetFloat(DefinedKeyInstances.Game.PopulationMaximum.Key);

				result += "Population: " + population.ToString("N0");
				result += "\n\t" + DeveloperStrings.GetRatio(
					population,
					populationMinimum,
					populationMaximum,
					DeveloperStrings.RatioThemes.ProgressBar,
					new DeveloperStrings.RatioColor(Color.green, Color.red, true)
				);

				result += "\n";

				var rations = Model.KeyValues.GetFloat(DefinedKeyInstances.Game.Rations.Key);
				var rationsMaximum = Model.KeyValues.GetFloat(DefinedKeyInstances.Game.RationsMaximum.Key);

				result += "Rations: " + rations.ToString("N0");
				result += "\n\t" + DeveloperStrings.GetRatio(
					rations,
					0f,
					rationsMaximum,
					DeveloperStrings.RatioThemes.ProgressBar,
					new DeveloperStrings.RatioColor(Color.red, Color.green)
				);

				result += "\n";
				return result;
			}
		}

		#region Events
		void OnKeyValueRequest(KeyValueRequest request)
		{
			if (!View.Visible) return;

			View.Message = CurrentMessage;
		}
		#endregion
	}
}
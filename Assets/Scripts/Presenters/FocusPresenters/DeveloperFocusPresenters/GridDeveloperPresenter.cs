using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridDeveloperPresenter : DeveloperFocusPresenter<ISystemScratchView, SystemFocusDetails>
	{
		static class LinkIds
		{
			public const string RationingPrefix = "rationing_";

			public const string LearnMore = "learn_more";
		}

		static float GetMessageAngle(int index = 0) { return (25f + ((Mathf.Abs(index) - 1) * 30f)) * (index < 0 ? -1f : 1); }

		protected override DeveloperViews DeveloperView { get { return DeveloperViews.Grid; } }

		DeveloperMessageEntry primaryMessage;
		DeveloperMessageEntry secondaryMessage;

		Dictionary<string, Action> links;

		public GridDeveloperPresenter(GameModel model) : base(model)
		{
			App.Callbacks.KeyValueRequest += OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed += OnCelestialSystemState;
			Model.Context.TransitState.Changed += OnTransitState;
			Model.Ship.Velocity.Changed += OnVelocity;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.KeyValueRequest -= OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;
			Model.Context.TransitState.Changed -= OnTransitState;
			Model.Ship.Velocity.Changed -= OnVelocity;
		}

		protected override void OnUpdateEnabled()
		{
			primaryMessage = View.CreateEntry(
				OnClickLink,
				GetMessageAngle(1)
			);

			secondaryMessage = View.CreateEntry(
				OnClickLink,
				GetMessageAngle(2)
			);

			OnRefreshMessage(true);
		}

		/* Better to hide this until I actually need it.
		string GetLink(string linkId, Dictionary<string, Action> target, Action callback)
		{
			return GetLink(linkId, linkId, target, callback);
		}
		*/

		string GetLink(string linkId, string message, Dictionary<string, Action> target, Action callback)
		{
			if (string.IsNullOrEmpty(linkId)) throw new ArgumentException("Cannot be null or empty", "linkId");
			if (string.IsNullOrEmpty(message)) throw new ArgumentException("Cannot be null or empty", "message");
			if (target == null) throw new ArgumentNullException("target");
			if (callback == null) throw new ArgumentNullException("callback");

			if (!target.ContainsKey(linkId)) target.Add(linkId, callback);

			message = string.IsNullOrEmpty(message) ? linkId : message;
			return "<link=\"" + linkId + "\">" + message + "</link>";
		}

		string CreatePrimaryMessage(Dictionary<string, Action> target)
		{
			if (target == null) throw new ArgumentNullException("target");

			var result = string.Empty;

			if (Model.Context.TransitState.Value.State != TransitState.States.Complete) return result;

			result = GetLink(LinkIds.LearnMore, DeveloperStrings.GetSize("Temporary Interface [ Learn More ]", 0.4f), target, OnLearnMore);

			switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					return CreateMessageTransitPreview(
						result,
						target,
						Model.Context.CelestialSystemStateLastSelected.Value.System
					);
			}

			switch (Model.Context.CelestialSystemState.Value.State)
			{
				case CelestialSystemStateBlock.States.Highlighted:
					return CreateMessageTransitPreview(
						result,
						target,
						Model.Context.CelestialSystemState.Value.System
					);
			}

			return CreateMessageIdle(result, target);
		}

		string CreateMessageIdle(string result, Dictionary<string, Action> target)
		{
			var gameSource = Model.KeyValues;

			//result = AppendSystemName(result, target, "Current System", Model.Context.CurrentSystem.Value);
			result = AppendPopulationMessage(result, target, gameSource);
			result = AppendRationing(result, target, gameSource);
			result = AppendRations(result, target, gameSource);
			result = AppendPropellant(result, target, gameSource);

			return result;
		}

		string CreateMessageTransitPreview(string result, Dictionary<string, Action> target, SystemModel system)
		{
			var gameSource = Model.KeyValues;

			if (system != null)
			{
				var currVelocity = Model.Ship.Velocity.Value.Current.RelativisticLightYears;
				var currDistance = UniversePosition.ToLightYearDistance(
					UniversePosition.Distance(
						Model.Context.CurrentSystem.Value.Position.Value,
						system.Position.Value
					)
				);

				gameSource = gameSource.Duplicate;
				GameplayUtility.ApplyTransit(
					RelativityUtility.TransitTime(
						currVelocity,
						currDistance
					).ShipTime.TotalYears,
					gameSource,
					system.KeyValues.Duplicate
				);
			}

			//result = AppendSystemName(result, target, "Target System", system);
			result = AppendPopulationMessage(result, target, gameSource);
			result = AppendRationing(result, target, gameSource);
			result = AppendRations(result, target, gameSource);
			result = AppendPropellant(result, target, gameSource);

			return result;
		}

		string AppendSystemName(string result, Dictionary<string, Action> target, string prefix, SystemModel system)
		{
			result += "\n";

			result += DeveloperStrings.GetBold(prefix+": ");
			if (system == null) result += "< null system >";
			else if (system.Name.Value == null) result += "< null name >";
			else if (string.IsNullOrEmpty(system.Name.Value)) result += "< empty name >";
			else result += system.Name.Value;

			return result;
		}

		string AppendPopulationMessage(string result, Dictionary<string, Action> target, KeyValueListModel gameSource)
		{
			result += "\n";

			var population = gameSource.Get(KeyDefines.Game.Population);
			var shipPopulationMinimum = gameSource.Get(KeyDefines.Game.ShipPopulationMinimum);
			var shipPopulationMaximum = gameSource.Get(KeyDefines.Game.ShipPopulationMaximum);

			result += DeveloperStrings.GetBold("Population: ") + population.ToString("N0");

			var currentPopulation = Model.KeyValues.Get(KeyDefines.Game.Population);
			if (1f < Mathf.Abs(population - currentPopulation))
			{
				var populationDelta = population - currentPopulation;
				result += DeveloperStrings.GetColor(
					DeveloperStrings.GetSize(
						(populationDelta < 0f ? " " : " +") + populationDelta.ToString("N0"),
						0.4f
					),
					(populationDelta < 0f ? Color.red : Color.green).NewS(0.65f)
				);
			}

			result += "\n\t" + DeveloperStrings.GetRatio(
				population,
				shipPopulationMinimum,
				shipPopulationMaximum,
				DeveloperStrings.RatioThemes.ProgressBar,
				new DeveloperStrings.RatioColor(Color.green, Color.red, true)
			);

			return result;
		}

		string AppendRationing(string result, Dictionary<string, Action> target, KeyValueListModel gameSource)
		{
			var rationing = gameSource.Get(KeyDefines.Game.Rationing);
			var rationingMinimum = gameSource.Get(KeyDefines.Game.RationingMinimum);
			var rationingMaximum = gameSource.Get(KeyDefines.Game.RationingMaximum);
			var rationingDelta = (rationingMaximum - rationingMinimum) + 1;
			if (3 <= rationingDelta && rationingDelta <= 17)
			{
				result += "\n";

				var boundrySaturation = 0.45f;
				var normalSaturation = 0.65f;

				result += DeveloperStrings.GetBold("Rationing: ");

				var rationingDescription = string.Empty;

				switch (rationing)
				{
					case 2: rationingDescription = "Plentiful"; break;
					case 1: rationingDescription = "Generous"; break;
					case 0: rationingDescription = "Sufficient"; break;
					case -1: rationingDescription = "Minimal"; break;
					case -2: rationingDescription = "Meager"; break;
					default:
						rationingDescription = rationing < 0 ? "Starved" : "Gorged";
						break;
				}

				if (rationing == 0) result += rationingDescription;
				else result += DeveloperStrings.GetColor(rationingDescription, rationing < 0 ? Color.red : Color.green);

				result += "\n\t" + DeveloperStrings.GetColor("|", Color.red.NewS(boundrySaturation)) + DeveloperStrings.GetColorTagBegin(Color.red.NewS(normalSaturation));

				for (var i = rationingMinimum; i < (rationingMaximum + 1); i++)
				{
					if (i == 0) result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColorTagBegin(Color.white);

					var currRationing = string.Empty;
					if (i == rationing)
					{
						var currentColor = Color.white;
						if (i < 0) currentColor = Color.red;
						else if (0 < i) currentColor = Color.green;

						currRationing = DeveloperStrings.GetColor(" + ", currentColor);
					}
					else currRationing = " — "; // Special dash, copy paste to preserve!

					var currIndex = i;
					result += GetLink(LinkIds.RationingPrefix + i, currRationing, target, () => OnSetRationing(currIndex));

					if (i == 0) result += DeveloperStrings.GetColorTagBegin(Color.green.NewS(normalSaturation));
				}

				result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColor("|", Color.green.NewS(boundrySaturation));
			}

			return result;
		}

		string AppendRations(
			string result,
			Dictionary<string, Action> target,
			KeyValueListModel gameSource
		)
		{
			result += "\n";

			var rationsTotal = gameSource.Get(KeyDefines.Game.Rations.Amount);
			var rationsMaximum = gameSource.Get(KeyDefines.Game.Rations.Maximum);

			result += DeveloperStrings.GetBold("Rations: ") + rationsTotal.ToString("N0");

			var currentRations = Model.KeyValues.Get(KeyDefines.Game.Rations.Amount);
			if (1f < Mathf.Abs(rationsTotal - currentRations))
			{
				var rationsDelta = rationsTotal - currentRations;
				if (rationsMaximum < rationsTotal) rationsDelta = (rationsDelta + currentRations) - rationsMaximum;

				result += DeveloperStrings.GetColor(
					DeveloperStrings.GetSize(
						(rationsDelta < 0f ? " " : " +") + rationsDelta.ToString("N0"),
						0.4f
					),
					(rationsDelta < 0f ? Color.red : Color.green).NewS(0.65f)
				);
			}

			result += "\n\t" + DeveloperStrings.GetRatio(
				rationsTotal,
				0f,
				rationsMaximum,
				DeveloperStrings.RatioThemes.ProgressBar,
				new DeveloperStrings.RatioColor(Color.red, Color.green)
			);

			return result;
		}

		string AppendPropellant(string result, Dictionary<string, Action> target, KeyValueListModel gameSource)
		{
			result += "\n";

			var propellant = Mathf.FloorToInt(gameSource.Get(KeyDefines.Game.Propellant.Amount));
			var propellantMaximum = Mathf.FloorToInt(gameSource.Get(KeyDefines.Game.Propellant.Maximum));

			result += DeveloperStrings.GetBold("Propellant: ") + propellant;

			var currentPropellant = Mathf.FloorToInt(Model.KeyValues.Get(KeyDefines.Game.Propellant.Amount));
			if (0 < Mathf.Abs(propellant - currentPropellant))
			{
				var propellantDelta = propellant - currentPropellant;
				result += DeveloperStrings.GetColor(
					DeveloperStrings.GetSize(
						(propellantDelta < 0f ? " " : " +") + propellantDelta,
						0.4f
					),
					(propellantDelta < 0f ? Color.red : Color.green).NewS(0.65f)
				);
			}

			result += "\n\t" + DeveloperStrings.GetRatio(
				propellant,
				0f,
				propellantMaximum,
				DeveloperStrings.RatioThemes.ProgressBar,
				new DeveloperStrings.RatioColor(Color.red, Color.green)
			);

			return result;
		}

		string CreateSecondaryMessage(Dictionary<string, Action> target)
		{
			if (target == null) throw new ArgumentNullException("target");

			var result = string.Empty;

			switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected:
					return CreateMessageSystem(
						result,
						target,
						Model.Context.CelestialSystemStateLastSelected.Value.System
					);
			}

			switch (Model.Context.CelestialSystemState.Value.State)
			{
				case CelestialSystemStateBlock.States.Highlighted:
					return CreateMessageSystem(
						result,
						target,
						Model.Context.CelestialSystemState.Value.System
					);
			}

			return result;
		}

		string CreateMessageSystem(
			string result,
			Dictionary<string, Action> target,
			SystemModel system
		)
		{
			result = AppendSystemName(result, target, "Target System", system);

			result += "\n";

			float rationsTotal;
			float rationsFromSystem;
			GameplayUtility.ResourcesAvailable(
				Model.KeyValues,
				system.KeyValues,
				KeyDefines.Game.Rations,
				KeyDefines.CelestialSystem.Rations,
				out rationsTotal,
				out rationsFromSystem
			);

			var rationsNormal = rationsFromSystem / Model.KeyValues.Get(KeyDefines.Game.Rations.Maximum);
			var rationsPercent = Mathf.FloorToInt(rationsNormal * 100f);
			var rationsColor = rationsPercent < 10 ? Color.red : Color.green;

			result += DeveloperStrings.GetBold("Rations: ");
			if (rationsPercent == 0) result += DeveloperStrings.GetColor("NONE", Color.red);
			else if (0 < rationsFromSystem) result += DeveloperStrings.GetColor("+" + rationsPercent + DeveloperStrings.GetSize("%", 0.35f), rationsColor);
			else result += "Invalid Amount " + rationsPercent+"%";

			result += "\n";

			float propellantTotal;
			float propellantFromSystem;
			GameplayUtility.ResourcesAvailable(
				Model.KeyValues,
				system.KeyValues,
				KeyDefines.Game.Propellant,
				KeyDefines.CelestialSystem.Propellant,
				out propellantTotal,
				out propellantFromSystem
			);

			propellantFromSystem = Mathf.Floor(propellantFromSystem);
			var propellantAbsolute = Mathf.FloorToInt(propellantFromSystem);

			result += DeveloperStrings.GetBold("Propellant: ");
			if (Mathf.Approximately(0f, propellantFromSystem)) result += DeveloperStrings.GetColor("NONE", Color.red);
			else if (0f < propellantFromSystem) result += DeveloperStrings.GetColor("+" + propellantAbsolute,  propellantAbsolute < 2 ? Color.red : Color.green);
			else result += "Invalid Amount " + propellantFromSystem;

			return result;
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

		void OnVelocity(VelocityProfileState velocity)
		{
			OnRefreshMessage();
		}

		void OnTransitState(TransitState transitState)
		{
			OnRefreshMessage();
		}

		void OnRefreshMessage(bool force = false)
		{
			if (!force && !View.Visible) return;

			links = new Dictionary<string, Action>();

			primaryMessage.Message(CreatePrimaryMessage(links));
			secondaryMessage.Message(CreateSecondaryMessage(links));
		}

		void OnClickLink(string linkId)
		{
			Action callback;
			if (links.TryGetValue(linkId, out callback))
			{
				if (callback == null) Debug.LogError("A null callback was registered for linkId: " + linkId);
				else
				{
					callback();
					OnRefreshMessage();
				}
			}
			else Debug.Log("No callback was registered for linkId: " + linkId);
		}

		void OnSetRationing(int rationing)
		{
			Model.KeyValues.Set(KeyDefines.Game.Rationing, rationing);
		}

		void OnLearnMore()
		{
			App.Callbacks.DialogRequest(
				DialogRequest.Confirm(
					LanguageStringModel.Override(
						"SubLight is a work in progress, and the current interfaces do not always represent the final product." +
						"\n - Use the <b>Rationing</b> slider to select how strict your ark's rationing is"
					),
					title: LanguageStringModel.Override("Work In Progress")
				)
			);
		}
		#endregion
	}
}
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
			public const string PropellantUsagePrefix = "propellant_usage_";

			public const string LearnMore = "learn_more";
		}

		static float GetMessageAngle(int index = 0) { return (25f + ((Mathf.Abs(index) - 1) * 30f)) * (index < 0 ? -1f : 1); }

		protected override DeveloperViews DeveloperView { get { return DeveloperViews.Grid; } }

		DeveloperMessageEntry shipMessage;
		DeveloperMessageEntry resourceMessage;
		DeveloperMessageEntry systemMessage;

		Dictionary<string, Action> links;

		public GridDeveloperPresenter(GameModel model) : base(model)
		{
			App.Callbacks.KeyValueRequest += OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed += OnCelestialSystemState;
			Model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.KeyValueRequest -= OnKeyValueRequest;

			Model.Context.CelestialSystemState.Changed -= OnCelestialSystemState;
			Model.Context.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{
			shipMessage = View.CreateEntry(
				OnClickLink,
				GetMessageAngle(-1)
			);

			resourceMessage = View.CreateEntry(
				OnClickLink,
				GetMessageAngle(1)
			);

			systemMessage = View.CreateEntry(
				OnClickLink,
				GetMessageAngle(2)
			);

			OnRefreshMessage(true);
		}

		// Better to hide this until I actually need it.
		//string GetLink(string linkId, Dictionary<string, Action> target, Action callback)
		//{
		//	return GetLink(linkId, linkId, target, callback);
		//}

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

		string CreateShipMessage(
			Dictionary<string, Action> target,
			KeyValueListModel gameSource,
			SystemModel system
		)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (system == null) throw new ArgumentNullException("system");
			var result = string.Empty;

			result = GetLink(LinkIds.LearnMore, DeveloperStrings.GetSize("Temporary Interface [ Learn More ]", 0.4f), target, OnLearnMore);
			//result = AppendRationing(result, target, gameSource);
			//result = AppendPropellantUsage(result, target, gameSource);

			return result;
		}

		string CreateResourceMessage(
			Dictionary<string, Action> target,
			KeyValueListModel gameSource,
			SystemModel system
		)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (system == null) throw new ArgumentNullException("system");
			var result = string.Empty;

			//result = AppendPopulationMessage(result, target, gameSource);
			result = AppendRations(result, target, gameSource);
			result = AppendPropellant(result, target, gameSource);
			result = AppendMetallics(result, target, gameSource);

			return result;
		}

		string CreateSystemMessage(
			Dictionary<string, Action> target,
			KeyValueListModel gameSource,
			SystemModel system
		)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (system == null) throw new ArgumentNullException("system");
			var result = string.Empty;

			if (system == Model.Context.CurrentSystem.Value)
			{
				result = AppendSystemName(result, target, "Current System", system);
				return result;
			}

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

			result += DeveloperStrings.GetBold("Rations: ");
			if (rationsPercent == 0) result += DeveloperStrings.GetColor("NONE", Color.red);
			else if (0f < rationsFromSystem) result += DeveloperStrings.GetColor("+" + rationsPercent + DeveloperStrings.GetSize("%", 0.35f), Color.green);
			else result += "Invalid Amount " + rationsPercent + "%";

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

			var propellantNormal = propellantFromSystem / Model.KeyValues.Get(KeyDefines.Game.Propellant.Maximum);
			var propellantPercent = Mathf.FloorToInt(propellantNormal * 100f);

			result += DeveloperStrings.GetBold("Propellant: ");
			if (propellantPercent == 0) result += DeveloperStrings.GetColor("NONE", Color.red);
			else if (0f < propellantFromSystem) result += DeveloperStrings.GetColor("+" + propellantPercent + DeveloperStrings.GetSize("%", 0.35f), Color.green);
			else result += "Invalid Amount " + propellantPercent + "%";

			result += "\n";

			float metallicsTotal;
			float metallicsFromSystem;
			GameplayUtility.ResourcesAvailable(
				Model.KeyValues,
				system.KeyValues,
				KeyDefines.Game.Metallics,
				KeyDefines.CelestialSystem.Metallics,
				out metallicsTotal,
				out metallicsFromSystem
			);

			var metallicsNormal = metallicsFromSystem / Model.KeyValues.Get(KeyDefines.Game.Metallics.Maximum);
			var metallicsPercent = Mathf.FloorToInt(metallicsNormal * 100f);

			result += DeveloperStrings.GetBold("Metallics: ");
			if (metallicsPercent == 0) result += DeveloperStrings.GetColor("NONE", Color.red);
			else if (0 < metallicsFromSystem) result += DeveloperStrings.GetColor("+" + metallicsPercent + DeveloperStrings.GetSize("%", 0.35f), Color.green);
			else result += "Invalid Amount " + metallicsPercent + "%";

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

		//string AppendPopulationMessage(string result, Dictionary<string, Action> target, KeyValueListModel gameSource)
		//{
		//	result += "\n";

		//	var population = gameSource.Get(KeyDefines.Game.Population);
		//	var shipPopulationMinimum = gameSource.Get(KeyDefines.Game.ShipPopulationMinimum);
		//	var shipPopulationMaximum = gameSource.Get(KeyDefines.Game.ShipPopulationMaximum);

		//	result += DeveloperStrings.GetBold("Population: ") + population.ToString("N0");

		//	var currentPopulation = Model.KeyValues.Get(KeyDefines.Game.Population);
		//	if (1f < Mathf.Abs(population - currentPopulation))
		//	{
		//		var populationDelta = population - currentPopulation;
		//		result += DeveloperStrings.GetColor(
		//			DeveloperStrings.GetSize(
		//				(populationDelta < 0f ? " " : " +") + populationDelta.ToString("N0"),
		//				0.4f
		//			),
		//			(populationDelta < 0f ? Color.red : Color.green).NewS(0.65f)
		//		);
		//	}

		//	result += "\n\t" + DeveloperStrings.GetRatio(
		//		population,
		//		shipPopulationMinimum,
		//		shipPopulationMaximum,
		//		DeveloperStrings.RatioThemes.ProgressBar,
		//		new DeveloperStrings.RatioColor(Color.green, Color.red, true)
		//	);

		//	return result;
		//}

		//string AppendRationing(
		//	string result,
		//	Dictionary<string, Action> target,
		//	KeyValueListModel gameSource
		//)
		//{
		//	var rationing = gameSource.Get(KeyDefines.Game.Rationing);
		//	var rationingMinimum = gameSource.Get(KeyDefines.Game.RationingMinimum);
		//	var rationingMaximum = gameSource.Get(KeyDefines.Game.RationingMaximum);
		//	var rationingDelta = (rationingMaximum - rationingMinimum) + 1;
		//	if (3 <= rationingDelta && rationingDelta <= 17)
		//	{
		//		result += "\n";

		//		var boundrySaturation = 0.45f;
		//		var normalSaturation = 0.65f;

		//		result += DeveloperStrings.GetBold("Rationing: ");

		//		var rationingDescription = string.Empty;

		//		switch (rationing)
		//		{
		//			case 2: rationingDescription = "Plentiful"; break;
		//			case 1: rationingDescription = "Generous"; break;
		//			case 0: rationingDescription = "Sufficient"; break;
		//			case -1: rationingDescription = "Minimal"; break;
		//			case -2: rationingDescription = "Meager"; break;
		//			default:
		//				rationingDescription = rationing < 0 ? "Starved" : "Gorged";
		//				break;
		//		}

		//		if (rationing == 0) result += rationingDescription;
		//		else result += DeveloperStrings.GetColor(rationingDescription, rationing < 0 ? Color.red : Color.green);

		//		result += "\n\t" + DeveloperStrings.GetColor("|", Color.red.NewS(boundrySaturation)) + DeveloperStrings.GetColorTagBegin(Color.red.NewS(normalSaturation));

		//		for (var i = rationingMinimum; i < (rationingMaximum + 1); i++)
		//		{
		//			if (i == 0) result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColorTagBegin(Color.white);

		//			var currRationing = string.Empty;
		//			if (i == rationing)
		//			{
		//				var currentColor = Color.white;
		//				if (i < 0) currentColor = Color.red;
		//				else if (0 < i) currentColor = Color.green;

		//				currRationing = DeveloperStrings.GetColor(" + ", currentColor);
		//			}
		//			else currRationing = " — "; // Special dash, copy paste to preserve!

		//			var currIndex = i;
		//			result += GetLink(LinkIds.RationingPrefix + i, currRationing, target, () => OnSetRationing(currIndex));

		//			if (i == 0) result += DeveloperStrings.GetColorTagBegin(Color.green.NewS(normalSaturation));
		//		}

		//		result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColor("|", Color.green.NewS(boundrySaturation));
		//	}

		//	return result;
		//}

		//string AppendPropellantUsage(
		//	string result,
		//	Dictionary<string, Action> target,
		//	KeyValueListModel gameSource
		//)
		//{
		//	result += "\nPropellant Usage\n\t";

		//	var boundrySaturation = 0.45f;
		//	var normalSaturation = 0.65f;
		//	var unusedSaturation = 0.45f;

		//	result += DeveloperStrings.GetColor("|", Color.blue.NewS(boundrySaturation)) + DeveloperStrings.GetColorTagBegin(Color.blue.NewS(normalSaturation));

		//	var currentPropellant = Mathf.FloorToInt(Model.KeyValues.Get(KeyDefines.Game.Propellant.Amount));
		//	var currentPropellantUsage = Model.KeyValues.Get(KeyDefines.Game.PropellantUsage);
		//	var currentPropellantMaximum = Mathf.FloorToInt(Model.KeyValues.Get(KeyDefines.Game.Propellant.Maximum));

		//	for (var i = 1; i <= currentPropellantMaximum; i++)
		//	{
		//		if (i - 1 == currentPropellantUsage)
		//		{
		//			result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColorTagBegin(Color.blue.NewS(unusedSaturation));
		//		}

		//		if (i - 1 == currentPropellant) result += DeveloperStrings.GetColorTagBegin(Color.white);

		//		var currPropellantUsage = string.Empty;
		//		currPropellantUsage = i == currentPropellantUsage ? " + " : " — "; // Special hyphen, copy paste!

		//		var currIndex = i;
		//		result += GetLink(LinkIds.PropellantUsagePrefix + i, currPropellantUsage, target, () => OnSetPropellantUsage(currIndex));
		//	}

		//	result += DeveloperStrings.GetColorTagEnd() + DeveloperStrings.GetColor("|", Color.blue.NewS(boundrySaturation));

		//	return result;
		//}

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

			var propellant = gameSource.Get(KeyDefines.Game.Propellant.Amount);
			var propellantMaximum = gameSource.Get(KeyDefines.Game.Propellant.Maximum);

			result += DeveloperStrings.GetBold("Propellant: ") + propellant.ToString("N2");

			var currentPropellant = Model.KeyValues.Get(KeyDefines.Game.Propellant.Amount);
			if (!Mathf.Approximately(0f, propellant - currentPropellant))
			{
				var propellantDelta = propellant - currentPropellant;
				result += DeveloperStrings.GetColor(
					DeveloperStrings.GetSize(
						(propellantDelta < 0f ? " " : " +") + propellantDelta.ToString("N2"),
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

		string AppendMetallics(
			string result,
			Dictionary<string, Action> target,
			KeyValueListModel gameSource
		)
		{
			result += "\n";

			var metallicsTotal = gameSource.Get(KeyDefines.Game.Metallics.Amount);
			var metallicsMaximum = gameSource.Get(KeyDefines.Game.Metallics.Maximum);

			result += DeveloperStrings.GetBold("Metallics: ") + metallicsTotal.ToString("N0");

			var currentMetallics = Model.KeyValues.Get(KeyDefines.Game.Metallics.Amount);
			if (1f < Mathf.Abs(metallicsTotal - currentMetallics))
			{
				var metallicsDelta = metallicsTotal - currentMetallics;
				if (metallicsMaximum < metallicsTotal) metallicsDelta = (metallicsDelta + currentMetallics) - metallicsMaximum;

				result += DeveloperStrings.GetColor(
					DeveloperStrings.GetSize(
						(metallicsDelta < 0f ? " " : " +") + metallicsDelta.ToString("N0"),
						0.4f
					),
					(metallicsDelta < 0f ? Color.red : Color.green).NewS(0.65f)
				);
			}

			result += "\n\t" + DeveloperStrings.GetRatio(
				metallicsTotal,
				0f,
				metallicsMaximum,
				DeveloperStrings.RatioThemes.ProgressBar,
				new DeveloperStrings.RatioColor(Color.red, Color.green)
			);

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

		void OnTransitState(TransitState transitState)
		{
			OnRefreshMessage();
		}

		void OnRefreshMessage(bool force = false)
		{
			if (!force && !View.Visible) return;

			if (Model.Context.TransitState.Value.State != TransitState.States.Complete)
			{
				shipMessage.Message(string.Empty);
				resourceMessage.Message(string.Empty);
				systemMessage.Message(string.Empty);
				return;
			}

			links = new Dictionary<string, Action>();

			var system = Model.Context.CurrentSystem.Value;

			switch (Model.Context.CelestialSystemState.Value.State)
			{
				case CelestialSystemStateBlock.States.Highlighted:
					system = Model.Context.CelestialSystemState.Value.System;
					break;
				default:
					switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
					{
						case CelestialSystemStateBlock.States.Selected:
							system = Model.Context.CelestialSystemStateLastSelected.Value.System;
							break;
					}
					break;
			}

			var gameSource = Model.KeyValues;

			if (system != Model.Context.CurrentSystem.Value)
			{
				var currVelocity = Model.KeyValues.Get(KeyDefines.Game.TransitVelocity);
				var currDistance = UniversePosition.Distance(
					Model.Context.CurrentSystem.Value.Position.Value,
					system.Position.Value
				);

				gameSource = gameSource.Duplicate;
				GameplayUtility.ApplyTransit(
					RelativityUtility.TransitTime(
						currVelocity,
						UniversePosition.ToLightYearDistance(currDistance)
					).ShipTime.TotalYears,
					currDistance,
					gameSource,
					system.KeyValues.Duplicate
				);
			}

			shipMessage.Message(CreateShipMessage(links, gameSource, system));
			resourceMessage.Message(CreateResourceMessage(links, gameSource, system));
			systemMessage.Message(CreateSystemMessage(links, gameSource, system));
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
			//App.Callbacks.KeyValueRequest(KeyValueRequest.SetDefined(KeyDefines.Game.Rationing, rationing));
		}

		void OnSetPropellantUsage(int propellantUsage)
		{
			//App.Callbacks.KeyValueRequest(KeyValueRequest.SetDefined(KeyDefines.Game.PropellantUsage, propellantUsage));
		}

		void OnLearnMore()
		{
			App.Callbacks.DialogRequest(
				DialogRequest.Confirm(
					LanguageStringModel.Override(
						"SubLight is a work in progress, and the current interfaces do not always represent the final product."
					),
					title: LanguageStringModel.Override("Work In Progress")
				)
			);
		}
		#endregion
	}
}
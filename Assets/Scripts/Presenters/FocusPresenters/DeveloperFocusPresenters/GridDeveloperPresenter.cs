﻿using System;
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
				-0.278f
			);

			resourceMessage = View.CreateEntry(
				OnClickLink,
				0.278f
			);

			systemMessage = View.CreateEntry(
				OnClickLink,
				0.61f
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
				// TODO: I don't think I need to do this anymore...
				gameSource = gameSource.Duplicate;
			}

			shipMessage.Message(CreateShipMessage(links, gameSource, system));
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
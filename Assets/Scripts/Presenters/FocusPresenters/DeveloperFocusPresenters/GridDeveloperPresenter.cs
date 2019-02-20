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

		protected override DeveloperViews DeveloperView { get { return DeveloperViews.Grid; } }

		Dictionary<string, Action> links;

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
			View.Message = CreateMessage(out links);
			View.ClickLink = OnClickLink;
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

		string CreateMessage(out Dictionary<string, Action> target)
		{
			target = new Dictionary<string, Action>();

			var result = GetLink(LinkIds.LearnMore, DeveloperStrings.GetSize("Temporary Interface [ Learn More ]", 0.4f), target, OnLearnMore);

			switch (Model.Context.CelestialSystemStateLastSelected.Value.State)
			{
				case CelestialSystemStateBlock.States.Selected: return CreateMessageSystemSelected(result, target);
			}

			return CreateMessageIdle(result, target);
		}

		string CreateMessageIdle(string result, Dictionary<string, Action> target)
		{
			result = AppendPopulationMessage(result, target);
			result = AppendRationing(result, target);
			result = AppendRations(result, target);

			return result;
		}

		string CreateMessageSystemSelected(string result, Dictionary<string, Action> target)
		{
			result += "\nTodo";

			//result = AppendPopulationMessage(result, target);
			//result = AppendRationing(result, target);
			//result = AppendRations(result, target);

			return result;
		}

		string AppendPopulationMessage(string result, Dictionary<string, Action> target)
		{
			result += "\n";

			var population = Model.KeyValues.Get(KeyDefines.Game.Population);
			var shipPopulationMinimum = Model.KeyValues.Get(KeyDefines.Game.ShipPopulationMinimum);
			var shipPopulationMaximum = Model.KeyValues.Get(KeyDefines.Game.ShipPopulationMaximum);

			result += DeveloperStrings.GetBold("Population: ") + population.ToString("N0");
			result += "\n\t" + DeveloperStrings.GetRatio(
				population,
				shipPopulationMinimum,
				shipPopulationMaximum,
				DeveloperStrings.RatioThemes.ProgressBar,
				new DeveloperStrings.RatioColor(Color.green, Color.red, true)
			);

			return result;
		}

		string AppendRationing(string result, Dictionary<string, Action> target)
		{
			var rationing = Model.KeyValues.Get(KeyDefines.Game.Rationing);
			var rationingMinimum = Model.KeyValues.Get(KeyDefines.Game.RationingMinimum);
			var rationingMaximum = Model.KeyValues.Get(KeyDefines.Game.RationingMaximum);
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

		string AppendRations(string result, Dictionary<string, Action> target)
		{
			result += "\n";

			var rations = Model.KeyValues.Get(KeyDefines.Game.Rations);
			var rationsMaximum = Model.KeyValues.Get(KeyDefines.Game.RationsMaximum);

			result += DeveloperStrings.GetBold("Rations: ") + rations.ToString("N0");
			result += "\n\t" + DeveloperStrings.GetRatio(
				rations,
				0f,
				rationsMaximum,
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

		void OnVelocity(TransitVelocity velocity)
		{
			OnRefreshMessage();
		}

		void OnRefreshMessage()
		{
			if (!View.Visible) return;

			View.Message = CreateMessage(out links);
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
					LanguageStringModel.Override("SubLight is a work in progress, and the current interfaces do not always represent the final product."),
					title: LanguageStringModel.Override("Work In Progress")
				)
			);
		}
		#endregion
	}
}
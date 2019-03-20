using System;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class DialogPresenter : Presenter<IDialogView>
	{
		LanguageStringModel alertTitle;
		LanguageStringModel confirmTitle;

		LanguageStringModel okayDefault;
		LanguageStringModel yesDefault;
		LanguageStringModel noDefault;
		LanguageStringModel cancelDefault;

		DialogRequest lastRequest;

		public DialogPresenter(
			LanguageStringModel alertTitle,
			LanguageStringModel confirmTitle,

			LanguageStringModel okayDefault,
			LanguageStringModel yesDefault,
			LanguageStringModel noDefault,
			LanguageStringModel cancelDefault
		)
		{
			this.alertTitle = alertTitle;
			this.confirmTitle = confirmTitle;

			this.okayDefault = okayDefault;
			this.yesDefault = yesDefault;
			this.noDefault = noDefault;
			this.cancelDefault = cancelDefault;

			App.Callbacks.DialogRequest += OnDialogRequest;
			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.DialogType = lastRequest.DialogType;
			View.Style = lastRequest.Style;

			LanguageStringModel defaultTitle = alertTitle;
			LanguageStringModel defaultSuccess = yesDefault;
			LanguageStringModel defaultFailure = noDefault;
			LanguageStringModel defaultCancel = cancelDefault;

			switch (lastRequest.DialogType)
			{
				case DialogTypes.Confirm:
					defaultTitle = alertTitle;
					defaultSuccess = okayDefault;
					break;
				case DialogTypes.ConfirmDeny:
					defaultTitle = confirmTitle;
					defaultSuccess = yesDefault;
					defaultFailure = noDefault;
					break;
				case DialogTypes.ConfirmDenyCancel:
					defaultTitle = confirmTitle;
					defaultSuccess = yesDefault;
					defaultFailure = noDefault;
					defaultCancel = cancelDefault;
					break;
				default:
					Debug.LogError("Unrecognized style: " + lastRequest.Style);
					break;
			}

			View.Title = lastRequest.Title ?? defaultTitle;
			View.Message = lastRequest.Message ?? string.Empty;
			View.SuccessText = lastRequest.SuccessText ?? defaultSuccess;
			View.FailureText = lastRequest.FailureText ?? defaultFailure;
			View.CancelText = lastRequest.CancelText ?? defaultCancel;

			View.CancelClick = OnCancelClick;
			View.FailureClick = OnFailureClick;
			View.SuccessClick = OnSuccessClick;

			ShowView();
		}

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			switch (request.State)
			{
				case DialogRequest.States.Request:
					if (lastRequest.State == DialogRequest.States.Active)
					{
						Debug.LogWarning("Unable to request a new dialog to open while waiting for another one.");
						return;
					}
					App.Callbacks.DialogRequest(lastRequest = request.Duplicate(DialogRequest.States.Active));
					break;
				case DialogRequest.States.Active:
					Show();
					break;
			}
		}

		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State != EncounterRequest.States.Handle || request.LogType != EncounterLogTypes.Dialog) return;
			if (!request.TryHandle<DialogHandlerModel>(OnEncounterDialogHandle)) Debug.LogError("Unable to handle specified model");
		}

		void OnEncounterDialogHandle(DialogHandlerModel handler)
		{
			var dialog = handler.Dialog.Value;

			if (dialog.Message == GDCHackGlobals.PlanetDetectedTrigger)
			{
				var game = (App.SM.CurrentHandler as GameState).Payload.Game;
				var systemModel = game.Context.CurrentSystem.Value;

				var newMessage = string.Empty;

				var planetReadable = GDCHackGlobals.PlanetPositionReadable(systemModel.KeyValues.Get(KeyDefines.CelestialSystem.PlanetCount));
				newMessage += "Of all the planets around this system's "+systemModel.SecondaryClassification.Value.ToLower()+", the "+planetReadable+" looks the most promising.\n";

				var scanLevel = game.KeyValues.Get(KeyDefines.Game.SurfaceProbeScanLevel);

				var systemScanLevelAtmosphere = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.ScanLevelAtmosphere);
				var systemScanLevelGravity = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.ScanLevelGravity);
				var systemScanLevelTemperature = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.ScanLevelTemperature);
				var systemScanLevelWater = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.ScanLevelWater);
				var systemScanLevelResources = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.ScanLevelResources);

				var allScanLevels = new int[]
				{
					systemScanLevelAtmosphere,
					systemScanLevelGravity,
					systemScanLevelTemperature,
					systemScanLevelWater,
					systemScanLevelResources
				};

				var maxScanLevel = Mathf.Max(allScanLevels);

				var anyScansObscured = 0 < maxScanLevel;
				var anyScanOutOfRange = scanLevel < maxScanLevel;
				var anyScansObscuredButScannable = allScanLevels.Any(s => 0 < s && s <= scanLevel);
				var allScansAreInRange = allScanLevels.None(s => scanLevel < s);

				var probesLeft = game.KeyValues.Get(KeyDefines.Game.SurfaceProbeCount);
				//var anyProbesLeft = 0 < probesLeft;

				if (anyScansObscured)
				{
					if (allScansAreInRange)
					{
						newMessage += "Our orbital sensors lack the resolution to fully scan the planet, ";

						switch (probesLeft)
						{
							case 0:
								newMessage += "but you have no surface probes left to send down. If we choose to settle here we'll be taking a big risk!";
								break;
							case 1:
								newMessage += "but we only have a single surface probe left. I suggest you choose wisely before deciding to use it.";
								break;
							default:
								newMessage += "but we have (" + probesLeft + ") surface probes left, if you choose to send one down to complete our scans.";
								break;
						}
					}
					else if (anyScansObscuredButScannable)
					{
						newMessage += "The terrain and chemical composition of this planet make scanning it extraordinarily hard. Even if we sent down a surface probe, we won't know what's down there until constructing our settlement. It would be a big risk to go in blind like that. ";

						switch (probesLeft)
						{
							case 0:
								newMessage += "We don't have any surface probels left anyways.";
								break;
							case 1:
								newMessage += "If you choose to send down a surface probe it will give us a clearer, albeit incomplete, picture. However, we only have a single probe left. I suggest you use it wisely.";
								break;
							default:
								newMessage += "If you choose to send down a surface probe it will give us a clearer, albeit incomplete, picture. We have ("+probesLeft+") surface probes left.";
								break;
						}
					}
					else
					{
						newMessage += "The unusual terrain makes resolving the remaining details of this planet impossible, even with the help of a surface probe.";
					}

					newMessage += "\n";
				}
				else
				{
					newMessage += "Our orbital scanners were sufficient to resolve all the properties of this planet.\n";
				}

				newMessage += GDCHackGlobals.GetReading(
					systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableAtmosphere),
					scanLevel,
					systemScanLevelAtmosphere,
					GDCHackGlobals.HabitableAtmosphereDescriptions,
					false,
					DeveloperStrings.GetBold("Atmosphere:")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableGravity),
					scanLevel,
					systemScanLevelGravity,
					GDCHackGlobals.HabitableGravityDescriptions,
					false,
					DeveloperStrings.GetBold("Gravity:\t")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableTemperature),
					scanLevel,
					systemScanLevelTemperature,
					GDCHackGlobals.HabitableTemperatureDescriptions,
					false,
					DeveloperStrings.GetBold("Temperature:")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableWater),
					scanLevel,
					systemScanLevelWater,
					GDCHackGlobals.HabitableWaterDescriptions,
					false,
					DeveloperStrings.GetBold("Water:\t")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableResources),
					scanLevel,
					systemScanLevelResources,
					GDCHackGlobals.HabitableResourcesDescriptions,
					false,
					DeveloperStrings.GetBold("Resources:")
				) + "\n";

				dialog.Message = newMessage;
			}

			App.Callbacks.DialogRequest(new DialogRequest(
				DialogRequest.States.Request,
				dialog.DialogType,
				dialog.DialogStyle,
				string.IsNullOrEmpty(dialog.Title) ? null : LanguageStringModel.Override(dialog.Title),
				string.IsNullOrEmpty(dialog.Message) ? null : LanguageStringModel.Override(dialog.Message),
				string.IsNullOrEmpty(dialog.CancelText) ? null : LanguageStringModel.Override(dialog.CancelText),
				string.IsNullOrEmpty(dialog.FailureText) ? null : LanguageStringModel.Override(dialog.FailureText),
				string.IsNullOrEmpty(dialog.SuccessText) ? null : LanguageStringModel.Override(dialog.SuccessText),
				dialog.CancelClick,
				dialog.FailureClick,
				dialog.SuccessClick
			));
		}

		void OnCancelClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Cancel);
		}

		void OnFailureClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Failure);
		}

		void OnSuccessClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			OnClose(RequestStatus.Success);
		}

		void OnClose(RequestStatus status)
		{
			App.Callbacks.DialogRequest(lastRequest = lastRequest.Duplicate(DialogRequest.States.Completing));

			View.Closed += () => OnClosed(status);
			CloseView();
		}

		void OnClosed(RequestStatus status)
		{
			var finalRequest = lastRequest.Duplicate(DialogRequest.States.Complete);
			lastRequest = default(DialogRequest);
			App.Callbacks.DialogRequest(finalRequest);

			switch(status)
			{
				case RequestStatus.Cancel: 
					finalRequest.Cancel(); 
					break;
				case RequestStatus.Failure:
					finalRequest.Failure();
					break;
				case RequestStatus.Success:
					finalRequest.Success();
					break;
			}
			finalRequest.Done(status);
		}
		#endregion

	}
}
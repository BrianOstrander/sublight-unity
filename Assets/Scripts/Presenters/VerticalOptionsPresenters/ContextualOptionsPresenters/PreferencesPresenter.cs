using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PreferencesPresenter : ContextualOptionsPresenter
	{
		public static PreferencesPresenter CreateDefault()
		{
			var defaultToggle = new ToggleOptionLanguageBlock
			{
				MessageEnabled = LanguageStringModel.Override("ENABLED"),
				MessageDisabled = LanguageStringModel.Override("DISABLED")
			};

			return new PreferencesPresenter(
				new PreferencesLanguageBlock
				{
					Title = LanguageStringModel.Override("Preferences"),
					Back = LanguageStringModel.Override("Back"),

					Analytics = defaultToggle.Duplicate(LanguageStringModel.Override("Analytics")),
					Tutorial = defaultToggle.Duplicate(LanguageStringModel.Override("Tutorial")),
					InterfaceLarge = new ToggleOptionLanguageBlock
					{
						Message = LanguageStringModel.Override("Interface Scale"),
						MessageEnabled = LanguageStringModel.Override("LARGE"),
						MessageDisabled = LanguageStringModel.Override("NORMAL")
					},

					VersionPrefix = LanguageStringModel.Override("SubLight Version "),
					Gameplay = LanguageStringModel.Override("Gameplay"),

					CannotEditDuringGame = new DialogLanguageBlock
					{
						Title = LanguageStringModel.Override("Restricted"),
						Message = LanguageStringModel.Override("This preference can only be edited from the main menu.")
					},
					ReloadHomeRequired = new DialogLanguageBlock
					{
						Title = LanguageStringModel.Override("Reload Required"),
						Message = LanguageStringModel.Override("Your modifications require the main menu to be reloaded, please confirm your changes.")
					},
					RestartRequired = new DialogLanguageBlock
					{
						Title = LanguageStringModel.Override("Restart Required"),
						Message = LanguageStringModel.Override("Your modification require the game to restart, please confirm your changes.")
					},

					ReloadRestartConfirm = LanguageStringModel.Override("Confirm"),
					ReloadRestartDiscard = LanguageStringModel.Override("Discard"),
					ReloadRestartCancel = LanguageStringModel.Override("Cancel")
				}
			);
		}

		PreferencesLanguageBlock language;

		KeyValueListModel editedPreferences;
		bool saving;

		protected override bool NotInteractable
		{
			get
			{
				return base.NotInteractable || saving;
			}
		}

		public PreferencesPresenter(
			PreferencesLanguageBlock language
		)
		{
			this.language = language;
		}

		protected override void OnShow()
		{
			editedPreferences = editedPreferences ?? App.MetaKeyValues.PreferencesKeyValues.Duplicate;
			saving = false;

			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Preferences),
				LabelVerticalOptionsEntry.CreateBody(language.VersionPrefix.Value + BuildPreferences.Instance.Info.Version),
				GetToggle(
					language.Analytics,
					() => Analytics.enabled,
					value => Analytics.enabled = value
				),
				LabelVerticalOptionsEntry.CreateHeader(language.Gameplay),
				GetToggle(
					language.Tutorial,
					KeyDefines.Preferences.IgnoreTutorial
				),
				GetToggle(
					language.InterfaceLarge,
					KeyDefines.Preferences.InterfaceLarge
				),
				ButtonVerticalOptionsEntry.CreateButton(language.Back.Value, OnClickBack)
			);
		}

		ButtonVerticalOptionsEntry GetToggle(
			ToggleOptionLanguageBlock toggleLanguage,
			Func<bool> getValue,
			Action<bool> setValue
		)
		{
			if (getValue == null) throw new ArgumentNullException("getValue");
			if (setValue == null) throw new ArgumentNullException("setValue");

			var result = ButtonVerticalOptionsEntry.CreateToggle(
				toggleLanguage.Message,
				toggleLanguage.GetValue(getValue()),
				OnClickToggleNotSet
			);

			result.Click = () =>
			{
				if (NotInteractable) return;
				var newValue = !getValue();
				result.SetMessages(toggleLanguage.Message, toggleLanguage.GetValue(newValue));
				setValue(newValue);
			};

			return result;
		}

		ButtonVerticalOptionsEntry GetToggle(
			ToggleOptionLanguageBlock toggleLanguage,
			KeyDefinitions.Boolean keyDefine
		)
		{
			if (keyDefine == null) throw new ArgumentNullException("keyDefine");
			if (editedPreferences == null) throw new NullReferenceException("An editedPreferences must be defined");

			var interactable = ButtonVerticalOptionsEntry.InteractionStates.Interactable;

			Action click = OnClickToggleNotSet;

			if (App.SM.CurrentState == StateMachine.States.Game && KeyDefines.Preferences.ReloadGameRequired.Any(k => k.Key == keyDefine.Key && k.ValueType == keyDefine.ValueType))
			{
				interactable = ButtonVerticalOptionsEntry.InteractionStates.LooksNotInteractable;
				click = () =>
				{
					View.Closed += () => App.Callbacks.DialogRequest(
						DialogRequest.Confirm(
							language.CannotEditDuringGame.Message,
							DialogStyles.Error,
							language.CannotEditDuringGame.Title,
							ReShow,
							true
						)
					);

					CloseView();
				};
			}

			var result = ButtonVerticalOptionsEntry.CreateToggle(
				toggleLanguage.Message,
				toggleLanguage.GetValue(editedPreferences.Get(keyDefine)),
				OnClickToggleNotSet,
				interactable
			);

			if (interactable == ButtonVerticalOptionsEntry.InteractionStates.Interactable)
			{
				click = () =>
				{
					var newValue = !editedPreferences.Get(keyDefine);
					result.SetMessages(toggleLanguage.Message, toggleLanguage.GetValue(newValue));
					editedPreferences.Set(keyDefine, newValue);
				};
			}

			result.Click = () =>
			{
				if (NotInteractable) return;
				click();
			};
			return result;
		}


		#region Events
		void OnClickToggleNotSet()
		{
			Debug.LogError("Toggle click was not set");
		}

		protected override void OnClickBack()
		{
			if (NotInteractable) return;

			var deltas = editedPreferences.GetDeltas(App.MetaKeyValues.PreferencesKeyValues);

			if (deltas.None())
			{
				editedPreferences = null;
				base.OnClickBack();
				return;
			}

			Debug.Log("lol something changed");

			foreach (var delta in deltas) Debug.Log(delta.Key + " changed");

			// Should be set null when closed...
			//editedPreferences = null;
		}
		#endregion

	}
}
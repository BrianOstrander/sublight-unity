using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;
using LunraGames.SubLight.Presenters;

namespace LunraGames.SubLight
{
	public partial class HomeState
	{
		public class Focuses : StateFocuses
		{
			const float PriorityDimming = 0.25f;

			public static void InitializePresenters(HomePayload payload, Action done)
			{
				// Basics: Cameras, Room, etc
				var roomCamera = payload.MainCamera;
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;

				new GenericFocusCameraPresenter<HomeFocusDetails>(gantryAnchor, fieldOfView);

				// TODO: Main menu presenter stuff...
				new HoloPresenter();

				payload.Changelog = ChangelogPresenter.CreateDefault();

				IPresenterCloseShowOptions mainLogoPresenter;
				MainMenuOptionsPresenter mainOptionsPresenter;
				IPresenterCloseShowOptions mainGalaxyPresenter;

				payload.DelayedPresenterShows[0f] = new IPresenterCloseShowOptions[]
				{
					new FocusLipPresenter(SetFocusLayers.Home),
					mainLogoPresenter = new GenericPresenter<IMainMenuLogoView>()
				};

				payload.DelayedPresenterShows[1f] = new IPresenterCloseShowOptions[]
				{
					mainOptionsPresenter = new MainMenuOptionsPresenter(
						payload,
						new MainMenuLanguageBlock
						{
							NewGame = LanguageStringModel.Override("New Game"),
							NewGameOverwriteConfirm = new DialogLanguageBlock
							{
								Title = LanguageStringModel.Override("Overwrite Save"),
								Message = LanguageStringModel.Override("Starting a new game will overwrite your existing one.")
							},
							NewGameError = new DialogLanguageBlock
							{
								Title = LanguageStringModel.Override("Error"),
								Message = LanguageStringModel.Override("An error was encountered while trying to create your game.")
							},

							ContinueGame = LanguageStringModel.Override("Continue Game"),
							ContinueGameError = new DialogLanguageBlock
							{
								Title = LanguageStringModel.Override("Error"),
								Message = LanguageStringModel.Override("An error was encountered while trying to load your game.")
							},

							Preferences = LanguageStringModel.Override("Preferences"),
							Feedback = LanguageStringModel.Override("Feedback"),
							LearnMore = LanguageStringModel.Override("Learn More"),
							Credits = LanguageStringModel.Override("Credits"),

							Quit = LanguageStringModel.Override("Quit"),
							QuitConfirm = new DialogLanguageBlock
							{
								Title = LanguageStringModel.Override("Quit to Desktop"),
								Message = LanguageStringModel.Override("Are you sure you want to quit?")
							}
						},
						PreferencesPresenter.CreateDefault(
							() =>
							{
								var homePayload = new HomePayload();
								homePayload.MainCamera = payload.MainCamera;
								return homePayload;
							}
						),
						LearnMorePresenter.CreateDefault()
					)
				};

				payload.DelayedPresenterShows[1.5f] = new IPresenterCloseShowOptions[]
				{
					mainGalaxyPresenter = new MainMenuGalaxyPresenter(payload.DefaultGalaxy)
				};

				// Additional presenters

				payload.GamemodePortal = new GamemodePortalPresenter(
					payload.Gamemodes.ToArray(),
					new GamemodePortalLanguageBlock
					{
						Start = LanguageStringModel.Override("Start"),
						Locked = LanguageStringModel.Override("Locked"),
						Back = LanguageStringModel.Override("Back"),

						InDevelopmentDescription = LanguageStringModel.Override("<b>Unavailable:</b> In development"),
						LockedDescription = LanguageStringModel.Override("<b>Locked:</b> Requirements not met"),

						UnavailableInDevelopment = new DialogLanguageBlock
						{
							Title = LanguageStringModel.Override("In Development"),
							Message = LanguageStringModel.Override("The selected gamemode is in development, and is not yet available.")
						},
						UnavailableLocked = new DialogLanguageBlock
						{
							Title = LanguageStringModel.Override("Locked"),
							Message = LanguageStringModel.Override("The selected gamemode is locked, you need to meet the requirements before it will unlock.")
						}
					}
				);

				// TODO: This could be more elegent...

				payload.ToggleMainMenu = show =>
				{
					if (show)
					{
						mainLogoPresenter.Show();
						mainOptionsPresenter.ShowQuick();
						mainGalaxyPresenter.Show();
					}
					else
					{
						mainLogoPresenter.Close();
						mainOptionsPresenter.Close();
						mainGalaxyPresenter.Close();
					}
				};

				done();
			}

			#region Focus Building
			public static SetFocusBlock[] GetDefaultFocuses()
			{
				var results = new List<SetFocusBlock>();

				foreach (var layer in AllLayers)
				{
					switch (layer)
					{
						case SetFocusLayers.Room: results.Add(GetFocus<RoomFocusDetails>()); break;
						case SetFocusLayers.Priority: results.Add(GetFocus<PriorityFocusDetails>()); break;
						case SetFocusLayers.Home: results.Add(GetFocus<HomeFocusDetails>()); break;
						case SetFocusLayers.Toolbar:
						case SetFocusLayers.System:
						case SetFocusLayers.Communication:
						case SetFocusLayers.Ship:
						case SetFocusLayers.Encyclopedia:
							// I dunno why I do this...
							break;
						default:
							Debug.LogError("Unrecognized Layer " + layer);
							break;
					}
				}

				return results.ToArray();
			}

			static List<SetFocusBlock> GetBaseEnabledFocuses(int startIndex = -1)
			{
				var results = new List<SetFocusBlock>();

				results.Add(GetFocus<RoomFocusDetails>(startIndex, true, 1f));

				return results;
			}

			public static SetFocusBlock[] GetNoFocus()
			{
				return GetBaseEnabledFocuses().ToArray();
			}

			public static SetFocusBlock[] GetMainMenuFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<HomeFocusDetails>(0, true, 1f, true));

				return results.ToArray();
			}

			public static SetFocusBlock[] GetPriorityFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<PriorityFocusDetails>(0, true, 1f, true));
				results.Add(GetFocus<HomeFocusDetails>(1, false, PriorityDimming, false));

				return results.ToArray();
			}
			#endregion
		}
	}
}
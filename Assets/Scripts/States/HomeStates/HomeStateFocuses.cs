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

			public static void InitializePresenters(HomeState state, Action done)
			{
				var payload = state.Payload;

				// Basics: Cameras, Room, etc
				var roomCamera = payload.MainCamera;
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;
				var layer = LayerConstants.Get(SetFocusLayers.Home);

				new GenericFocusCameraPresenter<HomeFocusDetails>(gantryAnchor, fieldOfView);

				// TODO: Main menu presenter stuff...
				new HoloPresenter();

				payload.DelayedPresenterShows[1f] = new IPresenterCloseShowOptions[]
				{
					new FocusLipPresenter(SetFocusLayers.Home),
					new GenericPresenter<IMainMenuLogoView>()
				};

				payload.DelayedPresenterShows[2f] = new IPresenterCloseShowOptions[]
				{
					new MainMenuOptionsPresenter(
						new LabelButtonBlock[] {
							new LabelButtonBlock(LanguageStringModel.Override("New Game"), state.OnNewGameClick),
							new LabelButtonBlock(LanguageStringModel.Override("Continue Game"), state.OnContinueGameClick, payload.CanContinueSave)
						},
						new LabelButtonBlock[] {
							new LabelButtonBlock(LanguageStringModel.Override("Settings"), state.OnSettingsClick),
							new LabelButtonBlock(LanguageStringModel.Override("Credits"), state.OnCreditsClick),
							new LabelButtonBlock(LanguageStringModel.Override("Exit"), state.OnExitClick)
						}
					)
				};

				payload.DelayedPresenterShows[2.5f] = new IPresenterCloseShowOptions[]
				{
					new MainMenuGalaxyPresenter(payload.PreviewGalaxy)
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
						case SetFocusLayers.Communications:
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
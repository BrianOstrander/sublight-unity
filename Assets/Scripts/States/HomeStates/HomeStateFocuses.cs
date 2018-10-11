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
		public static class Focuses
		{
			static SetFocusLayers[] allLayers;
			static SetFocusLayers[] AllLayers { get { return allLayers ?? (allLayers = EnumExtensions.GetValues(SetFocusLayers.Unknown)); } }

			public static void InitializePresenters(HomeState state, Action done)
			{
				var payload = state.Payload;

				// Basics: Cameras, Room, etc
				var roomCamera = new HoloRoomFocusCameraPresenter();
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;
				var holoSurface = payload.HoloSurfaceOrigin.transform;
				var layer = "Holo" + SetFocusLayers.Home;

				new HomeFocusCameraPresenter(gantryAnchor, fieldOfView);
				new PriorityFocusCameraPresenter(gantryAnchor, fieldOfView);

				// TODO: Main menu presenter stuff...
				new HoloPresenter();

				payload.DelayedPresenterShows[1f] = new IPresenterCloseShowOptions[]
				{
					new GenericPresenter<ILipView>(layer),
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

				done();
			}

			#region Focus Building
			static SetFocusBlock GetFocus<D>(
				int order = 0,
				bool enabled = false,
				float weight = 0f,
				D details = null
			)
				where D : SetFocusDetails<D>, new()
			{
				return new SetFocusBlock(
					details ?? new D().SetDefault(),
					enabled,
					order,
					weight
				);
			}

			public static SetFocusBlock[] GetDefaultFocuses()
			{
				var results = new List<SetFocusBlock>();

				foreach (var layer in AllLayers)
				{
					switch (layer)
					{
						case SetFocusLayers.Room: results.Add(GetFocus<RoomFocusDetails>()); break;
						case SetFocusLayers.Priority: results.Add(GetFocus<PriorityFocusDetails>()); break;
						case SetFocusLayers.Home:results.Add(GetFocus<HomeFocusDetails>()); break;
						case SetFocusLayers.Toolbar:
						case SetFocusLayers.System:
						case SetFocusLayers.Communications:
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

			public static SetFocusBlock[] GetMainMenuFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<HomeFocusDetails>(0, true, 1f));

				return results.ToArray();
			}

			public static SetFocusBlock[] GetPriorityFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<PriorityFocusDetails>(0, true, 1f));
				results.Add(GetFocus<HomeFocusDetails>(1, true, 0.25f));

				return results.ToArray();
			}
			#endregion
		}
	}
}
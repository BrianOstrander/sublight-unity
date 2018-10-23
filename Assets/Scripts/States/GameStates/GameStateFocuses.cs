using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;
using LunraGames.SubLight.Presenters;

namespace LunraGames.SubLight
{
	public partial class GameState
	{
		public class Focuses : StateFocuses
		{
			public static void InitializePresenters(GameState state, Action done)
			{
				var payload = state.Payload;

				// Basics: Cameras, Room, etc
				var roomCamera = payload.MainCamera;
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;

				new GenericFocusCameraPresenter<ToolbarFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<SystemFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<CommunicationsFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<ShipFocusDetails>(gantryAnchor, fieldOfView);
				new GenericFocusCameraPresenter<EncyclopediaFocusDetails>(gantryAnchor, fieldOfView);

				new HoloPresenter();

				// All other presenters for this state...

				payload.Toolbar = new ToolbarPresenter(payload.Game);
				
				new FocusLipPresenter(SetFocusLayers.System, SetFocusLayers.Ship, SetFocusLayers.Communications, SetFocusLayers.Encyclopedia);

				new GridPresenter(payload.Game);
				new GridScalePresenter(
					payload.Game,
					LanguageStringModel.Override("Scale"),
					LanguageStringModel.Override("System"),
					LanguageStringModel.Override("Local"),
					LanguageStringModel.Override("Sector"),
					LanguageStringModel.Override("Galactic"),
					LanguageStringModel.Override("Cluster")
				);

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
						case SetFocusLayers.Toolbar: results.Add(GetFocus<ToolbarFocusDetails>()); break;
						case SetFocusLayers.System: results.Add(GetFocus<SystemFocusDetails>()); break;
						case SetFocusLayers.Communications: results.Add(GetFocus<CommunicationsFocusDetails>()); break;
						case SetFocusLayers.Ship: results.Add(GetFocus<ShipFocusDetails>()); break;
						case SetFocusLayers.Encyclopedia: results.Add(GetFocus<EncyclopediaFocusDetails>()); break;
						case SetFocusLayers.Home:
							// I don't know why I do this...
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

			//public static SetFocusBlock[] GetSystemFocus()
			//{
			//	var results = GetBaseEnabledFocuses();

			//	results.Add(GetFocus<ToolbarFocusDetails>(0, true, 1f, true));
			//	results.Add(GetFocus<SystemFocusDetails>(1, true, 1f, true));

			//	return results.ToArray();
			//}

			public static SetFocusBlock[] GetToolbarSelectionFocus(ToolbarSelections selection)
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<ToolbarFocusDetails>(0, true, 1f, true));

				switch (selection)
				{
					case ToolbarSelections.System:
						results.Add(GetFocus<SystemFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Ship:
						results.Add(GetFocus<ShipFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Communications:
						results.Add(GetFocus<CommunicationsFocusDetails>(1, true, 1f, true));
						break;
					case ToolbarSelections.Encyclopedia:
						results.Add(GetFocus<EncyclopediaFocusDetails>(1, true, 1f, true));
						break;
					default:
						Debug.LogError("Unrecognized selection: " + selection);
						break;
				}

				return results.ToArray();
			}

			public static SetFocusBlock[] GetPriorityFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<PriorityFocusDetails>(0, true, 1f, true));
				// todo: ability to dim already opened ones...
				//results.Add(GetFocus<HomeFocusDetails>(1, false, 0.25f, false));

				return results.ToArray();
			}
			#endregion
		}
	}
}
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

				new ToolbarFocusCameraPresenter(gantryAnchor, fieldOfView);

				new SystemFocusCameraPresenter(gantryAnchor, fieldOfView);
				// Todo: Ship
				new CommunicationsFocusCameraPresenter(gantryAnchor, fieldOfView);
				// Todo: Encyclopedia

				new HoloPresenter();

				// All other presenters for this state...
				new FocusLipPresenter(SetFocusLayers.System, SetFocusLayers.Communications);

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
						case SetFocusLayers.Home:
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

			public static SetFocusBlock[] GetSystemFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<ToolbarFocusDetails>(0, true, 1f, true));
				results.Add(GetFocus<SystemFocusDetails>(1, true, 1f, true));

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
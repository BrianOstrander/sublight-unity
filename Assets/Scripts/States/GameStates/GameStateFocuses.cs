using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Presenters;

namespace LunraGames.SubLight
{
	public partial class GameState
	{
		public static class Focuses
		{
			static SetFocusLayers[] allLayers;
			static SetFocusLayers[] AllLayers { get { return allLayers ?? (allLayers = EnumExtensions.GetValues(SetFocusLayers.Unknown)); } }

			public static void InitializePresenters(Action done)
			{
				// Basics: Cameras, Room, etc
				var roomCamera = new HoloRoomFocusCameraPresenter();
				var gantryAnchor = roomCamera.GantryAnchor;
				var fieldOfView = roomCamera.FieldOfView;

				new ToolbarFocusCameraPresenter(gantryAnchor, fieldOfView);
				new SystemFocusCameraPresenter(gantryAnchor, fieldOfView);
				new CommunicationsFocusCameraPresenter(gantryAnchor, fieldOfView);

				new HoloPresenter();

				// System
				new GridSystemPresenter();

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
						case SetFocusLayers.Toolbar: results.Add(GetFocus<ToolbarFocusDetails>()); break;
						case SetFocusLayers.System: results.Add(GetFocus<SystemFocusDetails>()); break;
						case SetFocusLayers.Communications: results.Add(GetFocus<CommunicationsFocusDetails>()); break;
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
				results.Add(GetFocus<ToolbarFocusDetails>(startIndex + 1, true, 1f));

				return results;
			}

			public static SetFocusBlock[] GetSystemFocus()
			{
				var results = GetBaseEnabledFocuses();
				
				results.Add(GetFocus<SystemFocusDetails>(1, true, 1f));
				
				return results.ToArray();
			}

			public static SetFocusBlock[] GetCommunicationsFocus()
			{
				var results = GetBaseEnabledFocuses();

				results.Add(GetFocus<CommunicationsFocusDetails>(1, true, 1f));

				return results.ToArray();
			}
			#endregion
		}
	}
}
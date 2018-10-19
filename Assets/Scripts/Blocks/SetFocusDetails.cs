using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public abstract class SetFocusDetailsBase
	{
		public static SetFocusLayers GetLayer<T>()
			where T : SetFocusDetails<T>, new()
		{
			var type = typeof(T);

			if (type == typeof(RoomFocusDetails)) return SetFocusLayers.Room;
			if (type == typeof(PriorityFocusDetails)) return SetFocusLayers.Priority;
			if (type == typeof(HomeFocusDetails)) return SetFocusLayers.Home;
			if (type == typeof(ToolbarFocusDetails)) return SetFocusLayers.Toolbar;
			if (type == typeof(SystemFocusDetails)) return SetFocusLayers.System;
			if (type == typeof(CommunicationsFocusDetails)) return SetFocusLayers.Communications;
			if (type == typeof(ShipFocusDetails)) return SetFocusLayers.Ship;
			if (type == typeof(EncyclopediaFocusDetails)) return SetFocusLayers.Encyclopedia;

			Debug.LogError("Unrecognized type: " + type.FullName);
			return SetFocusLayers.Unknown;
		}

		public abstract SetFocusLayers Layer { get; }
		public abstract Type DetailType { get; }
		public abstract bool HasDelta(SetFocusDetailsBase other);

		public bool Interactable;
	}

	public abstract class SetFocusDetails<T> : SetFocusDetailsBase
		where T : SetFocusDetails<T>, new()
	{
		public override Type DetailType { get { return typeof(T); } }

		public override bool HasDelta(SetFocusDetailsBase other)
		{
			if (Layer != other.Layer)
			{
				Debug.Log("Cannot get delta between details with Layer " + Layer + " and " + other.Layer);
				return false;
			}
			if (DetailType != other.DetailType)
			{
				Debug.LogError("Cannot get delta between details of types " + DetailType.FullName + " and " + other.DetailType.FullName);
				return false;
			}
			var typed = other as T;
			return Interactable != typed.Interactable || OnHasDelta(typed);
		}

		public virtual bool OnHasDelta(T other) { return false; }
	}
}
using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public abstract class SetFocusDetailsBase
	{
		public abstract SetFocusLayers Layer { get; }
		public abstract Type DetailType { get; }
		public abstract bool HasDelta(SetFocusDetailsBase other);

		public virtual SetFocusDetailsBase SetDefault()
		{
			OnSetDefault();
			return this;
		}
		
		protected virtual void OnSetDefault() {}
	}

	public abstract class SetFocusDetails<T> : SetFocusDetailsBase
		where T : SetFocusDetails<T>, new()
	{
		public override Type DetailType { get { return typeof(T); } }

		public bool Interactable;

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
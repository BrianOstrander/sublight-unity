using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	/// <summary>
	/// Base Presenter, defining its view and model interface.
	/// </summary>
	public abstract class Presenter<V> : IPresenter
		where V : class, IView
	{
		/// <summary>
		/// Gets the view using the interface defined in the presenter.
		/// </summary>
		/// <value>The view.</value>
		public V View { get; private set; }
		/// <summary>
		/// Gets the view interface's type for this presenter.
		/// </summary>
		/// <value>The view interface type.</value>
		public Type ViewInterface { get { return typeof(V); } }

		public bool UnBinded { private set; get; }

		public Presenter() : this(App.V.Get<V>()) {}

		public Presenter(V view)
		{
			Register();
			SetView (view);
		}

		protected void Register()
		{
			App.P.Register(this, () => View.TransitionState, CloseView, UnBind);
		}

		protected void SetView(V view)
		{
			if (view == null) 
			{
				Debug.LogError("Unable to get an instance of a view for " + GetType());
				return;
			}
			// TODO: Is the cast below necessary?
			View = view as V;
			View.Reset();
		}

		protected virtual Transform DefaultAnchor { get { return null; } }

		void UnBind()
		{
			if (UnBinded) return;
			UnBinded = true;
			App.V.Pool(View);
			OnUnBind();
		}

		protected virtual void OnUnBind() {}

		protected virtual void ShowView(Transform parent = null, bool instant = false)
		{
			App.V.Show(View, instant, parent ?? DefaultAnchor);
		}

		protected virtual void CloseView(bool instant = false)
		{
			App.V.Close(View, instant);
		}

		#region StateMachine Helper Methods
		protected void Push(
			Action action,
			string description,
			bool repeating = false
		)
		{
			App.SM.Push(action, GetType(), description, repeating);
		}

		protected void PushBlocking(Action<Action> action, string description)
		{
			App.SM.PushBlocking(action, GetType(), description);
		}

		protected void PushBlocking(Action action, Func<bool> condition, string description)
		{
			App.SM.PushBlocking(action, condition, GetType(), description);
		}
		#endregion
	}
}
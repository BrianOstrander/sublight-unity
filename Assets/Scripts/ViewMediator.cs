using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm
{
	public class ViewMediator
	{
		int _interactionCount;
		public int InteractionCount
		{
			get { return _interactionCount; }
			set
			{
				var count = Mathf.Max(0, value);
				var oldCount = _interactionCount;
				_interactionCount = count;
#if UNITY_EDITOR
				if (UnityEngine.Application.isPlaying)
#endif
				{
					// if last button is disabled or first button is enabled
					if ((count == 0 || oldCount == 0) && count != oldCount)
					{
						App.Callbacks.Interaction(count != 0);
					}
				}
			}
		}
		List<IView> _pool = new List<IView>();
		List<GameObject> _defaultViews = new List<GameObject>();
		List<IView> _views = new List<IView>();
		Transform storage;

		public void Initialize(List<GameObject> defaultViews, Transform viewStorage, Action<RequestStatus> done)
		{
			_pool = new List<IView>();
			_defaultViews = defaultViews;
			storage = viewStorage;
			foreach (var prefab in defaultViews) Pool(Create(prefab));
			App.Heartbeat.Update += Update;
			App.Heartbeat.LateUpdate += LateUpdate;

			done(RequestStatus.Success);
		}

		/// <summary>
		/// Get a new or pooled view
		/// </summary>
		/// <typeparam name="V">Type of view.</typeparam>
		public V Get<V>(Func<V, bool> predicate = null) where V : class, IView
		{
			Func<IView, bool> defaultPredicate = null;
			if (predicate != null)
			{
				defaultPredicate = v =>
				{
					V typed;
					try { typed = v as V; }
					catch { return false; }
					if (typed == null) return false;
					return predicate(typed);
				};
			}
			return Get(typeof(V), defaultPredicate) as V;
		}

		/// <summary>
		/// Get a new or pooled view
		/// </summary>
		/// <param name="type">Type of view.</param>
		public IView Get(Type type, Func<IView, bool> predicate = null)
		{
			IView existing = null;
			foreach (var view in _pool)
			{
				if (!type.IsAssignableFrom(view.GetType())) continue;
				if (predicate != null)
				{
					try
					{
						if (!predicate(view)) continue;
					}
					catch (Exception e)
					{
						Debug.LogException(e);
						continue;
					}
				}
				existing = view;
				break;
			}

			if (existing != null)
			{
				_pool.Remove(existing);
				return existing;
			}
			return Create(type, predicate);
		}

		IView Create(Type type, Func<IView, bool> predicate = null)
		{
			GameObject prefab = null;
			foreach (var view in _defaultViews)
			{
				var component = view.GetComponent(type);
				if (component == null) continue;
				if (predicate != null)
				{
					try 
					{
						if (!predicate(component as IView)) continue;
					}
					catch (Exception e)
					{
						Debug.LogException(e);
						continue;
					}
				}
				prefab = view;
				break;
			}
			if (prefab == null)
			{
				Debug.LogError("No view prefab with a root component implimenting " + type.FullName);
				return null;
			}
			return Create(prefab);
		}

		private IView Create(GameObject prefab)
		{
			var spawned = Object.Instantiate(prefab).GetComponent<IView>();
			spawned.gameObject.SetActive(false);
			spawned.transform.SetParent(storage);

			return spawned;
		}

		/// <summary>
		/// Return a view to the pool of views available for assignment to new presenters.
		/// </summary>
		/// <remarks>
		/// This should only be called from Presenter, or if you know what you're doing.
		/// </remarks>
		/// <param name="view">View.</param>
		public void Pool(IView view)
		{
			if (view == null)
			{
				Debug.LogError("Can't pool a null view");
				return;
			}
			if (_pool.Contains(view))
			{
				Debug.LogError("Pool already contains the view " + view.gameObject.name);
				return;
			}
			if (view.Visible) Debug.LogError("Pooling a visible view, this shouldn't happen, and may cause unintended side effects");
			_pool.Add(view);
		}

		void Closing(IView view)
		{
			// TODO: make this take into account multiple calls per frame, because Time.deltaTime is going to ruin it.
			view.Progress = Mathf.Min(view.CloseDuration, view.Progress + Time.deltaTime);
			var scalar = view.Progress / view.CloseDuration;

			view.Closing(scalar);
			if (Mathf.Approximately(1f, scalar))
			{
				_views.Remove(view);

				view.Parent = null;
				DisableAndCacheView(view);
				view.Closed();
			}
		}

		void Showing(IView view)
		{
			// TODO: make this take into account multiple calls per frame, because Time.deltaTime is going to ruin it.
			view.Progress = Mathf.Min(view.ShowDuration, view.Progress + Time.deltaTime);
			var scalar = view.Progress / view.ShowDuration;

			view.Showing(scalar);
			if (Mathf.Approximately(1f, scalar))
			{
				view.Shown();
			}
		}

		void Update(float delta)
		{
			foreach (var view in _views.ToList())
			{
				if (view.TransitionState == TransitionStates.SHOWN) 
				{
					view.Idle (delta);
					continue;
				}

				var unmodifiedView = view;
				if (unmodifiedView.TransitionState == TransitionStates.SHOWING) Showing(unmodifiedView);
				else if (unmodifiedView.TransitionState == TransitionStates.CLOSING) Closing(unmodifiedView);
				else
				{
					var error = "The view " + (unmodifiedView == null ? "null" : unmodifiedView.gameObject.name) + " with state " + unmodifiedView.TransitionState + " is still on the waldo, this should not be possible";
					Debug.LogError(error);
					_views.Remove(unmodifiedView);
				}
			}
		}

		void LateUpdate(float delta)
		{
			foreach (var view in _views.ToList())
			{
				if (view.TransitionState == TransitionStates.SHOWN) view.LateIdle(delta);
			}
		}

		void DisableAndCacheView(IView view)
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
				return;
#endif
			view.gameObject.SetActive(false);
			view.transform.SetParent(storage);
		}

		public static bool Initialized { get { throw new NotImplementedException(); } }

		public void Show(IView view, bool instant = false, Transform parent = null)
		{
			if (view.Visible)
			{
				return;
			}

			view.Parent = parent;
			view.Progress = instant ? view.ShowDuration : 0f;

			_views.Add(view);
			view.Prepare();
			// Call showing here since we want instantanious shows to actually be instantanious.
			Showing(view);
		}

		public void Close(IView view, bool instant = false)
		{
			if (view == null) throw new ArgumentNullException("view");

			switch (view.TransitionState)
			{
				case TransitionStates.CLOSED:
					return;

				case TransitionStates.UNKNOWN:
					Debug.LogWarning("Can't close a view with an unknown state", view.gameObject);
					return;
			}

			view.Progress = instant ? view.CloseDuration : 0f;
			Closing(view);
		}
	}
}

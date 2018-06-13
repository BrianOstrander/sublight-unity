using System;

using UnityEngine.SceneManagement;

namespace LunraGames.SpaceFarm
{
	public class CallbackService
	{
		#region Events
		/// <summary>
		/// When the escape key is released.
		/// </summary>
		public Action Escape = ActionExtensions.Empty;
		/// <summary>
		/// A scene loaded.
		/// </summary>
		public Action<Scene, LoadSceneMode> SceneLoad = ActionExtensions.GetEmpty<Scene, LoadSceneMode>();
		/// <summary>
		/// A scene unloaded.
		/// </summary>
		public Action<Scene> SceneUnload = ActionExtensions.GetEmpty<Scene>();
		/// <summary>
		/// The state changed.
		/// </summary>
		public Action<StateChange> StateChange = ActionExtensions.GetEmpty<StateChange>();
		/// <summary>
		/// Highlight changed.
		/// </summary>
		public Action<Highlight> Highlight = ActionExtensions.GetEmpty<Highlight>();
		/// <summary>
		/// Called when any interactable elemnts are added to the world, and false when all are removed.
		/// </summary>
		public Action<bool> Interaction = ActionExtensions.GetEmpty<bool>();
		/// <summary>
		/// On camera orientation
		/// </summary>
		public Action<CameraOrientation> CameraOrientation = ActionExtensions.GetEmpty<CameraOrientation>();
		/// <summary>
		/// On beginning a gesture.
		/// </summary>
		public Action<Gesture> BeginGesture = ActionExtensions.GetEmpty<Gesture>();
		/// <summary>
		/// On ending a gesture.
		/// </summary>
		public Action<Gesture> EndGesture = ActionExtensions.GetEmpty<Gesture>();
		/// <summary>
		/// The current gesture.
		/// </summary>
		public Action<Gesture> CurrentGesture = ActionExtensions.GetEmpty<Gesture>();
		/// <summary>
		/// Called after a click.
		/// </summary>
		public Action<Click> Click = ActionExtensions.GetEmpty<Click>();
		/// <summary>
		/// The pointer orientation, (world position, rotation, forward, screen position)
		/// </summary>
		/// <remarks>
		/// The screen position is the point at which the pointer terminates, this is dependent on an arbitrary distance.
		/// </remarks>
		public Action<PointerOrientation> PointerOrientation = ActionExtensions.GetEmpty<PointerOrientation>();
		/// <summary>
		/// The day time delta.
		/// </summary>
		public Action<DayTimeDelta> DayTimeDelta = ActionExtensions.GetEmpty<DayTimeDelta>();
		/// <summary>
		/// System highlight changed.
		/// </summary>
		public Action<SystemHighlight> SystemHighlight = ActionExtensions.GetEmpty<SystemHighlight>();
		/// <summary>
		/// Called when any details pertaining to the travel radius change.
		/// </summary>
		public Action<TravelRadiusChange> TravelRadiusChange = ActionExtensions.GetEmpty<TravelRadiusChange>();
		/// <summary>
		/// The travel progress.
		/// </summary>
		public Action<TravelProgress> TravelProgress = ActionExtensions.GetEmpty<TravelProgress>();
		/// <summary>
		/// The speed change.
		/// </summary>
		public Action<SpeedChange> SpeedChange = ActionExtensions.GetEmpty<SpeedChange>();
		#endregion

		#region Caching
		public CameraOrientation LastCameraOrientation;
		public PointerOrientation LastPointerOrientation;
		public Highlight LastHighlight;
		public Gesture LastGesture;
		public DayTimeDelta LastDayTimeDelta;
		public SystemHighlight LastSystemHighlight;
		public TravelRadiusChange LastTravelRadiusChange;
		public TravelProgress LastTravelProgress;
		public SpeedChange LastSpeedChange;
		#endregion

		public CallbackService()
		{
			SceneManager.sceneLoaded += (scene, loadMode) => SceneLoad(scene, loadMode);
			SceneManager.sceneUnloaded += scene => SceneUnload(scene);
			CameraOrientation += orientation => LastCameraOrientation = orientation;
			PointerOrientation += orientation => LastPointerOrientation = orientation;
			Highlight += highlight => LastHighlight = highlight;
			CurrentGesture += gesture => LastGesture = gesture;
			DayTimeDelta += delta => LastDayTimeDelta = delta;
			SystemHighlight += highlight => LastSystemHighlight = highlight;
			TravelRadiusChange += travelRadiusChange => LastTravelRadiusChange = travelRadiusChange;
			TravelProgress += travelProgress => LastTravelProgress = travelProgress;
			SpeedChange += speedChange => LastSpeedChange = speedChange;
		}
	}
}
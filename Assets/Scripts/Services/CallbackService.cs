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
		/// Called when a dialog is requested to be open or when one is closed.
		/// </summary>
		public Action<DialogRequest> DialogRequest = ActionExtensions.GetEmpty<DialogRequest>();
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
		public Action<TravelRequest> TravelRequest = ActionExtensions.GetEmpty<TravelRequest>();
		/// <summary>
		/// The speed change.
		/// </summary>
		public Action<SpeedRequest> SpeedRequest = ActionExtensions.GetEmpty<SpeedRequest>();
		/// <summary>
		/// Requests a camera change of position or reports a change in the cameras position.
		/// </summary>
		public Action<SystemCameraRequest> SystemCameraRequest = ActionExtensions.GetEmpty<SystemCameraRequest>();
		#endregion

		#region Genaral Caching
		public CameraOrientation LastCameraOrientation;
		public PointerOrientation LastPointerOrientation;
		public Highlight LastHighlight;
		public Gesture LastGesture;
		#endregion

		#region Game Caching
		public DayTimeDelta LastDayTimeDelta;
		public SystemHighlight LastSystemHighlight;
		public TravelRadiusChange LastTravelRadiusChange;
		public TravelRequest LastTravelRequest;
		public SpeedRequest LastSpeedRequest;
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
			TravelRequest += travelRequest => LastTravelRequest = travelRequest;
			SpeedRequest += speedRequest => LastSpeedRequest = speedRequest;
		}
	}
}
using System;

using UnityEngine.SceneManagement;

namespace LunraGames.SpaceFarm
{
	public class CallbackService
	{
		#region Events
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
		#endregion

		#region Caching
		public CameraOrientation LastCameraOrientation;
		public PointerOrientation LastPointerOrientation;
		public Highlight LastHighlight;
		public Gesture LastGesture;
		public DayTimeDelta LastDayTimeDelta;
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
		}
	}
}
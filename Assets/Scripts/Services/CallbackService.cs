using System;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

namespace LunraGames.SubLight
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
		/// Called when any interactable elemnts are added to the world, and 
		/// false when all are removed.
		/// </summary>
		public Action<bool> Interaction = ActionExtensions.GetEmpty<bool>();
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
		/// On beginning a scroll gesture.
		/// </summary>
		public Action<ScrollGesture> BeginScrollGesture = ActionExtensions.GetEmpty<ScrollGesture>();
		/// <summary>
		/// On ending a scroll gesture.
		/// </summary>
		public Action<ScrollGesture> EndScrollGesture = ActionExtensions.GetEmpty<ScrollGesture>();
		/// <summary>
		/// The current scroll gesture.
		/// </summary>
		public Action<ScrollGesture> CurrentScrollGesture = ActionExtensions.GetEmpty<ScrollGesture>();

		/// <summary>
		/// Called after a click.
		/// </summary>
		public Action<Click> Click = ActionExtensions.GetEmpty<Click>();
		/// <summary>
		/// The pointer orientation, (world position, rotation, forward, screen 
		/// position)
		/// </summary>
		/// <remarks>
		/// The screen position is the point at which the pointer terminates, 
		/// this is dependent on an arbitrary distance.
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
		/// The speed change.
		/// </summary>
		public Action<SpeedRequest> SpeedRequest = ActionExtensions.GetEmpty<SpeedRequest>();
		/// <summary>
		/// Requests or reports saving.
		/// </summary>
		public Action<SaveRequest> SaveRequest = ActionExtensions.GetEmpty<SaveRequest>();
		/// <summary>
		/// The state of game. Dialogs and similar UI will pause the game. This 
		/// is separate from pausing the ingame time.
		/// </summary>
		public Action<PlayState> PlayState = ActionExtensions.GetEmpty<PlayState>();
		/// <summary>
		/// Any key value requests are handled through here.
		/// </summary>
		public Action<KeyValueRequest> KeyValueRequest = ActionExtensions.GetEmpty<KeyValueRequest>();
		/// <summary>
		/// Called when the inventory should be cleared of unusable items and
		/// resources.
		/// </summary>
		public Action<ClearInventoryRequest> ClearInventoryRequest = ActionExtensions.GetEmpty<ClearInventoryRequest>();
		/// <summary>
		/// Called when requesting, updating, or completing an encounter.
		/// </summary>
		public Action<EncounterRequest> EncounterRequest = ActionExtensions.GetEmpty<EncounterRequest>();
		/// <summary>
		/// Called to change focuses, or set new default focuses.
		/// </summary>
		/// <remarks>
		/// Use TransitionFocusRequest to actually find out when a tranisition
		/// is in progress or done.
		/// </remarks>
		public Action<SetFocusRequest> SetFocusRequest = ActionExtensions.GetEmpty<SetFocusRequest>();
		/// <summary>
		/// Called when transitioning between focuses.
		/// </summary>
		/// <remarks>
		/// Can be used to determine if a transition is in progress or done.
		/// </remarks>
		public Action<TransitionFocusRequest> TransitionFocusRequest = ActionExtensions.GetEmpty<TransitionFocusRequest>();
		/// <summary>
		/// Called if there are focuses to gather for a set focus request.
		/// </summary>
		public Action<GatherFocusRequest> GatherFocusRequest = ActionExtensions.GetEmpty<GatherFocusRequest>();
		/// <summary>
		/// Sets what layers are active and which won't respond to input.
		/// </summary>
		public Action<InputLayerRequest> InputLayerRequest = ActionExtensions.GetEmpty<InputLayerRequest>();
		/// <summary>
		/// Called when requesting, updating, or completeing a camera mask.
		/// </summary>
		public Action<CameraMaskRequest> CameraMaskRequest = ActionExtensions.GetEmpty<CameraMaskRequest>();
		/// <summary>
		/// Requests a change to the specified color.
		/// </summary>
		public Action<HoloColorRequest> HoloColorRequest = ActionExtensions.GetEmpty<HoloColorRequest>();
		/// <summary>
		/// The camera transform request.
		/// </summary>
		public Action<CameraTransformRequest> CameraTransformRequest = ActionExtensions.GetEmpty<CameraTransformRequest>();
		#endregion

		// TODO: Think about moving these to state or GameModel...

		#region Genaral Caching
		public PointerOrientation LastPointerOrientation;
		public Highlight LastHighlight;
		public Gesture LastGesture;
		public PlayState LastPlayState;
		public HoloColorRequest LastHoloColorRequest;
		#endregion

		#region Game Caching
		public DayTimeDelta LastDayTimeDelta;
		public SpeedRequest LastSpeedRequest;
		#endregion

		public CallbackService()
		{
			SceneManager.sceneLoaded += (scene, loadMode) => SceneLoad(scene, loadMode);
			SceneManager.sceneUnloaded += scene => SceneUnload(scene);
			PointerOrientation += orientation => LastPointerOrientation = orientation;
			Highlight += highlight => LastHighlight = highlight;
			CurrentGesture += gesture => LastGesture = gesture;
			DayTimeDelta += delta => LastDayTimeDelta = delta;
			SpeedRequest += speedRequest => LastSpeedRequest = speedRequest;
			PlayState += state => LastPlayState = state;
			HoloColorRequest += request => LastHoloColorRequest = request;
		}
	}
}
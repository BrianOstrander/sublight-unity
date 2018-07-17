using System;
using System.Collections.Generic;

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
		/// Called when any interactable elemnts are added to the world, and 
		/// false when all are removed.
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
		/// The pointer orientation, (world position, rotation, forward, screen 
		/// position)
		/// </summary>
		/// <remarks>
		/// The screen position is the point at which the pointer terminates, 
		/// this is dependent on an arbitrary distance.
		/// </remarks>
		public Action<PointerOrientation> PointerOrientation = ActionExtensions.GetEmpty<PointerOrientation>();
		/// <summary>
		/// Obscures the raycasting on cameras.
		/// </summary>
		public Action<ObscureCameraRequest> ObscureCameraRequest = ActionExtensions.GetEmpty<ObscureCameraRequest>();
		/// <summary>
		/// Requests the shade presenter to shade or unshade the interface.
		/// </summary>
		public Action<ShadeRequest> ShadeRequest = ActionExtensions.GetEmpty<ShadeRequest>();
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
		/// The speed change.
		/// </summary>
		public Action<SpeedRequest> SpeedRequest = ActionExtensions.GetEmpty<SpeedRequest>();
		/// <summary>
		/// Requests a camera change of position or reports a change in the 
		/// camera's position.
		/// </summary>
		public Action<CameraSystemRequest> CameraSystemRequest = ActionExtensions.GetEmpty<CameraSystemRequest>();
		/// <summary>
		/// Requests or reports saving.
		/// </summary>
		public Action<SaveRequest> SaveRequest = ActionExtensions.GetEmpty<SaveRequest>();
		/// <summary>
		/// Called when the void's render texture is updated.
		/// </summary>
		public Action<VoidRenderTexture> VoidRenderTexture = ActionExtensions.GetEmpty<VoidRenderTexture>();
		/// <summary>
		/// Called every time the offset of the universe is updated.
		/// </summary>
		public Action<UniversePositionRequest> UniversePositionRequest = ActionExtensions.GetEmpty<UniversePositionRequest>();
		/// <summary>
		/// The focus of the game, can be cast to retrieve more information.
		/// </summary>
		public Action<FocusRequest> FocusRequest = ActionExtensions.GetEmpty<FocusRequest>();
		/// <summary>
		/// The state of game. Dialogs and similar UI will pause the game. This 
		/// is separate from pausing the ingame time.
		/// </summary>
		public Action<PlayState> PlayState = ActionExtensions.GetEmpty<PlayState>();
		/// <summary>
		/// Any key value requests are handled through here.
		/// </summary>
		public Action<KeyValueRequest> KeyValueRequest = ActionExtensions.GetEmpty<KeyValueRequest>();
		#endregion

		// TODO: Think about moving these to state or GameModel...

		#region Genaral Caching
		public CameraOrientation LastCameraOrientation;
		public PointerOrientation LastPointerOrientation;
		public Highlight LastHighlight;
		public Gesture LastGesture;
		public ObscureCameraRequest LastObscureCameraRequest;
		public ShadeRequest LastShadeRequest;
		public PlayState LastPlayState;
		#endregion

		#region Game Caching
		public DayTimeDelta LastDayTimeDelta;
		public SystemHighlight LastSystemHighlight;
		public SpeedRequest LastSpeedRequest;
		public VoidRenderTexture LastVoidRenderTexture;
		public UniversePositionRequest LastUniversePositionRequest;
		#endregion

		Stack<EscapeEntry> escapes = new Stack<EscapeEntry>();

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
			SpeedRequest += speedRequest => LastSpeedRequest = speedRequest;
			ObscureCameraRequest += obscureCameraRequest => LastObscureCameraRequest = obscureCameraRequest;
			ShadeRequest += shadeRequest => LastShadeRequest = shadeRequest;
			VoidRenderTexture += texture => LastVoidRenderTexture = texture;
			UniversePositionRequest += request => LastUniversePositionRequest = request;
			PlayState += state => LastPlayState = state;

			Escape += OnEscape;
		}

		#region Internal Events
		void OnEscape()
		{
			if (0 < escapes.Count && escapes.Peek().Enabled())
			{
				var escape = escapes.Pop();
				if (escape.IsShaded.HasValue)
				{
					if (escape.IsShaded.Value) ShadeRequest(SpaceFarm.ShadeRequest.Shade);
					else ShadeRequest(SpaceFarm.ShadeRequest.UnShade);
				}
				if (escape.IsObscured.HasValue)
				{
					if (escape.IsShaded.Value) ObscureCameraRequest(SpaceFarm.ObscureCameraRequest.Obscure);
					else ObscureCameraRequest(SpaceFarm.ObscureCameraRequest.UnObscure);
				}
				escape.Escape();
			}
		}
  		#endregion

		public void PushEscape(EscapeEntry escape)
		{
			escapes.Push(escape);
		}

		public void PopEscape()
		{
			if (0 < escapes.Count) escapes.Pop();
		}

		public void ClearEscapables()
		{
			ShadeRequest(SpaceFarm.ShadeRequest.UnShade);
			ObscureCameraRequest(SpaceFarm.ObscureCameraRequest.UnObscure);
			escapes.Clear();
		}
	}
}
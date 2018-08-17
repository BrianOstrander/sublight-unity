using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	public class SystemsFocusRequest : FocusRequest
	{
		public override Focuses Focus { get { return Focuses.Systems; } }

		[JsonProperty] public readonly UniversePosition FocusedSector;
		[JsonProperty] public readonly UniversePosition CameraFocus;

		public SystemsFocusRequest(
			UniversePosition focusedSector, 
			UniversePosition cameraFocus,
			States state = States.Request
		) : base(state) 
		{
			FocusedSector = focusedSector;
			CameraFocus = cameraFocus;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new SystemsFocusRequest(
				FocusedSector, 
				CameraFocus,
				state == States.Unknown ? State : state
			);
		}
	}
}
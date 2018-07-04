namespace LunraGames.SpaceFarm
{
	public class SystemsFocusRequest : FocusRequest
	{
		public readonly UniversePosition FocusedSector;
		public readonly UniversePosition CameraFocus;

		public SystemsFocusRequest(
			UniversePosition focusedSector, 
			UniversePosition cameraFocus,
			States state = States.Request
		) : base(Focuses.Systems, state) 
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
namespace LunraGames.SubLight
{
	public class RoomFocusDetails : SetFocusDetails<RoomFocusDetails>
	{
		public override SetFocusLayers Layer { get { return SetFocusLayers.Room; } }
	}
}
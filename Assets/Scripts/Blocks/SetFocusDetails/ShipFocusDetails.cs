namespace LunraGames.SubLight
{
	public class ShipFocusDetails : SetFocusDetails<ShipFocusDetails>
	{
		public override SetFocusLayers Layer { get { return SetFocusLayers.Ship; } }
	}
}
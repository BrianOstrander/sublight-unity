namespace LunraGames.SubLight
{
	public class HomeFocusDetails : SetFocusDetails<HomeFocusDetails>
	{
		public override SetFocusLayers Layer { get { return SetFocusLayers.Home; } }
	}
}
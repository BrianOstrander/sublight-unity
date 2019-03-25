namespace LunraGames.SubLight.Models
{
	public class GridModel : Model
	{
		float hazardOffset;
		public readonly ListenerProperty<float> HazardOffset;

		public GridModel()
		{
			HazardOffset = new ListenerProperty<float>(value => hazardOffset = value, () => hazardOffset);
		}
	}
}
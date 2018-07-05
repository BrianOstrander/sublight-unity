namespace LunraGames.SpaceFarm.Models
{
	public interface IExplorableInventoryModel : IModel
	{
		bool IsExplorable(BodyModel body);
	}
}
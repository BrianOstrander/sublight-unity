namespace LunraGames.SubLight.Models
{
	public interface IExplorableInventoryModel : IModel
	{
		bool IsExplorable(BodyModel body);
	}
}
namespace LunraGames.SubLight.Models
{
	public interface IEdgeModel : IModel
	{
		string EdgeName { get; }
		int EdgeIndex { get; set; }
		string EdgeId { get; set; }
	}
}

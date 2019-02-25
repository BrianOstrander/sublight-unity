namespace LunraGames.SubLight.Models
{
	public interface IEdgedEncounterLogModel<E> : IModel
		where E : IEdgeModel
	{
		E[] Edges { get; set; }
	}
}

namespace LunraGames.SubLight.Models
{
	public interface IEdgedEncounterLogModel<E> : IModel
		where E : EdgeModel
	{
		E[] Edges { get; set; }
	}
}

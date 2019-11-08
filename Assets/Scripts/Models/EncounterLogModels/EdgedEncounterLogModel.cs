using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class EdgedEncounterLogModel<E> : EncounterLogModel
		where E : EdgeModel
	{
		[JsonProperty] E[] edges = new E[0];
		[JsonIgnore] public readonly ListenerProperty<E[]> Edges;

		public EdgedEncounterLogModel()
		{
			Edges = new ListenerProperty<E[]>(value => edges = value, () => edges);
		}
	}
}
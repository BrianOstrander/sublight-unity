using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public interface IModelDuplicatable<M> : IModel
		where M : IModel
	{
		IModel RawDuplicate { get; }
		M Duplicate { get; }
	}

	public abstract class ModelDuplicatable<M> : Model, IModelDuplicatable<M>
		where M : IModelDuplicatable<M>, new()
	{
		[JsonIgnore]
		public virtual M Duplicate
		{
			get
			{
				var duplicate = new M();
				duplicate.Id.Value = Id;
				return duplicate;
			}
		}

		[JsonIgnore]
		public IModel RawDuplicate { get { return Duplicate; } }
	}
}

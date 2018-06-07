namespace LunraGames.SpaceFarm
{
	// TODO: Unclear if we need this?
	public interface IModel 
	{
		ModelProperty<string> Id { get; }
	}

	public abstract class Model : IModel
	{
		readonly ModelProperty<string> id = new ModelProperty<string>();
		public ModelProperty<string> Id { get { return id; } }
	}
}

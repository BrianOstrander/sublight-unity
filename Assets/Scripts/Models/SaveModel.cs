using System;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class SaveModel : Model
	{
		/// <summary>
		/// Gets the type of the save.
		/// </summary>
		/// <value>The type of the save.</value>
		public abstract SaveTypes SaveType { get; }
		/// <summary>
		/// Is this loadable, or is the version too old.
		/// </summary>
		public readonly ModelProperty<bool> SupportedVersion = new ModelProperty<bool>();

		/// <summary>
		/// The version of the app this was saved under.
		/// </summary>
		public readonly ModelProperty<int> Version = new ModelProperty<int>();
		/// <summary>
		/// The name.
		/// </summary>
		public readonly ModelProperty<string> Meta = new ModelProperty<string>();
		/// <summary>
		/// The path of this save, depends on the SaveLoadService in use.
		/// </summary>
		public readonly ModelProperty<string> Path = new ModelProperty<string>();
		/// <summary>
		/// When this was created and saved.
		/// </summary>
		/// <remarks>
		/// If this is equal to DateTime.MinValue it has never been saved.
		/// </remarks>
		public readonly ModelProperty<DateTime> Created = new ModelProperty<DateTime>();
		/// <summary>
		/// When this was last modified and saved.
		/// </summary>
		/// <remarks>
		/// If this is equal to DateTime.MinValue it has never been saved.
		/// </remarks>
		public readonly ModelProperty<DateTime> Modified = new ModelProperty<DateTime>();
	}
}
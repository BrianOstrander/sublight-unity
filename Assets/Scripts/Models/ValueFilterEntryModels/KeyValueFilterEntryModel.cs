using System;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel, IKeyValueFilterEntryModel
		where T : IConvertible
	{
		public KeyValueAddress<T> Input0 { get; set; }
		public KeyValueAddress<T> Input1 { get; set; }
	}

	public interface IKeyValueFilterEntryModel : IValueFilterEntryModel
	{
 	
	}
}
using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel, IKeyValueFilterEntryModel
		where T : IConvertible
	{
		[JsonProperty] KeyValueAddress<T> input0 = KeyValueAddress<T>.Foreign(KeyValueTargets.Unknown, null);
		[JsonIgnore] public ListenerProperty<KeyValueAddress<T>> Input0;

		[JsonProperty] KeyValueAddress<T> input1 = KeyValueAddress<T>.Default;
		[JsonIgnore] public ListenerProperty<KeyValueAddress<T>> Input1;

		[JsonIgnore]
		public IKeyValueAddress Input0Address { get { return Input0.Value; } }
		[JsonIgnore]
		public IKeyValueAddress Input1Address { get { return Input1.Value; } }

		public void SetInput0(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null)
		{
			SetAddress(ref Input0, source, foreignTarget, foreignKey);
		}

		public void SetInput1(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null)
		{
			SetAddress(ref Input1, source, foreignTarget, foreignKey);
		}

		void SetAddress(
			ref ListenerProperty<KeyValueAddress<T>> property,
			KeyValueSources source,
			KeyValueTargets foreignTarget,
			string foreignKey
		)
		{
			var curr = property.Value;
			curr.Source = source;
			curr.ForeignTarget = foreignTarget;
			curr.ForeignKey = foreignKey;
			property.Value = curr;
		}

		public KeyValueFilterEntryModel()
		{
			Input0 = new ListenerProperty<KeyValueAddress<T>>(value => input0 = value, () => input0);
			Input1 = new ListenerProperty<KeyValueAddress<T>>(value => input1 = value, () => input1);
		}
	}

	public interface IKeyValueFilterEntryModel : IValueFilterEntryModel
	{
		IKeyValueAddress Input0Address { get; }
		IKeyValueAddress Input1Address { get; }

		void SetInput0(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null);
		void SetInput1(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null);
	}
}
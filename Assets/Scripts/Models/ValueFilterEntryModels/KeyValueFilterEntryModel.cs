using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel, IKeyValueFilterEntryModel
	{
		[JsonProperty] KeyValueAddress<T> operand = KeyValueAddress<T>.Foreign(KeyValueTargets.Unknown, null);
		[JsonIgnore] public ListenerProperty<KeyValueAddress<T>> Operand;

		[JsonProperty] KeyValueAddress<T> input = KeyValueAddress<T>.Default;
		[JsonIgnore] public ListenerProperty<KeyValueAddress<T>> Input;

		[JsonIgnore]
		public IKeyValueAddress OperandAddress { get { return Operand.Value; } }
		[JsonIgnore]
		public IKeyValueAddress InputAddress { get { return Input.Value; } }

		public void SetOperand(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null)
		{
			SetAddress(ref Operand, source, foreignTarget, foreignKey);
		}

		public void SetInput(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null)
		{
			SetAddress(ref Input, source, foreignTarget, foreignKey);
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
			Operand = new ListenerProperty<KeyValueAddress<T>>(value => operand = value, () => operand);
			Input = new ListenerProperty<KeyValueAddress<T>>(value => input = value, () => input);
		}
	}

	public interface IKeyValueFilterEntryModel : IValueFilterEntryModel
	{
		IKeyValueAddress OperandAddress { get; }
		IKeyValueAddress InputAddress { get; }

		void SetOperand(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null);
		void SetInput(KeyValueSources source, KeyValueTargets foreignTarget = KeyValueTargets.Unknown, string foreignKey = null);
	}
}
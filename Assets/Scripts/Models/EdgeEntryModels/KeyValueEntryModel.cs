using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEntryModel : EdgeEntryModel
	{
		public interface BaseBlock<T>
			where T : IConvertible
		{
			KeyValueAddress<T> Input0 { get; set; }

			KeyValueAddress<T> Output { get; set; }
		}

		[Serializable]
		public class BooleanBlock : BaseBlock<bool>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10,
				And = 20,
				Or = 30,
				Xor = 40
			}

			public static BooleanBlock Default
			{
				get
				{
					return new BooleanBlock
					{
						Operation = Operations.Set,
						Input0 = KeyValueAddress<bool>.Default,
						Input1 = KeyValueAddress<bool>.Default,
						Output = KeyValueAddress<bool>.Default
					};
				}
			}

			public Operations Operation { get; set; }

			public KeyValueAddress<bool> Input0 { get; set; }
			public KeyValueAddress<bool> Input1 { get; set; }

			public KeyValueAddress<bool> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable { get { return Operation.ToString().ToUpper(); } }
		}

		[Serializable]
		public class IntegerBlock : BaseBlock<int>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10,
				Add = 20,
				Subtract = 30,
				Multiply = 40,
				Divide = 50,
				Modulo = 60,

				Clamp = 100
			}

			public static IntegerBlock Default
			{
				get
				{
					return new IntegerBlock
					{
						Operation = Operations.Set,
						Input0 = KeyValueAddress<int>.Default,
						Input1 = KeyValueAddress<int>.Default,
						MinimumClampingEnabled = false,
						MaximumClampingEnabled = false,
						MinimumClamping = KeyValueAddress<int>.Default,
						MaximumClamping = KeyValueAddress<int>.Default,
						Output = KeyValueAddress<int>.Default
					};
				}
			}

			public Operations Operation { get; set; }

			public KeyValueAddress<int> Input0 { get; set; }
			public KeyValueAddress<int> Input1 { get; set; }

			public bool MinimumClampingEnabled { get; set; }
			public bool MaximumClampingEnabled { get; set; }

			public KeyValueAddress<int> MinimumClamping { get; set; }
			public KeyValueAddress<int> MaximumClamping { get; set; }

			public KeyValueAddress<int> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable
			{
				get
				{
					switch (Operation)
					{
						case Operations.Add: return "+";
						case Operations.Subtract: return "-";
						case Operations.Multiply: return "*";
						case Operations.Divide: return "/";
						case Operations.Modulo: return "%";
					}
					return Operation.ToString().ToUpper();
				}
			}
		}

		[Serializable]
		public class StringBlock : BaseBlock<string>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static StringBlock Default
			{
				get
				{
					return new StringBlock
					{
						Operation = Operations.Set,
						Input0 = KeyValueAddress<string>.Default,
						Output = KeyValueAddress<string>.Default
					};
				}
			}

			public Operations Operation { get; set; }

			public KeyValueAddress<string> Input0 { get; set; }

			public KeyValueAddress<string> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable { get { return Operation.ToString().ToUpper(); } }
		}

		[Serializable]
		public class FloatBlock : BaseBlock<float>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10,
				Add = 20,
				Subtract = 30,
				Multiply = 40,
				Divide = 50,
				Modulo = 60,

				Clamp = 100,
				Round = 110,
				Floor = 120,
				Ceiling = 130
			}

			public static FloatBlock Default
			{
				get
				{
					return new FloatBlock
					{
						Operation = Operations.Set,
						Input0 = KeyValueAddress<float>.Default,
						Input1 = KeyValueAddress<float>.Default,
						MinimumClampingEnabled = false,
						MaximumClampingEnabled = false,
						MinimumClamping = KeyValueAddress<float>.Default,
						MaximumClamping = KeyValueAddress<float>.Default,
						Output = KeyValueAddress<float>.Default
					};
				}
			}

			public Operations Operation { get; set; }

			public KeyValueAddress<float> Input0 { get; set; }
			public KeyValueAddress<float> Input1 { get; set; }

			public bool MinimumClampingEnabled { get; set; }
			public bool MaximumClampingEnabled { get; set; }

			public KeyValueAddress<float> MinimumClamping { get; set; }
			public KeyValueAddress<float> MaximumClamping { get; set; }

			public KeyValueAddress<float> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable
			{
				get
				{
					switch (Operation)
					{
						case Operations.Add: return "+";
						case Operations.Subtract: return "-";
						case Operations.Multiply: return "*";
						case Operations.Divide: return "/";
						case Operations.Modulo: return "%";
					}
					return Operation.ToString().ToUpper();
				}
			}
		}

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering { get { return filtering; } }

		[JsonProperty] KeyValueTypes keyValueType;
		[JsonIgnore] public readonly ListenerProperty<KeyValueTypes> KeyValueType;

		[JsonProperty] BooleanBlock booleanValue;
		[JsonIgnore] public readonly ListenerProperty<BooleanBlock> BooleanValue;

		[JsonProperty] IntegerBlock integerValue;
		[JsonIgnore] public readonly ListenerProperty<IntegerBlock> IntegerValue;
		
		[JsonProperty] StringBlock stringValue;
		[JsonIgnore] public readonly ListenerProperty<StringBlock> StringValue;

		[JsonProperty] FloatBlock floatValue;
		[JsonIgnore] public readonly ListenerProperty<FloatBlock> FloatValue;

		[JsonIgnore]
		public string Name
		{
			get
			{
				switch (KeyValueType.Value)
				{
					case KeyValueTypes.Boolean: return KeyValueType.Value + "." + BooleanValue.Value.Operation;
					case KeyValueTypes.Integer:
					case KeyValueTypes.Enumeration:
						return KeyValueType.Value + "." + IntegerValue.Value.Operation;
					case KeyValueTypes.String: return KeyValueType.Value + "." + StringValue.Value.Operation;
					case KeyValueTypes.Float: return KeyValueType.Value + "." + FloatValue.Value.Operation;
					default:
						Debug.LogError("Unrecognized KeyValueType: " + KeyValueType.Value);
						return KeyValueType.Value.ToString();
				}
			}
		}

		public KeyValueEntryModel()
		{
			KeyValueType = new ListenerProperty<KeyValueTypes>(value => keyValueType = value, () => keyValueType);
			BooleanValue = new ListenerProperty<BooleanBlock>(value => booleanValue = value, () => booleanValue);
			IntegerValue = new ListenerProperty<IntegerBlock>(value => integerValue = value, () => integerValue);
			StringValue = new ListenerProperty<StringBlock>(value => stringValue = value, () => stringValue);
			FloatValue = new ListenerProperty<FloatBlock>(value => floatValue = value, () => floatValue);
		}
	}
}
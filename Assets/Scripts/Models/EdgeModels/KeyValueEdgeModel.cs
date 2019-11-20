using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEdgeModel : EdgeModel
	{
		/// <summary>
		/// This is an interface to enforce compliance with required fields that should not be serialized as generic.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public interface IBaseBlock<T>
			where T : IConvertible
		{
			KeyValueAddress<T> Input0 { get; set; }

			KeyValueAddress<T> Output { get; set; }
			
			string OperationReadable { get; }
			string OperationTooltip { get; }
			GUIContent OperationLabelContent { get; }
		}

		[Serializable]
		public class BooleanBlock : IBaseBlock<bool>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10,
				And = 20,
				Or = 30,
				Xor = 40,

				Random = 100
			}

			public static BooleanBlock Default => new BooleanBlock
			{
				Operation = Operations.Set,
				Input0 = KeyValueAddress<bool>.Default,
				Input1 = KeyValueAddress<bool>.Default,
				Output = KeyValueAddress<bool>.Default
			};

			public Operations Operation { get; set; }

			public KeyValueAddress<bool> Input0 { get; set; }
			public KeyValueAddress<bool> Input1 { get; set; }

			public KeyValueAddress<bool> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable => Operation.ToString().ToUpper();

			[JsonIgnore]
			public string OperationTooltip
			{
				get
				{
					switch (Operation)
					{
						case Operations.Set:
							return "Sets the value of the boolean.";
						case Operations.And:
						case Operations.Or:
						case Operations.Xor:
							return "Sets the value equal to the results of the boolean \"" + Operation + "\" operation.";
						case Operations.Random:
							return "Sets the value equal to a random boolean.";
						default:
							return "Unknown operation \"" + Operation + "\".";
					}
				}
			}

			[JsonIgnore]
			public GUIContent OperationLabelContent => new GUIContent(OperationReadable, OperationTooltip);
		}

		[Serializable]
		public class IntegerBlock : IBaseBlock<int>
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

				Random = 200
			}

			public static IntegerBlock Default => new IntegerBlock
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
						case Operations.Random: return "TO";
					}
					return Operation.ToString().ToUpper();
				}
			}
			
			[JsonIgnore]
			public string OperationTooltip
			{
				get
				{
					switch (Operation)
					{
						case Operations.Set:
							return "Sets the value of the integer.";
						case Operations.Add:
						case Operations.Subtract:
						case Operations.Multiply:
						case Operations.Divide:
						case Operations.Modulo:
						case Operations.Clamp:
							return "Sets the value equal to the results of the \"" + Operation + "\" operation.";
						case Operations.Random:
							return "Sets the value equal to a random integer between the inclusive minimum and the exclusive maximum.";
						default:
							return "Unknown operation \"" + Operation + "\".";
					}
				}
			}
			
			[JsonIgnore]
			public GUIContent OperationLabelContent => new GUIContent(OperationReadable, OperationTooltip);
		}

		[Serializable]
		public class StringBlock : IBaseBlock<string>
		{
			public enum Operations
			{
				Unknown = 0,
				Set = 10
			}

			public static StringBlock Default => new StringBlock
			{
				Operation = Operations.Set,
				Input0 = KeyValueAddress<string>.Default,
				Output = KeyValueAddress<string>.Default
			};

			public Operations Operation { get; set; }

			public KeyValueAddress<string> Input0 { get; set; }

			public KeyValueAddress<string> Output { get; set; }

			[JsonIgnore]
			public string OperationReadable => Operation.ToString().ToUpper();
		
			[JsonIgnore]
			public string OperationTooltip
			{
				get
				{
					switch (Operation)
					{
						case Operations.Set:
							return "Sets the value of the string.";
						default:
							return "Unknown operation \"" + Operation + "\".";
					}
				}
			}
			
			[JsonIgnore]
			public GUIContent OperationLabelContent => new GUIContent(OperationReadable, OperationTooltip);
		}

		[Serializable]
		public class FloatBlock : IBaseBlock<float>
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
				Ceiling = 130,

				Random = 200,
				RandomNormal = 210
			}

			public static FloatBlock Default => new FloatBlock
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
						case Operations.Random: return "TO";
					}
					return Operation.ToString().ToUpper();
				}
			}
			
			[JsonIgnore]
			public string OperationTooltip
			{
				get
				{
					switch (Operation)
					{
						case Operations.Set:
							return "Sets the value of the float.";
						case Operations.Add:
						case Operations.Subtract:
						case Operations.Multiply:
						case Operations.Divide:
						case Operations.Modulo:
						case Operations.Clamp:
						case Operations.Round:
						case Operations.Floor:
						case Operations.Ceiling:
							return "Sets the value equal to the results of the \"" + Operation + "\" operation.";
						case Operations.Random:
							return "Sets the value equal to a random float between the inclusive minimum and the exclusive maximum.";
						case Operations.RandomNormal:
							return "Sets the value equal to a random float between the inclusive minimum 0.0f and the exclusive maximum 1.0f.";
						default:
							return "Unknown operation \"" + Operation + "\".";
					}
				}
			}
			
			[JsonIgnore]
			public GUIContent OperationLabelContent => new GUIContent(OperationReadable, OperationTooltip);
		}

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering => filtering;

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

		public override string EdgeName => Name;
		
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

		public KeyValueEdgeModel()
		{
			KeyValueType = new ListenerProperty<KeyValueTypes>(value => keyValueType = value, () => keyValueType);
			BooleanValue = new ListenerProperty<BooleanBlock>(value => booleanValue = value, () => booleanValue);
			IntegerValue = new ListenerProperty<IntegerBlock>(value => integerValue = value, () => integerValue);
			StringValue = new ListenerProperty<StringBlock>(value => stringValue = value, () => stringValue);
			FloatValue = new ListenerProperty<FloatBlock>(value => floatValue = value, () => floatValue);
		}
	}
}
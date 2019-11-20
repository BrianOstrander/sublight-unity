using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleTraitEdgeModel : EdgeModel
	{
		public enum Operations
		{
			Unknown = 0,
			AppendTraitByTraitId = 10,
			RemoveTraitByTraitId = 100,
			RemoveTraitByTraitFamilyId = 110
		}
		
		[JsonProperty] Operations operation;
		[JsonIgnore] public readonly ListenerProperty<Operations> Operation;
		
		[JsonProperty] string operationId;
		[JsonIgnore] public readonly ListenerProperty<string> OperationId;

		[JsonProperty] ModuleTypes[] validModuleTypes = new ModuleTypes[0];
		[JsonIgnore] public readonly ListenerProperty<ModuleTypes[]> ValidModuleTypes;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering => filtering;
		
#if UNITY_EDITOR
		public override string EdgeName
		{
			get
			{
				switch (Operation.Value)
				{
					case Operations.AppendTraitByTraitId:
						return "Append Trait By Trait Id";
					case Operations.RemoveTraitByTraitId:
						return "Remove Trait By Trait Id";
					case Operations.RemoveTraitByTraitFamilyId:
						return "Remove Trait By Trait Family Id";
					default:
						return "Unknown Operation";	
				}
			}
		}
		
		[JsonIgnore]
		public string OperationIdName
		{
			get
			{
				switch (operation)
				{
					case Operations.AppendTraitByTraitId:
					case Operations.RemoveTraitByTraitId:
						return "Trait Id";
					case Operations.RemoveTraitByTraitFamilyId:
						return "Trait Family Id";
					default:
						return "Unrecognized Id";
				}
			}
		}
#endif
		
		public ModuleTraitEdgeModel()
		{
			Operation = new ListenerProperty<Operations>(value => operation = value, () => operation);
			OperationId = new ListenerProperty<string>(value => operationId = value, () => operationId);
			ValidModuleTypes = new ListenerProperty<ModuleTypes[]>(value => validModuleTypes = value, () => validModuleTypes);
		}
	}
}
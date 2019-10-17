using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleTraitModel : SaveModel
	{
		#region Serialized
		[JsonProperty] string name;
		/// <summary>
		/// The name of this encounter, will be seen by the player and listed as
		/// the meta data.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] string description;
		/// <summary>
		/// An internal description of this encounter, only for editor use.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Description;

		[JsonProperty] ModuleTraitSeverity severity;
		[JsonIgnore] public readonly ListenerProperty<ModuleTraitSeverity> Severity;

		[JsonProperty] ModuleTypes[] compatibleModuleTypes = new ModuleTypes[0];
		[JsonIgnore] public readonly ListenerProperty<ModuleTypes[]> CompatibleModuleTypes;

		[JsonProperty] string[] familyIds = new string[0];
		[JsonIgnore] public readonly ListenerProperty<string[]> FamilyIds;
		
		[JsonProperty] string[] incompatibleFamilyIds = new string[0];
		[JsonIgnore] public readonly ListenerProperty<string[]> IncompatibleFamilyIds;

		[JsonProperty] string[] incompatibleIds = new string[0];
		[JsonIgnore] public readonly ListenerProperty<string[]> IncompatibleIds;
		#endregion

		public ModuleTraitModel()
		{
			SaveType = SaveTypes.ModuleTrait;
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Severity = new ListenerProperty<ModuleTraitSeverity>(value => severity = value, () => severity);
			CompatibleModuleTypes = new ListenerProperty<ModuleTypes[]>(value => compatibleModuleTypes = value, () => compatibleModuleTypes);
			FamilyIds = new ListenerProperty<string[]>(value => familyIds = value, () => familyIds);
			IncompatibleFamilyIds = new ListenerProperty<string[]>(value => incompatibleFamilyIds = value, () => incompatibleFamilyIds);
			IncompatibleIds = new ListenerProperty<string[]>(value => incompatibleIds = value, () => incompatibleIds);
		}
	}

	public static class ModuleTraitModelLinq
	{
		public static IEnumerable<ModuleTraitModel> HighestSeverity(this IEnumerable<ModuleTraitModel> entries)
		{;
			var ordered = entries.OrderByDescending(e => e.Severity.Value);
			if (ordered.None()) return ordered;

			return entries.Where(e => e.Severity.Value == ordered.First().Severity.Value);
		}
	}
}
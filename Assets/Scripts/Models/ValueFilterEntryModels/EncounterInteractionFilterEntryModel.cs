using Newtonsoft.Json;
using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class EncounterInteractionFilterEntryModel : ValueFilterEntryModel<string>
	{
		[JsonProperty] EncounterInteractionFilterOperations operation;
		[JsonIgnore] public ListenerProperty<EncounterInteractionFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.EncounterInteraction; } }
		public override KeyValueTypes FilterValueType { get { return KeyValueTypes.String; } }

#if UNITY_EDITOR
		[JsonIgnore] public RequestStatus EncounterIdIsValid { get; set; }
#endif
		
		public EncounterInteractionFilterEntryModel()
		{
			Operation = new ListenerProperty<EncounterInteractionFilterOperations>(value => operation = value, () => operation);
			
#if UNITY_EDITOR
			FilterValue.Changed += OnFilterValue;
#endif
		}
		
#if UNITY_EDITOR
		void OnFilterValue(string value)
		{
			EncounterIdIsValid = RequestStatus.Unknown;
		}
#endif
	}
}
using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEntryModel : EdgeModel
	{
		[JsonProperty] string message;
		[JsonProperty] string nextLogId;
		[JsonProperty] bool notAutoUsed;
		[JsonProperty] bool autoDisableInteractions;
		[JsonProperty] bool autoDisableEnabled;
		[JsonProperty] ValueFilterModel usedFiltering = ValueFilterModel.Default(false);
		[JsonProperty] ValueFilterModel interactableFiltering = ValueFilterModel.Default();
		[JsonProperty] ValueFilterModel enabledFiltering = ValueFilterModel.Default();

		[JsonIgnore]
		public readonly ListenerProperty<string> Message;
		[JsonIgnore]
		public readonly ListenerProperty<string> NextLogId;
		/// <summary>
		/// When clicked, buttons normally set themselves to be used. If this
		/// value is true, that is not the case. The auto used button value is
		/// combined with UsedFiltering by an OR operation.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> NotAutoUsed;
		/// <summary>
		/// If true, clicking the button disables future interactions with it.
		/// This is combined with the InteractableFiltering with an AND
		/// operation.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AutoDisableInteractions;
		/// <summary>
		/// If true, clicking the button disables future showings of it. This is
		/// combined with the EnabledFiltering in an AND operation.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AutoDisableEnabled;

		[JsonProperty] bool showFiltering;
		[JsonIgnore] public readonly ListenerProperty<bool> ShowFiltering;
		
		/// <summary>
		/// Does the button look like its already been clicked? This is combined
		/// with the auto used logic in a OR operation. So the button will look
		/// clicked if either used filtering returns true, or auto using sets it
		/// to true.
		/// </summary>
		/// <value>The used filtering.</value>
		[JsonIgnore]
		public ValueFilterModel UsedFiltering { get { return usedFiltering; } }
		/// <summary>
		/// Is the button interactable, can it be clicked? Combined with auto
		/// interaction disabling using an AND operation.
		/// </summary>
		/// <value>The interactable filtering.</value>
		[JsonIgnore]
		public ValueFilterModel InteractableFiltering { get { return interactableFiltering; } }
		/// <summary>
		/// Is the button enabled, visible? Combined with auto disable enabled
		/// using an AND operation.
		/// </summary>
		/// <value>The enabled filtering.</value>
		[JsonIgnore]
		public ValueFilterModel EnabledFiltering { get { return enabledFiltering; } }

		[JsonIgnore]
		public string AutoDisabledKey { get { return "AutoDisabled_" + EdgeId; } }
		[JsonIgnore]
		public string AutoDisabledInteractionsKey { get { return "AutoDisabledInteractions_" + EdgeId; } }
		[JsonIgnore]
		public string AutoUsedKey { get { return "AutoUsed_" + EdgeId; } }

		public override string EdgeName => "Button";
		
		public ButtonEntryModel()
		{
			Message = new ListenerProperty<string>(value => message = value, () => message);
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
			NotAutoUsed = new ListenerProperty<bool>(value => notAutoUsed = value, () => notAutoUsed);
			AutoDisableInteractions = new ListenerProperty<bool>(value => autoDisableInteractions = value, () => autoDisableInteractions);
			AutoDisableEnabled = new ListenerProperty<bool>(value => autoDisableEnabled = value, () => autoDisableEnabled);
			
			ShowFiltering = new ListenerProperty<bool>(value => showFiltering = value, () => showFiltering);
		}
	}
}
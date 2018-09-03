using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEdgeModel : Model, IEdgeModel
	{
		[JsonProperty] LanguageStringModel _message = new LanguageStringModel();

		[JsonProperty] string buttonId;
		[JsonProperty] int index;
		[JsonProperty] bool ignore;
		[JsonProperty] string message;
		[JsonProperty] string nextLogId;
		[JsonProperty] bool notAutoUsed;
		[JsonProperty] bool autoDisableInteractions;
		[JsonProperty] bool autoDisableEnabled;
		[JsonProperty] ValueFilterModel usedFiltering = ValueFilterModel.Default(false);
		[JsonProperty] ValueFilterModel interactableFiltering = ValueFilterModel.Default();
		[JsonProperty] ValueFilterModel enabledFiltering = ValueFilterModel.Default();

		[JsonIgnore]
		public LanguageStringModel _Message { get { return _message; } }

		[JsonIgnore]
		public readonly ListenerProperty<string> ButtonId;
		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;
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
		public string AutoDisabledKey { get { return "AutoDisabled_" + ButtonId.Value; } }
		[JsonIgnore]
		public string AutoDisabledInteractionsKey { get { return "AutoDisabledInteractions_" + ButtonId.Value; } }
		[JsonIgnore]
		public string AutoUsedKey { get { return "AutoUsed_" + ButtonId.Value; } }

		public ButtonEdgeModel()
		{
			ButtonId = new ListenerProperty<string>(value => buttonId = value, () => buttonId);
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			Message = new ListenerProperty<string>(value => message = value, () => message);
			NextLogId = new ListenerProperty<string>(value => nextLogId = value, () => nextLogId);
			NotAutoUsed = new ListenerProperty<bool>(value => notAutoUsed = value, () => notAutoUsed);
			AutoDisableInteractions = new ListenerProperty<bool>(value => autoDisableInteractions = value, () => autoDisableInteractions);
			AutoDisableEnabled = new ListenerProperty<bool>(value => autoDisableEnabled = value, () => autoDisableEnabled);
		}

		protected override void OnRegisterLanguageStrings()
		{
			AddLanguageStrings(_Message);
		}

		[JsonIgnore]
		public string EdgeName { get { return "Button"; } }
		[JsonIgnore]
		public int EdgeIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}
		[JsonIgnore]
		public string EdgeId
		{
			get { return ButtonId.Value; }
			set { ButtonId.Value = value; }
		}
	}
}
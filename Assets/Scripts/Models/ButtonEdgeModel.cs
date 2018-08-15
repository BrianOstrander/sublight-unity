﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEdgeModel : Model
	{
		[JsonProperty] string buttonId;
		[JsonProperty] int index;
		[JsonProperty] bool ignore;
		[JsonProperty] string message;
		[JsonProperty] string nextLogId;
		[JsonProperty] bool notAutoUsed;
		[JsonProperty] bool autoDisableInteractions;
		[JsonProperty] bool autoDisableEnabled;
		[JsonProperty] ValueFilterModel usedFiltering = new ValueFilterModel();
		[JsonProperty] ValueFilterModel interactableFiltering = new ValueFilterModel();
		[JsonProperty] ValueFilterModel enabledFiltering = new ValueFilterModel();

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
		/// value is true, that is not the case.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> NotAutoUsed;
		/// <summary>
		/// If true, clicking the button disables future interactions with it.
		/// This is combined with the InteractableFiltering in an OR operation.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AutoDisableInteractions;
		/// <summary>
		/// If true, clicking the button disables future showings of it. This is
		/// combined with the EnabledFiltering in an OR operation.
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
		/// Is the button interactable, can it be clicked?
		/// </summary>
		/// <value>The interactable filtering.</value>
		[JsonIgnore]
		public ValueFilterModel InteractableFiltering { get { return interactableFiltering; } }
		/// <summary>
		/// Is the button enabled, visible?
		/// </summary>
		/// <value>The enabled filtering.</value>
		[JsonIgnore]
		public ValueFilterModel EnabledFiltering { get { return enabledFiltering; } }

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
	}
}
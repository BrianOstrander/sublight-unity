using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogEntryModel : EdgeEntryModel
	{
		[JsonProperty] string title;
		[JsonProperty] string message;
		[JsonProperty] DialogTypes dialogType;
		[JsonProperty] DialogStyles dialogStyle;

		[JsonProperty] string successLogId;
		[JsonProperty] string failureLogId;
		[JsonProperty] string cancelLogId;

		[JsonProperty] string successText;
		[JsonProperty] string failureText;
		[JsonProperty] string cancelText;

		[JsonIgnore]
		public readonly ListenerProperty<string> Title;
		[JsonIgnore]
		public readonly ListenerProperty<string> Message;
		[JsonIgnore]
		public readonly ListenerProperty<DialogTypes> DialogType;
		[JsonIgnore]
		public readonly ListenerProperty<DialogStyles> DialogStyle;

		[JsonIgnore]
		public readonly ListenerProperty<string> SuccessLogId;
		[JsonIgnore]
		public readonly ListenerProperty<string> FailureLogId;
		[JsonIgnore]
		public readonly ListenerProperty<string> CancelLogId;

		[JsonIgnore]
		public readonly ListenerProperty<string> SuccessText;
		[JsonIgnore]
		public readonly ListenerProperty<string> FailureText;
		[JsonIgnore]
		public readonly ListenerProperty<string> CancelText;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default(true);
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public DialogEntryModel()
		{
			Title = new ListenerProperty<string>(value => title = value, () => title);
			Message = new ListenerProperty<string>(value => message = value, () => message);
			DialogType = new ListenerProperty<DialogTypes>(value => dialogType = value, () => dialogType);
			DialogStyle = new ListenerProperty<DialogStyles>(value => dialogStyle = value, () => dialogStyle);

			SuccessLogId = new ListenerProperty<string>(value => successLogId = value, () => successLogId);
			FailureLogId = new ListenerProperty<string>(value => failureLogId = value, () => failureLogId);
			CancelLogId = new ListenerProperty<string>(value => cancelLogId = value, () => cancelLogId);

			SuccessText = new ListenerProperty<string>(value => successText = value, () => successText);
			FailureText = new ListenerProperty<string>(value => failureText = value, () => failureText);
			CancelText = new ListenerProperty<string>(value => cancelText = value, () => cancelText);
		}
	}
}
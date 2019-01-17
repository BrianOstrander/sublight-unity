//using Newtonsoft.Json;

//namespace LunraGames.SubLight.Models
//{
//	public class DialogEdgeModel : Model, IEdgeModel
//	{
//		[JsonProperty] DialogEntryModel entry = new DialogEntryModel();

//		[JsonProperty] int index;
//		[JsonProperty] bool ignore;

//		[JsonIgnore]
//		public DialogEntryModel Entry { get { return entry; } }
//		[JsonIgnore]
//		public ListenerProperty<string> DialogId { get { return Entry.DialogId; } }

//		[JsonIgnore]
//		public readonly ListenerProperty<int> Index;
//		[JsonIgnore]
//		public readonly ListenerProperty<bool> Ignore;

//		public DialogEdgeModel()
//		{
//			Index = new ListenerProperty<int>(value => index = value, () => index);
//			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
//		}

//		[JsonIgnore]
//		public string EdgeName { get { return "TODO: NAME THIS"; } }
//		[JsonIgnore]
//		public int EdgeIndex
//		{
//			get { return Index.Value; }
//			set { Index.Value = value; }
//		}
//		[JsonIgnore]
//		public string EdgeId
//		{
//			get { return DialogId.Value; }
//			set { DialogId.Value = value; }
//		}
//	}
//}
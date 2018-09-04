namespace LunraGames.SubLight
{
	public abstract class SetFocusDetailsBase {}

	public abstract class SetFocusDetails<T> : SetFocusDetailsBase
		where T : SetFocusDetails<T>, new()
	{
		public static T Default
		{
			get
			{
				var result = new T();
				result.OnDefault();
				return result;
			}
		}

		public bool Interactable;

		public virtual void OnDefault() {}
	}
}
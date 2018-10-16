namespace LunraGames.SubLight
{
	public class StateFocuses
	{
		static SetFocusLayers[] allLayers;
		protected static SetFocusLayers[] AllLayers { get { return allLayers ?? (allLayers = EnumExtensions.GetValues(SetFocusLayers.Unknown)); } }

		#region Focus Building
		protected static SetFocusBlock GetFocus<D>(
			int order = 0,
			bool enabled = false,
			float weight = 0f,
			bool? interactable = null,
			D details = null
		)
			where D : SetFocusDetails<D>, new()
		{
			var baseDetails = details ?? new D().SetDefault();
			if (interactable.HasValue) baseDetails.Interactable = interactable.Value;
			return new SetFocusBlock(
				baseDetails,
				enabled,
				order,
				weight
			);
		}
		#endregion
	}
}
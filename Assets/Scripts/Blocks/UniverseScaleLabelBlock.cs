using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct UniverseScaleLabelBlock
	{
		public static UniverseScaleLabelBlock Default
		{
			get
			{
				return new UniverseScaleLabelBlock
				{
					Name = LanguageStringModel.Empty
				};
			}
		}

		public static UniverseScaleLabelBlock Create(LanguageStringModel name)
		{
			return new UniverseScaleLabelBlock
			{
				Name = name
			};
		}

		public LanguageStringModel Name;
	}
}
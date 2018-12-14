using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GridTimeLanguageBlock
	{
		public static GridTimeLanguageBlock Default
		{
			get
			{
				var referenceFrameNames = new Dictionary<ReferenceFrames, LanguageStringModel>();
				foreach (var referenceFrame in EnumExtensions.GetValues<ReferenceFrames>()) referenceFrameNames[referenceFrame] = LanguageStringModel.Empty;
				return new GridTimeLanguageBlock
				{
					Title = LanguageStringModel.Empty,
					SubTitle = LanguageStringModel.Empty,
					Tooltip = LanguageStringModel.Empty,
					ReferenceFrameNames = referenceFrameNames
				};
			}
		}

		public LanguageStringModel Title;
		public LanguageStringModel SubTitle;
		public LanguageStringModel Tooltip;
		public Dictionary<ReferenceFrames, LanguageStringModel> ReferenceFrameNames;

		public GridTimeLanguageBlock Duplicate
		{
			get
			{
				return new GridTimeLanguageBlock
				{
					Title = Title,
					SubTitle = SubTitle,
					Tooltip = Tooltip,
					ReferenceFrameNames = new Dictionary<ReferenceFrames, LanguageStringModel>(ReferenceFrameNames)
				};
			}
		}
	}
}
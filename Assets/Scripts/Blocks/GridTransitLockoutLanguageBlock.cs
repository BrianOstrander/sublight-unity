﻿using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GridTransitLockoutLanguageBlock
	{
		public LanguageStringModel TransitTitle;
		public LanguageStringModel TransitDescription;

		public LanguageStringModel DescriptionPrefix;

		public LanguageStringModel UnlockLeftTitle;
		public LanguageStringModel UnlockRightTitle;

		public LanguageStringModel[] UnlockLeftStatuses;
		public LanguageStringModel[] UnlockRightStatuses;

		public DialogLanguageBlock SaveDisabledDuringTransit;

	}
}
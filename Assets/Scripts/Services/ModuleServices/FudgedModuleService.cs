using LunraGames.NumberDemon;

namespace LunraGames.SubLight
{
	public class FudgedModuleService : ModuleService
	{
		static class Seeds
		{
			public static Demon Get(int seed, int offset) => new Demon(seed + offset);

			public const int Name = 1;
			public const int YearManufactured = 2;
			public const int Description = 3;
		}
		
		public FudgedModuleService(IModelMediator modelMediator) : base(modelMediator) {}

		protected override string GetName(GenerationInfo info)
		{
			var random = Seeds.Get(info.Seed, Seeds.Name);
			return info.Type + " " + random.GetNextInteger(0, 999);
		}

		protected override string GetYearManufactured(GenerationInfo info)
		{
			var random = Seeds.Get(info.Seed, Seeds.YearManufactured);
			return random.GetNextInteger(2100, 2674).ToString();
		}

		protected override string GetDescription(GenerationInfo info)
		{
			// var random = Seeds.Get(info.Seed, Seeds.Description);
			return "A standard " + info.Type + " unit.";
		}
	}
}
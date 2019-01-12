using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct TransitStateRequest
	{
		public static TransitStateRequest Create(
			SystemModel beginSystem,
			SystemModel endSystem
		)
		{
			return new TransitStateRequest(
				false,
				beginSystem,
				endSystem
			);
		}

		public static TransitStateRequest CreateInstant(
			SystemModel beginSystem,
			SystemModel endSystem
		)
		{
			return new TransitStateRequest(
				true,
				beginSystem,
				endSystem
			);
		}

		public readonly bool Instant;
		public readonly SystemModel BeginSystem;
		public readonly SystemModel EndSystem;

		TransitStateRequest(
			bool instant,
			SystemModel beginSystem,
			SystemModel endSystem
		)
		{
			Instant = instant;
			BeginSystem = beginSystem;
			EndSystem = endSystem;
		}
	}
}
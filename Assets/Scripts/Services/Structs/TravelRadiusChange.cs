using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct TravelRadiusChange
	{
		public readonly UniversePosition Origin;
		public readonly float Speed;
		public readonly float RationConsumption;
		public readonly float Rations;
		public readonly DayTime RationDuration;
		public readonly TravelRadius TravelRadius;

		public TravelRadiusChange(
			UniversePosition origin, 
			float speed, 
			float rationConsumption, 
			float rations)
		{
			Origin = origin;
			Speed = speed;
			RationConsumption = rationConsumption;
			Rations = rations;
			RationDuration = DayTime.FromDayNormal(rations / rationConsumption);
			var rationDistance = RationDuration.TotalTime * speed;
			// TODO: Find a better place for handling this weird range stuff?
			TravelRadius = new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}
	}
}
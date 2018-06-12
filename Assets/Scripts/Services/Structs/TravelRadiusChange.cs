using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct TravelRadiusChange
	{
		public UniversePosition Origin;
		public float Speed;
		public float RationConsumption;
		public float Rations;
		public DayTime RationDuration;
		public TravelRadius TravelRadius;

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
			RationDuration = new DayTime(rations / rationConsumption);
			var rationDistance = RationDuration.TotalTime * speed;
			// TODO: Find a better place for handling this weird range stuff?
			TravelRadius = new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}
	}
}
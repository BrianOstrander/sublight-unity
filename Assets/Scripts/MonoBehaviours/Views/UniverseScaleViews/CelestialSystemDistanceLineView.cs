using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CelestialSystemDistanceLineView : UniverseScaleView, ICelestialSystemDistanceLineView
	{
		[SerializeField]
		LineRenderer bottomLine;

		public void SetPoints(Vector3 begin, Vector3 end)
		{
			var delta = end - begin;
			bottomLine.SetPositions(new Vector3[] { Vector3.zero, delta });
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		void SetPoints(Vector3 begin, Vector3 end);
	}
}
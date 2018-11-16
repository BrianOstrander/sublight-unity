using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CelestialSystemDistanceLineView : UniverseScaleView, ICelestialSystemDistanceLineView
	{
		[SerializeField]
		LineRenderer bottomLine;

		public void SetPoints(Vector3 begin, Vector3 end)
		{
			bottomLine.SetPositions(new Vector3[] { begin, end });
		}
	}

	public interface ICelestialSystemDistanceLineView : IUniverseScaleView
	{
		void SetPoints(Vector3 begin, Vector3 end);
	}
}
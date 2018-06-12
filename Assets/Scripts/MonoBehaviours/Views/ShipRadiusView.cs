#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class ShipRadiusView : View, IShipRadiusView
	{
		public UniversePosition UniversePosition { set; get; }

		TravelRadius travelRadius;
		public TravelRadius TravelRadius
		{
			set
			{
				travelRadius = value;
				OnTravelRadius();
			}
			private get { return travelRadius; }
		}

		public override void Reset()
		{
			base.Reset();

			UniversePosition = UniversePosition.Zero;
			TravelRadius = TravelRadius.Zero;
		}

		#region Events
		void OnTravelRadius()
		{
			
		}
		#endregion

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
#if UNITY_EDITOR
			Handles.color = Color.green;
			Handles.DrawWireDisc(transform.position, Vector3.up, App.UniverseService.UniverseToUnityDistance(TravelRadius.SafeRadius));
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(transform.position, Vector3.up, App.UniverseService.UniverseToUnityDistance(TravelRadius.DangerRadius));
			Handles.color = Color.red;
			Handles.DrawWireDisc(transform.position, Vector3.up, App.UniverseService.UniverseToUnityDistance(TravelRadius.MaximumRadius));
#endif
		}
	}

	public interface IShipRadiusView : IGridTransform
	{
		TravelRadius TravelRadius { set; }
	}
}
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ShipRadiusView : View, IShipRadiusView
	{
		public Vector3 UnityPosition
		{
			set { Root.position = value; }
			get { return Root.position; }
		}
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

			UnityPosition = Vector3.zero;
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
			Handles.DrawWireDisc(transform.position, Vector3.up, UniversePosition.ToUnityDistance(TravelRadius.SafeRadius));
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(transform.position, Vector3.up, UniversePosition.ToUnityDistance(TravelRadius.DangerRadius));
			Handles.color = Color.red;
			Handles.DrawWireDisc(transform.position, Vector3.up, UniversePosition.ToUnityDistance(TravelRadius.MaximumRadius));
#endif
		}
	}

	public interface IShipRadiusView : IGridTransform
	{
		TravelRadius TravelRadius { set; }
	}
}
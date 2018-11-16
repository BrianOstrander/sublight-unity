using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public abstract class UniverseScaleView : View, IUniverseScaleView
	{
		enum Axises
		{
			Unknown = 0,
			None = 10,
			X = 20,
			Y = 30,
			Z = 40
		}

		[SerializeField]
		Axises scaleIgnores = Axises.Y;
		[SerializeField]
		Axises positionIgnores = Axises.Y;
		[SerializeField]
		Transform scaleArea;
		[SerializeField]
		Transform positionArea;

		public Vector3 WorldOrigin { set; protected get; }
		public float WorldRadius { set; protected get; }

		public Vector3 Scale
		{
			set
			{
				var rawValue = value;
				switch(scaleIgnores)
				{
					case Axises.None: break;
					case Axises.X: value = value.NewX(1f); break;
					case Axises.Y: value = value.NewY(1f); break;
					case Axises.Z: value = value.NewZ(1f); break;
					default:
						Debug.LogError("Unrecognized axis: " + scaleIgnores);
						break;
				}

				if (scaleArea != null) scaleArea.localScale = value;
				OnScale(value, rawValue);
			}
		}

		public Vector3 Position
		{
			set
			{
				var rawValue = value;
				switch (positionIgnores)
				{
					case Axises.None: break;
					case Axises.X: value = value.NewX(transform.position.x); break;
					case Axises.Y: value = value.NewY(transform.position.y); break;
					case Axises.Z: value = value.NewZ(transform.position.z); break;
					default:
						Debug.LogError("Unrecognized axis: " + positionIgnores);
						break;
				}

				if (positionArea != null) positionArea.position = value;
				OnPosition(value, rawValue);
			}
		}

		protected float RadiusNormal(Vector3 worldPosition)
		{
			if (Mathf.Approximately(0f, WorldRadius)) return 1f;
			worldPosition = worldPosition.NewY(WorldOrigin.y);
			return Vector3.Distance(WorldOrigin, worldPosition) / WorldRadius;
		}

		protected virtual void OnScale(Vector3 scale, Vector3 rawScale) {}
		protected virtual void OnPosition(Vector3 position, Vector3 rawPosition) {}
	}

	public interface IUniverseScaleView : IView
	{
		Vector3 WorldOrigin { set; }
		float WorldRadius { set; }

		Vector3 Scale { set; }
		Vector3 Position { set; }
	}
}
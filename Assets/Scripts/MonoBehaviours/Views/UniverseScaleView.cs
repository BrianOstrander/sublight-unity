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

		protected Transform ScaleArea { get { return scaleArea; } }
		protected Transform PositionArea { get { return positionArea; } }

		protected Vector3 GridOrigin { private set; get; }
		protected float GridRadius { private set; get; }

		public void SetGrid(Vector3 gridOrigin, float gridRadius)
		{
			GridOrigin = gridOrigin;
			GridRadius = gridRadius;
		}

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
			if (Mathf.Approximately(0f, GridRadius)) return 1f;
			worldPosition = worldPosition.NewY(GridOrigin.y);
			return Vector3.Distance(GridOrigin, worldPosition) / GridRadius;
		}

		protected virtual void OnScale(Vector3 scale, Vector3 rawScale) {}
		protected virtual void OnPosition(Vector3 position, Vector3 rawPosition) {}

		protected bool PositionIsInRadius(Vector3 worldPosition)
		{
			return Vector3.Distance(GridOrigin, worldPosition.NewY(GridOrigin.y)) <= GridRadius;
		}
	}

	public interface IUniverseScaleView : IView
	{
		void SetGrid(Vector3 gridOrigin, float gridRadius);

		Vector3 Scale { set; }
		Vector3 Position { set; }
	}
}
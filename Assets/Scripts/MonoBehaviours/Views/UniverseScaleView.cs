using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public enum UniverseScaleAxises
	{
		Unknown = 0,
		None = 10,
		X = 20,
		Y = 30,
		Z = 40
	}

	public abstract class UniverseScaleView : View, IUniverseScaleView
	{

		[SerializeField]
		UniverseScaleAxises scaleIgnores = UniverseScaleAxises.Y;
		[SerializeField]
		UniverseScaleAxises positionIgnores = UniverseScaleAxises.Y;
		[SerializeField]
		Transform scaleArea;
		[SerializeField]
		Transform positionArea;

		bool? wasInBounds;

		public UniverseScaleAxises ScaleIgnores { get { return scaleIgnores; } }
		public UniverseScaleAxises PositionIgnores { get { return positionIgnores; } }
		protected Transform ScaleArea { get { return scaleArea; } }
		protected Transform PositionArea { get { return positionArea; } }

		protected Vector3 GridOrigin { private set; get; }
		protected float GridRadius { private set; get; }

		public virtual bool RestrictVisibiltyInBounds { get { return false; } }

		public bool IsInBounds { get; private set; }
		public float RadiusNormal { get; private set; }

		public Func<Vector3, float, float> GetRadiusNormalCallback { get; set; }
		public Func<Vector3, float, bool> GetPositionIsInRadiusCallback { get; set; }

		public void SetGrid(Vector3 gridOrigin, float gridRadius)
		{
			GridOrigin = gridOrigin;
			GridRadius = gridRadius;
		}

		public void SetScale(Vector3 scale, Vector3 rawScale)
		{
			if (scaleArea != null) scaleArea.localScale = scale;
			OnScale(scale, rawScale);
		}

		public void SetPosition(Vector3 position, Vector3 rawPosition, bool isInBounds, float radiusNormal)
		{
			if (positionArea != null) positionArea.position = position;
			IsInBounds = isInBounds;
			RadiusNormal = radiusNormal;

			if (!wasInBounds.HasValue || wasInBounds != isInBounds)
			{
				wasInBounds = isInBounds;
				OnInBoundsChanged(isInBounds);
			}

			OnPosition(position, rawPosition);
		}

		public override void Reset()
		{
			base.Reset();

			wasInBounds = null;

			// These are set to null to make sure they're properly set...
			GetRadiusNormalCallback = null;
			GetPositionIsInRadiusCallback = null;
		}

		protected virtual void OnScale(Vector3 scale, Vector3 rawScale) {}
		protected virtual void OnPosition(Vector3 position, Vector3 rawPosition) {}
		protected virtual void OnInBoundsChanged(bool isInBounds) {}
		protected virtual void OnInBoundsUnscaledChanged(bool isInBoundsUnscaled) {}

		protected float GetRadiusNormal(Vector3 worldPosition, float margin = 0f)
		{
			if (GetRadiusNormalCallback == null) throw new NullReferenceException("GetRadiusNormalCallback");
			return GetRadiusNormalCallback(worldPosition, margin);
		}

		protected bool GetPositionIsInRadius(Vector3 worldPosition, float margin = 0f)
		{
			if (GetPositionIsInRadiusCallback == null) throw new NullReferenceException("GetPositionIsInRadiusCallback");
			return GetPositionIsInRadiusCallback(worldPosition, margin);
		}
	}

	public interface IUniverseScaleView : IView
	{
		bool RestrictVisibiltyInBounds { get; }
		bool IsInBounds { get; }
		float RadiusNormal { get; }
		UniverseScaleAxises ScaleIgnores { get; }
		UniverseScaleAxises PositionIgnores { get; }

		Func<Vector3, float, float> GetRadiusNormalCallback { get; set; }
		Func<Vector3, float, bool> GetPositionIsInRadiusCallback { get; set; }

		void SetScale(Vector3 scale, Vector3 rawScale);
		void SetPosition(Vector3 position, Vector3 rawPosition, bool isInBounds, float radiusNormal);

		void SetGrid(Vector3 gridOrigin, float gridRadius);
	}
}
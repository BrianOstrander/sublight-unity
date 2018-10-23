using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		[Serializable]
		struct GridZoomCountEntry
		{
			public float Begin;
			public float End;
			public float ZoomCount;
			public float Landings;
			public bool CallClustering;

			public float Delta { get { return End - Begin; } }

			public bool Invalid { get { return Mathf.Approximately(0f, Delta); } }

			public float GetScalar(float value) { return (value - Begin) / Delta; }
			public float GetLanding(int index) { return Begin + ((Delta / Landings) * index); }

			public float GetZoom(float value) { return GetScalar(value) * ZoomCount; }

			public bool IsOnLanding(float value)
			{
				if (Mathf.Approximately(0f, Landings)) return true;
				for (var i = 0; i < Landings; i++)
				{
					if (Mathf.Approximately(GetLanding(i), value)) return true;
				}
				return false;
			}

			public void NextLanding(float value, bool up, out float lesser, out float higher)
			{
				lesser = Begin;
				higher = End;

				if (up)
				{
					for (var i = 0; i < Landings; i++)
					{
						higher = GetLanding(i);

						if (value < higher) return;

						lesser = higher;
					}
					higher = End;
					return;
				}

				for (var i = (int)(Landings - 1); 0 <= i; i--)
				{
					lesser = GetLanding(i);
					if (lesser < value) return;
					higher = lesser;
				}
			}
		}

		[SerializeField]
		float zoomMaximum;
		[SerializeField]
		float zoomSensitivity;
		[SerializeField]
		AnimationCurve revealCurve;
		[SerializeField]
		AnimationCurve zoomRadiusProgress;
		[SerializeField]
		AnimationCurve zoomAlpha;

		[SerializeField]
		float zoomSettlingCooldown;
		[SerializeField]
		float zoomSettlingScalar;
		[SerializeField]
		float zoomSettlingMinimum;

		[SerializeField]
		AnimationCurve zoomSettlingUpCurve;
		[SerializeField]
		AnimationCurve zoomSettlingDownCurve;

		[SerializeField]
		GridZoomCountEntry[] GridZoomCounts;

		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		MeshRenderer gridMesh;

		public Action<float> Zoom { set; private get; }
		public Action<float> Clustering { set; private get; }
		public bool Highlighted { get; private set; }
		public Color HoloColor { set { gridMesh.material.SetColor(ShaderConstants.HoloGrid.GridColor, value); } }

		float lastZoom;
		float lastZoomDelta;
		float lastZoomDeltaSign;

		float nextZoomTarget;

		float cooldownRemaining;
		bool isSettlingOrHasZoomed;

		public override void Reset()
		{
			base.Reset();

			gridMesh.material = new Material(gridMaterial);

			Zoom = ActionExtensions.GetEmpty<float>();
			Clustering = ActionExtensions.GetEmpty<float>();

			Highlighted = false;
			SetRadius(0f, true);
			UpdateZoom(1.5f);
			HoloColor = Color.white;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			SetRadius(scalar, true);
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			SetRadius(scalar, false);
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			if (!isSettlingOrHasZoomed) return;

			if (Mathf.Approximately(lastZoom, 0f) || Mathf.Approximately(lastZoom, 5f))
			{
				isSettlingOrHasZoomed = false;
				return;
			}

			if (0f < cooldownRemaining)
			{
				cooldownRemaining = Mathf.Max(0f, cooldownRemaining - delta);
				return;
			}

			var landing = GridZoomCounts.FirstOrDefault(c => c.Begin <= lastZoom && lastZoom < c.End);

			if (landing.IsOnLanding(lastZoom))
			{
				isSettlingOrHasZoomed = false;
				SetZoom(lastZoom, 0f, false);
				return;
			}

			var goingUp = 0f < lastZoomDeltaSign;

			var curve = goingUp ? zoomSettlingUpCurve : zoomSettlingDownCurve;

			float lesserLanding;
			float higherLanding;

			landing.NextLanding(lastZoom, goingUp, out lesserLanding, out higherLanding);

			var landingScalar = (lastZoom - lesserLanding) / (higherLanding - lesserLanding);

			var curveDelta = curve.Evaluate(landingScalar) * zoomSettlingScalar * delta;
			curveDelta = Mathf.Max(Mathf.Abs(curveDelta), (zoomSettlingMinimum * delta)) * Mathf.Sign(curveDelta);
			var currDistance = goingUp ? (higherLanding - lastZoom) : (lastZoom - lesserLanding);

			if (currDistance < Mathf.Abs(curveDelta)) curveDelta = currDistance * Mathf.Sign(curveDelta);

			SetZoom(lastZoom + curveDelta, curveDelta, false);
		}

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, revealCurve.Evaluate(scalar));
		}

		public void UpdateZoom(float current, float delta = 0f)
		{
			delta *= zoomSensitivity;

			cooldownRemaining = zoomSettlingCooldown;
			isSettlingOrHasZoomed = true;

			var zoom = Mathf.Clamp(current + delta, 0f, zoomMaximum);

			SetZoom(zoom, delta);
		}

		void SetZoom(float zoom, float delta, bool isUserInput = true)
		{
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.Alpha, zoomAlpha.Evaluate(zoom));
			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, zoomRadiusProgress.Evaluate(zoom));

			lastZoom = zoom;
			lastZoomDelta = delta;
			if (isUserInput) lastZoomDeltaSign = Mathf.Approximately(0f, delta) ? lastZoomDeltaSign : Mathf.Sign(delta);
			else lastZoomDeltaSign = Mathf.Sign(delta);

			Zoom(zoom);
			float clustering;
			if (UpdateClustering(zoom, out clustering)) Clustering(clustering);

			gridMesh.material.SetFloat(ShaderConstants.HoloGrid.Zoom, clustering - (int)clustering);
		}

		bool UpdateClustering(float zoom, out float result)
		{
			result = 0f;

			var entry = GridZoomCounts.FirstOrDefault(c => c.Begin <= zoom && zoom < c.End);
			if (!entry.Invalid) result = entry.GetZoom(zoom);

			return entry.CallClustering;
		}

		#region Events
		public void OnEnter()
		{
			Highlighted = true;
		}

		public void OnExit()
		{
			Highlighted = false;
		}
		#endregion
	}

	public interface IGridView : IView, IHoloColorView
	{
		Action<float> Zoom { set; }
		Action<float> Clustering { set; }
		void SetRadius(float scalar, bool showing);
		void UpdateZoom(float current, float delta = 0f);
		bool Highlighted { get; }
	}
}
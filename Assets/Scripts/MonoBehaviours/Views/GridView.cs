using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class GridView : View, IGridView
	{
		public struct Grid
		{
			public bool IsTarget;
			public bool IsActive;
			public bool ZoomingUp;
			public float Progress;
			public float Tiling;
			public Vector2 Offset;
		}

		[SerializeField]
		AnimationCurve revealCurve;

		[SerializeField]
		float gridUnityWidth;

		[SerializeField]
		Material gridMaterial;
		[SerializeField]
		MeshRenderer gridMesh;
		[SerializeField]
		GameObject gridArea;

		Material[] grids;

		public float GridUnityWidth { get { return gridUnityWidth; } }
		public Action<bool> Dragging { set; private get; }
		public bool Highlighted { get; private set; }
		//public Color HoloColor { set { gridMesh.material.SetColor(ShaderConstants.HoloGrid.GridColor, value); } }
		public Color HoloColor { set { Debug.LogWarning("todo"); } }

		public Grid[] Grids
		{
			set
			{
				if (value == null || value.Length == 0)
				{
					gridMesh.materials = new Material[0];
					grids = null;
					return;
				}
				if (grids == null || grids.Length == 0)
				{
					grids = new Material[value.Length];
					for (var i = 0; i < value.Length; i++) grids[i] = new Material(gridMaterial);
				}

				var activeMaterials = new List<Material>();

				for (var i = 0; i < value.Length; i++)
				{
					if (!value[i].IsActive) continue;

					var block = value[i];
					var grid = grids[i];

					var tiling = 1f;

					if (block.IsTarget)
					{
						if (block.ZoomingUp) tiling = block.Tiling - (block.Tiling * 0.5f * (1f - block.Progress));
						else tiling = block.Tiling + (block.Tiling * (1f - block.Progress));
					}
					else
					{
						if (block.ZoomingUp) tiling = block.Tiling + (block.Tiling * block.Progress);
						else tiling = block.Tiling - (block.Tiling * 0.5f * block.Progress);
					}

					var alpha = block.IsTarget ? block.Progress : 1f - block.Progress;

					//grid.SetColor(ShaderConstants.HoloGridBasic.MainColor, block.IsTarget ? Color.green : Color.red);
					grid.SetFloat(ShaderConstants.HoloGridBasic.Tiling, tiling);
					grid.SetVector(ShaderConstants.HoloGridBasic.Offset, block.Offset);
					grid.SetFloat(ShaderConstants.HoloGridBasic.Alpha, alpha);

					activeMaterials.Add(grid);
				}

				gridMesh.materials = activeMaterials.ToArray();
			}
		}

		public override void Reset()
		{
			base.Reset();

			Grids = null;
			Dragging = ActionExtensions.GetEmpty<bool>();
			Highlighted = false;
			SetRadius(0f, true);
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

		public void SetRadius(float scalar, bool showing)
		{
			if (!showing) scalar += 1f;
			//gridMesh.material.SetFloat(ShaderConstants.HoloGrid.RadiusProgress, revealCurve.Evaluate(scalar));
		}

		public void ProcessDrag(Vector2 viewport, out Vector3 unityPosition)
		{
			//Gesture.GetViewport(gestureNormal) don't forget to do this in the presenter!
			
			unityPosition = Vector3.zero;

			var plane = new Plane(Vector3.up, transform.position);
			var ray = Camera.main.ViewportPointToRay(viewport);

			float distance;
			if (!plane.Raycast(ray, out distance)) return; // Not sure this is possible... maybe?

			unityPosition = ray.origin + (ray.direction * distance);
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

		public void OnDragBegin()
		{
			Dragging(true);
		}

		public void OnDragEnd()
		{
			Dragging(false);
		}
		#endregion

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			/*
			Handles.color = Color.green;
			Handles.DrawWireCube(transform.position, new Vector3(gridRadius * 2f, 0f, gridRadius * 2f));

			if (!Application.isPlaying || !lastZoomInfo.HasValue) return;

			var info = lastZoomInfo.Value;

			var minUnitRadius = info.UnitAmountMinimum * TilingRadius;
			var maxUnitRadius = info.UnitAmountMaximum * TilingRadius;
			var deltaUnitRadius = maxUnitRadius - minUnitRadius;

			var unitsInGridRadius = minUnitRadius + (deltaUnitRadius * info.UnitProgress);
			//var unitsInGridRadius = (minUnitRadius * (1f - info.UnitProgress)) + (deltaUnitRadius * info.UnitProgress);

			var unitInUnity = gridRadius / unitsInGridRadius;
			var unitInUnityVector = new Vector3(unitInUnity, 0f, unitInUnity);
			Handles.DrawWireCube(transform.position + (unitInUnityVector * -0.5f), unitInUnityVector);

			Handles.Label(transform.position + (new Vector3(1f, 0f, -1.55f) * gridRadius), "Zoom: " + info.Zoom.ToString("N2"));
			Handles.Label(transform.position + (new Vector3(1f, 0f, -1.3f) * gridRadius), "Units in Radius: " + unitsInGridRadius);

			if (!dragBegin.HasValue) return;

			var dragDistance = Vector3.Distance(dragBegin.Value, dragCurrent) * unitInUnity;
			Handles.Label(transform.position + (new Vector3(1f, 0f, -1.7f) * gridRadius), "Drag Distance: " + dragDistance);

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(dragBegin.Value, 0.1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(dragCurrent, 0.1f);
*/
#endif
		}
	}

	public interface IGridView : IView, IHoloColorView
	{
		float GridUnityWidth { get; }
		bool Highlighted { get; }
		Action<bool> Dragging { set; }
		GridView.Grid[] Grids { set; }
		void ProcessDrag(Vector2 viewport, out Vector3 unityPosition);

		void SetRadius(float scalar, bool showing);
	}
}
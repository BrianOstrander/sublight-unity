using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ClusterView : GalaxyView, IClusterView
	{
		[SerializeField]
		Transform galaxyNameLabelPositionScaleArea;
		[SerializeField]
		TextCurve galaxyNameLabel;
		[SerializeField]
		Transform galaxyRotationArea;
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		LineRenderer line;
		[SerializeField]
		MeshRenderer[] meshes;
		[SerializeField]
		Transform canvasArea;

		public string GalaxyName { set { galaxyNameLabel.Text = value ?? string.Empty; } }
		public Vector3 GalaxyNormal { set { galaxyRotationArea.LookAt(galaxyRotationArea.position + value.normalized); } }

		public override void SetGalaxy(Texture2D texture, Vector3 worldOrigin, float worldRadius)
		{
			base.SetGalaxy(texture, worldOrigin, worldRadius);
			line.material.SetVector(ShaderConstants.HoloTextureColorAlpha.WorldOrigin, worldOrigin);
			line.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.WorldRadius, worldRadius);
			foreach (var mesh in meshes)
			{
				mesh.material.SetVector(ShaderConstants.HoloTextureColorAlpha.WorldOrigin, worldOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.WorldRadius, worldRadius);
			}
		}

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				group.alpha = value;
				line.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, value);
				foreach (var mesh in meshes)
				{
					mesh.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, value);
				}
			}
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			canvasArea.LookAt(canvasArea.position + App.V.CameraForward);
		}

		public override void Reset()
		{
			base.Reset();

			line.useWorldSpace = true;
			GalaxyName = string.Empty;
			GalaxyNormal = Vector3.forward;
		}

		protected override void OnScale(Vector3 scale)
		{
			galaxyNameLabelPositionScaleArea.localScale = scale;
		}

		protected override void OnPosition(Vector3 position)
		{
			galaxyNameLabelPositionScaleArea.position = position.NewY(transform.position.y);

			canvasArea.position = position;

			line.SetPosition(0, position);
			line.SetPosition(1, galaxyNameLabelPositionScaleArea.position);
		}
	}

	public interface IClusterView : IGalaxyView
	{
		Vector3 GalaxyNormal { set; }
		string GalaxyName { set; }
	}
}
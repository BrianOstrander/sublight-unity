using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class ClusterView : GalaxyView, IClusterView
	{
		[SerializeField]
		TextCurve galaxyNameLabel;
		[SerializeField]
		CanvasGroup group;
		[SerializeField]
		Transform lookAtCameraArea;

		public string GalaxyName { set { galaxyNameLabel.Text = value ?? string.Empty; } }

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				group.alpha = value;
			}
		}

		public override void Reset()
		{
			base.Reset();

			GalaxyName = string.Empty;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(lookAtCameraArea.position, lookAtCameraArea.position + lookAtCameraArea.forward);
		}
	}

	public interface IClusterView : IGalaxyView
	{
		string GalaxyName { set; }
	}
}
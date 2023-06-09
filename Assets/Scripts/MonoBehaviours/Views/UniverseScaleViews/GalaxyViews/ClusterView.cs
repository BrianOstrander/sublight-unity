﻿using System;

using UnityEngine;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class ClusterView : GalaxyView, IClusterView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
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
		AnimationCurve lineRadiusOpacity;
		[SerializeField]
		MeshRenderer[] meshes;
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		Transform verticalLookAtArea;
		[SerializeField]
		CanvasGroup interactableArea;
		[SerializeField]
		TextMeshProUGUI detailNameLabel;
		[SerializeField]
		TextMeshProUGUI detailLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string GalaxyName
		{
			set
			{
				value = value ?? string.Empty;
				galaxyNameLabel.Text = value;
				detailNameLabel.text = value;
			}
		}
		public string DetailText { set { detailLabel.text = value ?? string.Empty; } }

		public Vector3 GalaxyNormal { set { galaxyRotationArea.LookAt(galaxyRotationArea.position + value.normalized); } }
		public float AlertHeightMultiplier { set; private get; }

		public override void SetGalaxy(Texture2D texture)
		{
			base.SetGalaxy(texture);
			foreach (var mesh in meshes)
			{
				mesh.material.SetVector(ShaderConstants.HoloTextureColorAlphaMasked.WorldOrigin, GridOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloTextureColorAlphaMasked.WorldRadius, GridRadius);
			}
		}

		public override bool Interactable
		{
			get { return base.Interactable; }

			set
			{
				base.Interactable = value;
				interactableArea.interactable = value;
				interactableArea.blocksRaycasts = value;
				interactableArea.alpha = value ? 1f : 0f;
			}
		}

		public Action Click { set; private get; }

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
			line.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, opacity * lineRadiusOpacity.Evaluate(RadiusNormal));
			foreach (var mesh in meshes)
			{
				mesh.material.SetFloat(ShaderConstants.HoloTextureColorAlphaMasked.Alpha, opacity);
			}
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + App.V.CameraForward.FlattenY());
			verticalLookAtArea.LookAt(verticalLookAtArea.position + App.V.CameraForward);
		}

		public override void Reset()
		{
			base.Reset();

			line.useWorldSpace = true;

			GalaxyName = string.Empty;
			DetailText = string.Empty;
			GalaxyNormal = Vector3.forward;
			Click = ActionExtensions.Empty;
			AlertHeightMultiplier = 0f;
		}

		protected override void OnScale(Vector3 scale, Vector3 rawScale)
		{
			//Debug.Break();
			galaxyNameLabelPositionScaleArea.localScale = scale;
		}

		protected override void OnPosition(Vector3 position, Vector3 rawPosition)
		{
			galaxyNameLabelPositionScaleArea.position = position.NewY(transform.position.y);

			lookAtArea.position = position.NewY(position.y + (TotalGalaxyHeight * (1f + AlertHeightMultiplier)));

			line.SetPosition(0, position);
			line.SetPosition(1, galaxyNameLabelPositionScaleArea.position);
			line.material.SetFloat(ShaderConstants.HoloTextureColorAlpha.Alpha, OpacityStack * lineRadiusOpacity.Evaluate(GetRadiusNormal(line.transform.position)));
		}

		#region Events
		public void OnClick()
		{
			if (Click != null) Click();
		}
		#endregion
	}

	public interface IClusterView : IGalaxyView
	{
		Vector3 GalaxyNormal { set; }
		string GalaxyName { set; }
		string DetailText { set; }
		Action Click { set; }
		float AlertHeightMultiplier { set; }
	}
}
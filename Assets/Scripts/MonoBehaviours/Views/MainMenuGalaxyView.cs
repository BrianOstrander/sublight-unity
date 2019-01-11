using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using LunraGames;

namespace LunraGames.SubLight.Views
{
	public class MainMenuGalaxyView : View, IMainMenuGalaxyView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Vector3 rotationSpeed;
		[SerializeField]
		Transform rotationAnchor;
		[SerializeField]
		AnimationCurve[] revealCurves;
		[SerializeField]
		Color[] previewColors = new Color[0];
		[SerializeField]
		MeshRenderer[] previewMeshes = new MeshRenderer[0];
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public Texture2D GalaxyPreview
		{
			set
			{
				for (var i = 0; i < previewMeshes.Length; i++)
				{
					var color = previewColors.Length == 0 ? Color.black : previewColors[Mathf.Min(i, previewColors.Length)];
					var mesh = previewMeshes[i];
					mesh.material.SetTexture(ShaderConstants.HoloGalaxyPreviewBasic.LayerTexture, value);
					mesh.material.SetInt(ShaderConstants.HoloGalaxyPreviewBasic.Channel, i);
					mesh.material.SetColor(ShaderConstants.HoloGalaxyPreviewBasic.ChannelColor, color);
				}
			}
		}

		float RevealProgress
		{
			set
			{
				for (var i = 0; i < previewMeshes.Length; i++)
				{
					var revealed = revealCurves.Length == 0 ? 0f : revealCurves[Mathf.Min(i, revealCurves.Length)].Evaluate(value);

					var mesh = previewMeshes[i];
					mesh.material.SetFloat(ShaderConstants.HoloGalaxyPreviewBasic.Revealed, revealed);
					//mesh.material.SetTexture(ShaderConstants.HoloGalaxyPreviewBasic.Layer, value);


				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			GalaxyPreview = null;
			RevealProgress = 0f;
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			RevealProgress = scalar;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			rotationAnchor.rotation = Quaternion.Euler(rotationAnchor.rotation.eulerAngles + (rotationSpeed * delta));
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			RevealProgress = 1f - scalar;
		}
	}

	public interface IMainMenuGalaxyView : IView
	{
		Texture2D GalaxyPreview { set; }
	}
}
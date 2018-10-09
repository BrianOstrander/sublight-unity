﻿using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class MenuLogoView : View, IMenuLogoView
	{
		[SerializeField]
		MeshRenderer MaskMesh;

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				MaskMesh.material.SetFloat(ShaderConstants.HoloMask.Opacity, Opacity);
			}
		}
	}

	public interface IMenuLogoView : IView
	{

	}
}
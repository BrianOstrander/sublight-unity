using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class DestructionSystemView : View, IDestructionSystemView
	{
		[SerializeField]
		Vector3 rimQuadRotation;
		[SerializeField]
		Transform rimRoot;
		[SerializeField]
		Transform rimQuad;
		[SerializeField]
		Renderer voidRender;

		float radius;
		public float Radius 
		{
			private get { return radius; }
			set
			{
				radius = value;
				rimRoot.localScale = Vector3.one * UniversePosition.ToUnityDistance(radius) * 2f;
			}
		}
		public Vector3 UnityPosition
		{
			set { Root.position = value; }
			get { return Root.position; }
		}
		public UniversePosition UniversePosition { set; get; }
		public Action<bool> Highlight { set; private get; }
		public Action Click { set; private get; }
		public Texture VoidTexture
		{
			set
			{
				voidRender.material.SetTexture(ShaderConstants.VoidRim.VoidInterior, value);
			}
		}

		public override void Reset()
		{
			base.Reset();

			Radius = 0f;
			UnityPosition = Vector3.zero;
			UniversePosition = UniversePosition.Zero;
			Highlight = ActionExtensions.GetEmpty<bool>();
			Click = ActionExtensions.Empty;
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			rimQuad.Rotate(rimQuadRotation * delta);
		}

		#region Events
		public void OnEnter() { Highlight(true); }
		public void OnExit() { Highlight(false); }
		public void OnClick() { Click(); }
		#endregion

		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
#if UNITY_EDITOR
			Handles.color = Color.cyan;
			Handles.DrawWireDisc(transform.position, Vector3.up, UniversePosition.ToUnityDistance(Radius));
#endif
		}
	}

	public interface IDestructionSystemView : IGridTransform
	{
		float Radius { set; }
		Action<bool> Highlight { set; }
		Action Click { set; }
		Texture VoidTexture { set; }
	}
}
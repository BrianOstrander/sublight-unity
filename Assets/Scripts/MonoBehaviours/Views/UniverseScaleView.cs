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
		Transform scaleArea;
		[SerializeField]
		Transform positionArea;

		public Vector3 Scale
		{
			set
			{
				if (scaleArea == null) return;

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

				scaleArea.localScale = value;
			}
		}

		public Vector3 Position { set { if (positionArea != null) positionArea.position = value; } }
	}

	public interface IUniverseScaleView : IView
	{
		Vector3 Scale { set; }
		Vector3 Position { set; }
	}
}
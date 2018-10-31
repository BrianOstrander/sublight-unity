using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GalaxyPresenter : UniverseScalePresenter<IGalaxyView>
	{
		UniversePosition scaleInUniverse;
		UniversePosition positionInUniverse;

		protected override UniversePosition ScaleInUniverse { get { return scaleInUniverse; } }
		protected override UniversePosition PositionInUniverse { get { return positionInUniverse; } }

		public GalaxyPresenter(GameModel model) : base(model, UniverseScales.Galactic)
		{
			scaleInUniverse = model.Galaxy.GalaxySize;
			positionInUniverse = model.Galaxy.GalaxyOrigin;
			Debug.Log(scaleInUniverse);

		}
	}
}
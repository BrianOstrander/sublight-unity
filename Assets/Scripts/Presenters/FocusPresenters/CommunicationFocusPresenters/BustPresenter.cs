using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BustPresenter : CommunicationFocusPresenter<IBustView>
	{
		GameModel model;

		public BustPresenter(GameModel model)
		{
			this.model = model;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();


		}

		protected override void OnUpdateEnabled()
		{
			//OnLabel(labelProperty.Value);
			//View.PushOpacity(() => scaleModel.Opacity.Value);
			//View.PushOpacity(() => model.GridScaleOpacity.Value);

			View.InitializeBusts(
				new BustBlock
				{
					BustId = "lol",
					TitleSource = "Some",
					TitleClassification = "Ark",
					TransmitionType = "Transmission",
					TransmitionStrength = "Strong",
					TransmitionStrengthIndex = 0,
					PlacardName = "S. Cap",
					PlacardDescription = "Captain",
					AvatarStaticIndex = 1
				}
			);

			App.Heartbeat.Wait(() => View.FocusBust("lol"), 0.5f);
		}

		#region Events

		#endregion
	}
}
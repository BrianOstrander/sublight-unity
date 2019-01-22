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
				},
				new BustBlock
				{
					BustId = "lol2",
					TitleSource = "Other",
					TitleClassification = "Ark",
					TransmitionType = "Transmission",
					TransmitionStrength = "Weak",
					TransmitionStrengthIndex = 2,
					PlacardName = "S. Caap",
					PlacardDescription = "Captain'",
					AvatarStaticIndex = 0
				}
			);

			View.FocusBust("lol", true);

			App.Heartbeat.Wait(() => View.FocusBust("lol2"), 0.5f);
			App.Heartbeat.Wait(() => View.FocusBust("lol"), 1f);
		}

		#region Events

		#endregion
	}
}
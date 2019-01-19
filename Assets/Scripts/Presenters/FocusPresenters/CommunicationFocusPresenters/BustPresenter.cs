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
		}

		#region Events

		#endregion
	}
}
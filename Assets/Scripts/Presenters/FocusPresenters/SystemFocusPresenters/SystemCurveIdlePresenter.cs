using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class SystemCurveIdlePresenter : SystemFocusPresenter<ISystemCurveIdleView>
	{
		GameModel model;

		public SystemCurveIdlePresenter(
			GameModel model
		)
		{
			this.model = model;

			model.Context.GridInput.Changed += OnGridInput;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			model.Context.GridInput.Changed -= OnGridInput;
		}

		protected override void OnUpdateEnabled()
		{

		}

		protected override void OnUpdateDisabled()
		{

		}

		#region Events
		void OnGridInput(GridInputRequest gridInput)
		{
			if (!View.Visible) return;

			View.InputHack();
		}
		#endregion
	}
}
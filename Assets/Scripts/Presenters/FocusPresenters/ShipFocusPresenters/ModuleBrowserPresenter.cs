using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ModuleBrowserPresenter : ShipFocusPresenter<IModuleBrowserView>
	{
		GameModel model;
		
		public ModuleBrowserPresenter(
			GameModel model
		)
		{
			this.model = model;
			
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
		}
		
		protected override void OnUpdateEnabled()
		{
			// Debug.Log("uhhh browser enabled...");
			var entries = new List<ModuleBrowserBlock>();

			foreach (var module in model.Ship.Modules.Value)
			{
				var current = new ModuleBrowserBlock();
				current.Id = module.Id.Value;
				current.Name = module.Name.Value;
				current.Type = module.Type.Value.ToString();
				current.Description = module.Description;
				
				entries.Add(current);
			}

			View.Entries = entries.ToArray();
			View.Selected = entries.FirstOrDefault().Id;
		}

	}
}
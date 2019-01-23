using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LunraGames.SubLight.Views
{
	public class ConversationView : View, IConversationView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		GameObject conversationArea;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public override void Reset()
		{
			base.Reset();
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			//lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		#region Events
		#endregion
	}

	public interface IConversationView : IView
	{
	}
}
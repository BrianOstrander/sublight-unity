using UnityEngine;

namespace LunraGames.SubLight
{
	public class QuitPayload : IStatePayload
	{
		/// <summary>
		/// Who requested this quit.
		/// </summary>
		public string Requester;
	}

	public class QuitState : State<QuitPayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Quit; } }

		#region Idle
		protected override void Idle()
		{
			Debug.Log("Quit requested by " + (string.IsNullOrEmpty(Payload.Requester) ? "< null or empty >" : Payload.Requester));

			if (Application.isEditor) Debug.LogError("Quiting from the editor is not supported, you are now stuck here...");

			Application.Quit();
		}
		#endregion

		#region Events
		#endregion
	}
}
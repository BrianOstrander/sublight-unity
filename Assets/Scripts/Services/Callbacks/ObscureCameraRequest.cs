namespace LunraGames.SubLight
{
	public struct ObscureCameraRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static ObscureCameraRequest Obscure { get { return new ObscureCameraRequest(States.Request, true); } }
		public static ObscureCameraRequest UnObscure { get { return new ObscureCameraRequest(States.Request, false); } }

		public readonly States State;
		public readonly bool IsObscured;

		public ObscureCameraRequest(States state, bool isObscured)
		{
			State = state;
			IsObscured = isObscured;
		}

		public ObscureCameraRequest Duplicate(States state = States.Unknown, bool? isObscured = null)
		{
			return new ObscureCameraRequest(
				state == States.Unknown ? State : state,
				isObscured.HasValue ? isObscured.Value : IsObscured
			);
		}
	}
}
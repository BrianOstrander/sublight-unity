namespace LunraGames.SpaceFarm.Views
{
	public class CameraView : View, ICameraView
	{
		public override void Reset()
		{
			base.Reset();
		}
	}

	public interface ICameraView : IView {}
}
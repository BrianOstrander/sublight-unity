namespace LunraGames.SubLight.Views
{
	public class GenericFocusCameraView : FocusCameraView, IGenericFocusCameraView 
	{
		public int CullingMask { set { FocusCamera.cullingMask = value; } }
	}

	public interface IGenericFocusCameraView : IFocusCameraView 
	{
		int CullingMask { set; }
	}
}
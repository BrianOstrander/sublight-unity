using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public enum ValidatedFilterStates
	{
		Unknown = 0,
		Processing = 10,
		Valid = 20,
		Invalid = 30
	}
	
	public abstract class ValidatedFilterEntryModel<T> : ValueFilterEntryModel<T>, IValidatedFilterEntryModel
	{
		
#if UNITY_EDITOR
		[JsonIgnore] public ValidatedFilterStates ValidationState { get; private set; }
		[JsonIgnore] public string[] ValidationIssues { get; private set; } = new string[0];

		public void SetValidation(
			ValidatedFilterStates state,
			params string[] issues
		)
		{
			ValidationState = state;
			ValidationIssues = issues;
		}
#endif
		
		public ValidatedFilterEntryModel()
		{
#if UNITY_EDITOR
			FilterValue.Changed += ResetValidation;
#endif
		}
		
#if UNITY_EDITOR
		protected void ResetValidation(T value = default) => SetValidation(ValidatedFilterStates.Unknown);
#endif
	}
	
	public interface IValidatedFilterEntryModel : IValueFilterEntryModel
	{
#if UNITY_EDITOR
		void SetValidation(
			ValidatedFilterStates state,
			params string[] issues
		);
#endif
	}
}
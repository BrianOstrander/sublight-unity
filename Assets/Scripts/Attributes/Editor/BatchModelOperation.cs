using System;

namespace LunraGames.SubLight
{
	[AttributeUsage(AttributeTargets.Method)]
	public class BatchModelOperation : Attribute
	{
		public Type ModelType;
		public string Name;
		public string Description;

		public BatchModelOperation(
			Type modelType,
			string name = null,
			string description = null
		)
		{
			ModelType = modelType;
			Name = name;
			Description = description;
		}
	}
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class BatchModelOperationCache<T>
		where T : SaveModel
	{
		public class Entry
		{
			public MethodInfo Method;
			public BatchModelOperation Operation;
			public Action<T, Action<T, RequestResult>, bool> Run;

			public string Name
			{
				get
				{
					if (string.IsNullOrEmpty(Operation.Name)) return ObjectNames.NicifyVariableName(Method.Name);
					return Operation.Name;
				}
			}

			public string Description
			{
				get
				{
					if (string.IsNullOrEmpty(Operation.Description)) return "No description provided.";
					return Operation.Description;
				}
			}
		}

		public List<Entry> Entries { get; private set; }

		public BatchModelOperationCache()
		{
			var typeName = typeof(T).Name;
			var doneType = typeof(Action<T, RequestResult>);

			Entries = new List<Entry>();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var method in assemblies.SelectMany(a => a.GetTypes()).SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)))
			{
				foreach (var attribute in method.GetCustomAttributes(typeof(BatchModelOperation), true).Cast<BatchModelOperation>())
				{
					if (attribute.ModelType == typeof(SaveModel))
					{
						Debug.LogError("Generic model batch operations not supported yet");
						continue;
					}
					if (attribute.ModelType != typeof(T)) continue;

					var parameters = method.GetParameters();

					var validationPrefix = typeName + " batch operation \"" + method.Name + "\" ";

					if (parameters.Length != 3)
					{
						Debug.LogError(validationPrefix + "requires 3 parameters");
						continue;
					}
					if (parameters[0].ParameterType != typeof(T))
					{
						Debug.LogError(validationPrefix + "requires the first parameter be of type " + typeName);
						continue;
					}
					if (parameters[1].ParameterType != doneType)
					{
						Debug.LogError(validationPrefix + "requires the second parameter be of type " + doneType.Name);
						continue;
					}
					if (parameters[2].ParameterType != typeof(bool))
					{
						Debug.LogError(validationPrefix + "requires the third parameter be of type " + typeof(bool).Name);
						continue;
					}

					Entries.Add(
						new Entry
						{
							Method = method,
							Operation = attribute,
							Run = (model, done, write) =>
							{
								if (model == null) throw new ArgumentNullException("model");
								if (done == null) throw new ArgumentNullException("done");

								try
								{
									method.Invoke(
										null,
										new object[]
										{
											model,
											done,
											write
										}
									);
								}
								catch (Exception e)
								{
									Debug.LogException(e);
									done(model, RequestResult.Failure("Encountered exception: " + e.Message));
								}
							}
						}
					);
				}
			}
		}
	}
}
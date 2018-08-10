using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class EncounterEditorWindow
	{
		bool OnLogBegin(int count, int maxCount, EncounterInfoModel infoModel, EncounterLogModel model, bool isMoving, out int indexDelta, ref string beginning, ref string ending)
		{
			var deleted = false;
			indexDelta = 0;
			var isAlternate = count % 2 == 0;
			if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.gray);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (isAlternate) EditorGUILayoutExtensions.PopColor();
			GUILayout.BeginHorizontal();
			{
				var header = "#"+(count + 1)+" | "+model.LogType + ".LogId:";
				GUILayout.Label(header, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
				EditorGUILayout.SelectableLabel(model.LogId, EditorStyles.boldLabel);
				if (isMoving)
				{
					GUILayout.Space(10f);
					EditorGUILayoutExtensions.PushEnabled(0 < count);
					if (GUILayout.Button("^", EditorStyles.miniButtonLeft, GUILayout.Width(60f), GUILayout.Height(18f)))
					{
						indexDelta = -1;
					}
					EditorGUILayoutExtensions.PopEnabled();
					EditorGUILayoutExtensions.PushEnabled(count < maxCount - 1);
					if (GUILayout.Button("v", EditorStyles.miniButtonRight, GUILayout.Width(60f), GUILayout.Height(18f)))
					{
						indexDelta = 1;
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
				else
				{
					if (EditorGUILayout.ToggleLeft("Beginning", model.Beginning.Value, GUILayout.Width(70f)) && !model.Beginning.Value)
					{
						beginning = model.LogId;
					}
					if (EditorGUILayout.ToggleLeft("Ending", model.Ending.Value, GUILayout.Width(60f)) && !model.Ending.Value)
					{
						ending = model.LogId;
					}
				}
				EditorGUILayoutExtensions.PushEnabled(!isMoving);
				deleted = EditorGUILayoutExtensions.XButton();
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();

			selectedEncounterModified |= EditorGUI.EndChangeCheck();
			{
				// Pausing checks for foldout, since this shouldn't signal that the object is savable.
				if (!model.HasNotes) EditorGUILayoutExtensions.PushColor(Color.grey);
				model.ShowNotes.Value = EditorGUILayout.Foldout(model.ShowNotes.Value, new GUIContent("Notes", "Internal notes for production."), true);
				if (!model.HasNotes) EditorGUILayoutExtensions.PopColor();
			}
			EditorGUI.BeginChangeCheck();

			if (model.ShowNotes.Value)
			{
				EditorGUILayoutExtensions.PushIndent();
				{
					model.Notes.Value = EditorGUILayoutExtensions.TextDynamic(model.Notes.Value);
				}
				EditorGUILayoutExtensions.PopIndent();
			}

			OnLogDuration(infoModel, model);

			return deleted;
		}

		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model, EncounterLogModel nextModel)
		{
			switch (model.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(infoModel, model as TextEncounterLogModel, nextModel);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(infoModel, model as KeyValueEncounterLogModel, nextModel);
					break;
				case EncounterLogTypes.Inventory:
					OnInventoryLog(infoModel, model as InventoryEncounterLogModel, nextModel);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized EncounterLogType: " + model.LogType, MessageType.Error);
					break;
			}
		}

		void OnLogDuration(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			EditorGUILayoutExtensions.PushEnabled(model.EditableDuration);

			if (model.EditableDuration)
			{
				model.Duration.Value = EditorGUILayout.FloatField("Duration", model.Duration);
			}
			else EditorGUILayout.FloatField("Duration", model.TotalDuration);

			EditorGUILayoutExtensions.PopEnabled();
		}

		#region Text Logs
		void OnTextLog(EncounterInfoModel infoModel, TextEncounterLogModel model, EncounterLogModel nextModel)
		{
			model.Header.Value = EditorGUILayoutExtensions.TextDynamic("Header", model.Header.Value);
			model.Message.Value = EditorGUILayoutExtensions.TextDynamic("Message", model.Message.Value);
			OnLinearLog(infoModel, model, nextModel);
		}
		#endregion

		#region KeyValue Logs
		void OnKeyValueLog(EncounterInfoModel infoModel, KeyValueEncounterLogModel model, EncounterLogModel nextModel)
		{
			var targets = Enum.GetValues(typeof(KeyValueTargets)).Cast<KeyValueTargets>().ToList();
			var kvTypes = Enum.GetValues(typeof(KeyValueOperations)).Cast<KeyValueOperations>().ToList();

			var labels = new List<string>(new string[] { "- Select Operation -" });
			var onSelections = new Dictionary<int, Action>();
			var index = 1;

			foreach (var target in targets)
			{
				if (target == KeyValueTargets.Unknown) continue;
				labels.Add("--- " + target + " ---");
				index++;

				foreach (var kvType in kvTypes)
				{
					if (kvType == KeyValueOperations.Unknown) continue;
					labels.Add(target+"."+kvType);
					onSelections.Add(
						index,
						() => OnKeyValueLogSpawn(infoModel, model, target, kvType)
					);
					index++;
				}
			}

			var selection = 0;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Append New Key Value Operation: ", GUILayout.ExpandWidth(false));
				selection = EditorGUILayout.Popup(selection, labels.ToArray());
			}
			GUILayout.EndHorizontal();
			Action onSelection;
			if (onSelections.TryGetValue(selection, out onSelection)) onSelection();

			var deleted = string.Empty;
			var isAlternate = false;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					foreach (var operation in model.Operations.Value)
					{
						isAlternate = !isAlternate;

						if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.grey.NewV(0.5f));
						GUILayout.BeginVertical(EditorStyles.helpBox);
						if (isAlternate) EditorGUILayoutExtensions.PopColor();

						if (OnKeyValueLogHeader(infoModel, model, operation)) deleted = operation.OperationId.Value;
						switch (operation.Operation)
						{
							case KeyValueOperations.SetString:
								OnKeyValueLogSetString(infoModel, model, operation as SetStringOperationModel);
								break;
							default:
								Debug.LogError("Unrecognized KeyValueOperation: " + operation.Operation);
								break;
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Operations.Value = model.Operations.Value.Where(kv => kv.OperationId != deleted).ToArray();
			}

			OnLinearLog(infoModel, model, nextModel);
		}

		bool OnKeyValueLogHeader(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueOperationModel operation
		)
		{
			var deleted = false;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(operation.Operation + ":", GUILayout.ExpandWidth(false));
				operation.Target.Value = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Target -", operation.Target.Value);
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();
			return deleted;
		}

		void OnKeyValueLogSetString(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			SetStringOperationModel operation
		)
		{
			operation.Key.Value = EditorGUILayout.TextField("Key", operation.Key.Value);
			operation.Value.Value = EditorGUILayoutExtensions.TextDynamic("Value", operation.Value.Value);
		}

		void OnKeyValueLogSpawn(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			KeyValueTargets target,
			KeyValueOperations operation
		)
		{
			var guid = Guid.NewGuid().ToString();
			switch(operation)
			{
				case KeyValueOperations.SetString:
					var setString = new SetStringOperationModel();
					setString.OperationId.Value = guid;
					setString.Target.Value = target;
					model.Operations.Value = model.Operations.Value.Append(setString).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized KeyValueOperation: " + operation);
					break;
			}
		}
		#endregion

		#region Inventory Logs
		void OnInventoryLog(EncounterInfoModel infoModel, InventoryEncounterLogModel model, EncounterLogModel nextModel)
		{
			var selection = InventoryOperations.Unknown;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Append New Inventory Operation: ", GUILayout.ExpandWidth(false));
				selection = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Operation -", selection);
			}
			GUILayout.EndHorizontal();

			if (selection != InventoryOperations.Unknown) OnInventoryLogSpawn(infoModel, model, selection);

			var deleted = string.Empty;
			var isAlternate = false;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					foreach (var operation in model.Operations.Value)
					{
						isAlternate = !isAlternate;

						if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.grey.NewV(0.5f));
						GUILayout.BeginVertical(EditorStyles.helpBox);
						if (isAlternate) EditorGUILayoutExtensions.PopColor();

						if (OnInventoryLogHeader(infoModel, model, operation)) deleted = operation.OperationId.Value;
						switch (operation.Operation)
						{
							case InventoryOperations.AddResources:
								OnInventoryLogAddResource(infoModel, model, operation as AddResourceOperationModel);
								break;
							case InventoryOperations.AddInstance:
								OnInventoryLogAddInstance(infoModel, model, operation as AddInstanceOperationModel);
								break;
							default:
								Debug.LogError("Unrecognized InventoryOperation: " + operation.Operation);
								break;
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Operations.Value = model.Operations.Value.Where(kv => kv.OperationId != deleted).ToArray();
			}

			OnLinearLog(infoModel, model, nextModel);
		}

		bool OnInventoryLogHeader(
			EncounterInfoModel infoModel,
			InventoryEncounterLogModel model,
			InventoryOperationModel operation
		)
		{
			var deleted = false;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(operation.Operation.ToString());
				deleted = EditorGUILayoutExtensions.XButton();
			}
			GUILayout.EndHorizontal();
			return deleted;
		}

		void OnInventoryLogAddResource(
			EncounterInfoModel infoModel,
			InventoryEncounterLogModel model,
			AddResourceOperationModel operation
		)
		{
			EditorGUILayoutResource.Values(operation.Value);
		}

		void OnInventoryLogAddInstance(
			EncounterInfoModel infoModel,
			InventoryEncounterLogModel model,
			AddInstanceOperationModel operation
		)
		{
			operation.InventoryId.Value = EditorGUILayout.TextField("Inventory Id", operation.InventoryId);
		}

		void OnInventoryLogSpawn(
			EncounterInfoModel infoModel,
			InventoryEncounterLogModel model,
			InventoryOperations operation
		)
		{
			var guid = Guid.NewGuid().ToString();
			switch (operation)
			{
				case InventoryOperations.AddResources:
					var addResource = new AddResourceOperationModel();
					addResource.OperationId.Value = guid;
					model.Operations.Value = model.Operations.Value.Append(addResource).ToArray();
					break;
				case InventoryOperations.AddInstance:
					var addInstance = new AddInstanceOperationModel();
					addInstance.OperationId.Value = guid;
					model.Operations.Value = model.Operations.Value.Append(addInstance).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized InventoryOperation: " + operation);
					break;
			}
		}
		#endregion

		void OnLinearLog(EncounterInfoModel infoModel, LinearEncounterLogModel model, EncounterLogModel nextModel)
		{
			var nextId = nextModel == null ? string.Empty : nextModel.LogId.Value;
			var options = infoModel.Logs.All.Value.OrderBy(l => l.Index.Value).Where(l => l.LogId != model.LogId).Select(l => l.LogId.Value).Prepend("- Select Next Log -").ToArray();
			var optionNames = options.Select(l => l == nextId ? (l + " <- Next") : l).ToArray();
			var index = 0;
			if (!string.IsNullOrEmpty(model.NextLogId.Value))
			{
				for (var i = 0; i < options.Length; i++)
				{
					if (options[i] == model.NextLogId.Value)
					{
						index = i;
						break;
					}
				}
			}
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Next Log: ", GUILayout.Width(55f));
				index = EditorGUILayout.Popup(index, optionNames);
			}
			GUILayout.EndHorizontal();
			if (index == 0) model.NextLogId.Value = null;
			else model.NextLogId.Value = options[index];
		}

		void OnLogEnd(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			GUILayout.EndVertical();
		}
	}
}
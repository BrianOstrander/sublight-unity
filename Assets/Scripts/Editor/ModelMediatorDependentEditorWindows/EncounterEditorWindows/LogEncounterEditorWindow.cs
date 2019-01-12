using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow
	{
		bool OnLogBegin(
			int count,
			int maxCount,
			EncounterInfoModel infoModel,
			EncounterLogModel model,
			bool isMoving,
			out int indexDelta,
			ref string beginning
		)
		{
			var deleted = false;
			indexDelta = 0;
			var isAlternate = count % 2 == 0;

			EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.cyan.NewH(0.5f), Color.cyan.NewH(0.6f), isAlternate);
			GUILayout.Space(2f);

			GUILayout.BeginVertical(EditorStyles.miniButton);
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUILayoutExtensions.PushColor(Color.cyan.NewH(0.55f).NewS(0.4f));
					var header = "#" + (count + 1) + " | " + model.LogType + (model.HasName ? ".Name:" : ".LogId");
						
					GUILayout.Label(header, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
					EditorGUILayout.SelectableLabel(model.HasName ? model.Name.Value : model.LogId.Value, EditorStyles.boldLabel);
					EditorGUILayoutExtensions.PopColor();
					if (isMoving)
					{
						GUILayout.Space(10f);
						EditorGUILayoutExtensions.PushEnabled(0 < count);
						if (GUILayout.Button("^", EditorStyles.miniButtonLeft, GUILayout.Width(30f), GUILayout.Height(18f)))
						{
							indexDelta = -1;
						}
						if (GUILayout.Button("^^", EditorStyles.miniButtonMid, GUILayout.Width(30f), GUILayout.Height(18f)))
						{
							indexDelta = -2;
						}
						EditorGUILayoutExtensions.PopEnabled();
						EditorGUILayoutExtensions.PushEnabled(count < maxCount - 1);
						if (GUILayout.Button("vv", EditorStyles.miniButtonMid, GUILayout.Width(30f), GUILayout.Height(18f)))
						{
							indexDelta = 2;
						}
						if (GUILayout.Button("v", EditorStyles.miniButtonRight, GUILayout.Width(30f), GUILayout.Height(18f)))
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
						model.Ending.Value = EditorGUILayout.ToggleLeft("Ending", model.Ending.Value, GUILayout.Width(60f));
					}
					EditorGUILayoutExtensions.PushEnabled(!isMoving);
					deleted = EditorGUILayoutExtensions.XButton();
					EditorGUILayoutExtensions.PopEnabled();
				}
			}
			GUILayout.EndVertical();

			model.Name.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Name", "Internal name for production."), model.Name.Value);

			EditorGUIExtensions.PauseChangeCheck();
			{
				// Pausing checks for foldout, since this shouldn't signal that the object is savable.
				if (!model.HasNotes) EditorGUILayoutExtensions.PushColor(Color.gray);
				model.ShowNotes.Value = EditorGUILayout.Foldout(model.ShowNotes.Value, new GUIContent("Notes", "Internal notes for production."), true);
				if (!model.HasNotes) EditorGUILayoutExtensions.PopColor();
			}
			EditorGUIExtensions.UnPauseChangeCheck();

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

		void OnLog(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			switch (model.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(infoModel, model as TextEncounterLogModel);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(infoModel, model as KeyValueEncounterLogModel);
					break;
				case EncounterLogTypes.Inventory:
					//OnInventoryLog(infoModel, model as InventoryEncounterLogModel);
					throw new NotImplementedException("Inventory log not implimented yet");
				case EncounterLogTypes.Switch:
					OnSwitchLog(infoModel, model as SwitchEncounterLogModel);
					break;
				case EncounterLogTypes.Button:
					OnButtonLog(infoModel, model as ButtonEncounterLogModel);
					break;
				case EncounterLogTypes.Encyclopedia:
					OnEncyclopediaLog(infoModel, model as EncyclopediaEncounterLogModel);
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
		void OnTextLog(EncounterInfoModel infoModel, TextEncounterLogModel model)
		{
			model.Header.Value = EditorGUILayoutExtensions.TextDynamic("Header", model.Header.Value);
			model.Message.Value = EditorGUILayoutExtensions.TextDynamic("Message", model.Message.Value);
			OnLinearLog(infoModel, model);
		}
		#endregion

		#region KeyValue Logs
		void OnKeyValueLog(EncounterInfoModel infoModel, KeyValueEncounterLogModel model)
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

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{
							if (OnKeyValueLogHeader(infoModel, model, operation)) deleted = operation.OperationId.Value;
							switch (operation.Operation)
							{
								case KeyValueOperations.SetString:
									OnKeyValueLogSetString(infoModel, model, operation as SetStringOperationModel);
									break;
								case KeyValueOperations.SetBoolean:
									OnKeyValueLogSetBoolean(infoModel, model, operation as SetBooleanOperationModel);
									break;
								default:
									Debug.LogError("Unrecognized KeyValueOperation: " + operation.Operation);
									break;
							}
						}
						EditorGUILayoutExtensions.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Operations.Value = model.Operations.Value.Where(kv => kv.OperationId != deleted).ToArray();
			}

			OnLinearLog(infoModel, model);
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
				operation.Target.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Target -", operation.Target.Value);
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

		void OnKeyValueLogSetBoolean(
			EncounterInfoModel infoModel,
			KeyValueEncounterLogModel model,
			SetBooleanOperationModel operation
		)
		{
			operation.Key.Value = EditorGUILayout.TextField("Key", operation.Key.Value);
			operation.Value.Value = EditorGUILayoutExtensions.ToggleButton(new GUIContent("Value"), operation.Value.Value);
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
				case KeyValueOperations.SetBoolean:
					var setBoolean = new SetBooleanOperationModel();
					setBoolean.OperationId.Value = guid;
					setBoolean.Target.Value = target;
					model.Operations.Value = model.Operations.Value.Append(setBoolean).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized KeyValueOperation: " + operation);
					break;
			}
		}
		#endregion

		#region Inventory Logs
		/*
		void OnInventoryLog(EncounterInfoModel infoModel, InventoryEncounterLogModel model)
		{
			var selection = InventoryOperations.Unknown;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Append New Inventory Operation: ", GUILayout.ExpandWidth(false));
				selection = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", selection);
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

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{
							if (OnInventoryLogHeader(infoModel, model, operation)) deleted = operation.OperationId.Value;
							switch (operation.Operation)
							{
								case InventoryOperations.AddResources:
									OnInventoryLogAddResource(infoModel, model, operation as AddResourceOperationModel);
									break;
								case InventoryOperations.AddInstance:
									OnInventoryLogAddInstance(infoModel, model, operation as AddInstanceOperationModel);
									break;
								case InventoryOperations.AddRandomInstance:
									OnInventoryLogAddRandomInstance(infoModel, model, operation as AddRandomInstanceOperationModel);
									break;
								default:
									Debug.LogError("Unrecognized InventoryOperation: " + operation.Operation);
									break;
							}
						}
						EditorGUILayoutExtensions.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Operations.Value = model.Operations.Value.Where(kv => kv.OperationId != deleted).ToArray();
			}

			OnLinearLog(infoModel, model);
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
				GUILayout.Label(operation.Operation.ToString(), EditorStyles.boldLabel);
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

		void OnInventoryLogAddRandomInstance(
			EncounterInfoModel infoModel,
			InventoryEncounterLogModel model,
			AddRandomInstanceOperationModel operation
		)
		{
			EditorGUILayoutValueFilter.Field(new GUIContent("Filtering", "Constraints for the selected inventory reference."), operation.Filtering);
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
				case InventoryOperations.AddRandomInstance:
					var addRandomInstance = new AddRandomInstanceOperationModel();
					addRandomInstance.OperationId.Value = guid;
					model.Operations.Value = model.Operations.Value.Append(addRandomInstance).ToArray();
					break;
				default:
					Debug.LogError("Unrecognized InventoryOperation: " + operation);
					break;
			}
		}
		*/
		#endregion

		#region Switch Logs
		void OnSwitchLog(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				"Append New Switch: ",
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result => OnSwitchLogSpawn(result, AppendNewLog(newSelection, infoModel))),
				EncounterLogBlankHandling.None,
				"- Select Target Log -",
				"< Blank >"
			);

			OnEdgedLog<SwitchEncounterLogModel, SwitchEdgeModel>(infoModel, model, OnSwitchLogEdge);
		}

		void OnSwitchLogSpawn(
			SwitchEdgeModel edge,
			string targetLogId
		)
		{
			edge.NextLogId.Value = targetLogId;
		}

		void OnSwitchLogEdge(
			EncounterInfoModel infoModel,
			SwitchEncounterLogModel model,
			SwitchEdgeModel edge
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				"Target Log: ",
				edge.NextLogId.Value,
				infoModel,
				model,
				existingSelection => edge.NextLogId.Value = existingSelection,
				newSelection => edge.NextLogId.Value = AppendNewLog(newSelection, infoModel),
				EncounterLogBlankHandling.Error,
				"- Select Target Log -"
			);

			EditorGUILayoutValueFilter.Field(
				new GUIContent("Filtering", "Passing this filter is required to continue to the target log."),
				edge.Filtering
			);
		}
		#endregion

		#region Button Logs
		void OnButtonLog(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				"Append New Button: ",
				null,
				infoModel,
				model,
				existingSelection => OnEdgedLogSpawn(model, result => OnButtonLogSpawn(result, existingSelection)),
				newSelection => OnEdgedLogSpawn(model, result =>OnButtonLogSpawn(result, AppendNewLog(newSelection, infoModel))),
				EncounterLogBlankHandling.None,
				"- Select Target Log -",
				"< Blank >"
			);

			OnEdgedLog<ButtonEncounterLogModel, ButtonEdgeModel>(infoModel, model, OnButtonLogEdge);
		}

		void OnButtonLogSpawn(
			ButtonEdgeModel edge,
			string targetLogId
		)
		{
			edge.NextLogId.Value = targetLogId;
		}

		void OnButtonLogEdge(
			EncounterInfoModel infoModel,
			ButtonEncounterLogModel model,
			ButtonEdgeModel edge
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				"Target Log: ",
				edge.NextLogId.Value,
				infoModel,
				model,
				existingSelection => edge.NextLogId.Value = existingSelection,
				newSelection => edge.NextLogId.Value = AppendNewLog(newSelection, infoModel),
				EncounterLogBlankHandling.Error,
				"- Select Target Log -"
			);

			edge.Message.Value = EditorGUILayoutExtensions.TextDynamic("Message", edge.Message.Value);

			GUILayout.BeginHorizontal();
			{
				edge.NotAutoUsed.Value = !EditorGUILayout.ToggleLeft(new GUIContent("Auto Used", "When this button is pressed, automatically set it to appear used the next time around."), !edge.NotAutoUsed.Value, GUILayout.Width(74f));
				edge.AutoDisableInteractions.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable Interactions", "When this button is pressed, automatically disable future interactions the next time around."), edge.AutoDisableInteractions.Value, GUILayout.Width(152f));
				edge.AutoDisableEnabled.Value = EditorGUILayout.ToggleLeft(new GUIContent("Auto Disable", "When this button is pressed, automatically set this button to be disabled and invisible the next time around."), edge.AutoDisableEnabled.Value, GUILayout.Width(90f));
			}
			GUILayout.EndHorizontal();

			EditorGUILayoutValueFilter.Field(new GUIContent("Used Filtering", "If this filter returns true, the button will appear used."), edge.UsedFiltering);
			EditorGUILayoutValueFilter.Field(new GUIContent("Interactable Filtering", "If this filter returns true, the button will be interactable."), edge.InteractableFiltering);
			EditorGUILayoutValueFilter.Field(new GUIContent("Enabled Filtering", "If this filter returns true, the button will be enabled and visible."), edge.EnabledFiltering);

		}
		#endregion

		#region Encyclopedia Logs
		void OnEncyclopediaLog(
			EncounterInfoModel infoModel,
			EncyclopediaEncounterLogModel model
		)
		{
			if (GUILayout.Button("Append New Encyclopedia Section")) OnEdgedLogSpawn(model, OnEncyclopediaLogSpawn);

			OnEdgedLog<EncyclopediaEncounterLogModel, EncyclopediaEdgeModel>(infoModel, model, OnEncyclopediaLogEdge);
		}

		void OnEncyclopediaLogSpawn(
			EncyclopediaEdgeModel edge
		)
		{
			edge.Entry.OrderWeight.Value = -1;
		}

		void OnEncyclopediaLogEdge(
			EncounterInfoModel infoModel,
			EncyclopediaEncounterLogModel model,
			EncyclopediaEdgeModel edge
		)
		{
			var entry = edge.Entry;
			entry.Title.Value = EditorGUILayoutExtensions.TextDynamic("Title", entry.Title.Value);
			entry.Header.Value = EditorGUILayoutExtensions.TextDynamic(new GUIContent("Header", "The section header, leave blank to indicate this is the introduction."), entry.Header.Value);
			entry.Body.Value = EditorGUILayoutExtensions.TextDynamic("Body", entry.Body.Value);
			entry.Priority.Value = EditorGUILayout.IntField(new GUIContent("Priority", "Higher priority sections will replace lower priority sections with the same header."), entry.Priority.Value);
			entry.OrderWeight.Value = EditorGUILayout.IntField(new GUIContent("Order Weight", "The order of this section in the article, lower weights appear first."), entry.OrderWeight.Value);
		}
		#endregion

		#region Edged Logs
		void OnEdgedLog<L, E>(
			EncounterInfoModel infoModel,
			L model,
			Action<EncounterInfoModel, L, E> edgeEditor
		)
			where L : IEdgedEncounterLogModel<E>
			where E : class, IEdgeModel
		{
			var deleted = string.Empty;
			var isAlternate = false;

			E indexSwap0 = null;
			E indexSwap1 = null;

			var isMoving = Event.current.control;

			var sorted = model.Edges.OrderBy(l => l.EdgeIndex).ToList();
			var sortedCount = sorted.Count;
			E last = null;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					for (var i = 0; i < sortedCount; i++)
					{
						var current = sorted[i];
						var next = (i + 1 < sortedCount) ? sorted[i + 1] : null;
						int currMoveDelta;

						isAlternate = !isAlternate;

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{

							if (OnEdgedLogEdgeHeader(current.EdgeName, i, sortedCount, isMoving, out currMoveDelta)) deleted = current.EdgeId;

							if (currMoveDelta != 0)
							{
								indexSwap0 = current;
								indexSwap1 = currMoveDelta == 1 ? next : last;
							}

							edgeEditor(infoModel, model, current);

							last = current;
						}
						EditorGUILayoutExtensions.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (model.IsLinear) OnLinearLog(infoModel, model as LinearEncounterLogModel);

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Edges = model.Edges.Where(e => e.EdgeId != deleted).ToArray();
			}

			if (indexSwap0 != null && indexSwap1 != null)
			{
				var swap0 = indexSwap0.EdgeIndex;
				var swap1 = indexSwap1.EdgeIndex;

				indexSwap0.EdgeIndex = swap1;
				indexSwap1.EdgeIndex = swap0;
			}
		}

		void OnEdgedLogSpawn<E>(
			IEdgedEncounterLogModel<E> model,
			Action<E> initialize = null
		)
			where E : class, IEdgeModel, new()
		{
			var index = 0;
			if (model.Edges.Any())
			{
				index = model.Edges.OrderBy(e => e.EdgeIndex).Last().EdgeIndex + 1;
			}
			var result = new E();
			result.EdgeId = Guid.NewGuid().ToString();
			result.EdgeIndex = index;
			if (initialize != null) initialize(result);
			model.Edges = model.Edges.Append(result).ToArray();
		}

		bool OnEdgedLogEdgeHeader(
			string label,
			int count,
			int maxCount,
			bool isMoving,
			out int indexDelta
		)
		{
			var deleted = false;
			indexDelta = 0;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("#" + (count + 1) + " | "+label, EditorStyles.boldLabel);
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
				EditorGUILayoutExtensions.PushEnabled(!isMoving);
				deleted = EditorGUILayoutExtensions.XButton();
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();
			return deleted;
		}
		#endregion

		void OnLinearLog(
			EncounterInfoModel infoModel,
			LinearEncounterLogModel model
		)
		{
			EditorGUILayoutEncounter.LogPopup(
				"Next Log: ",
				model.NextLogId.Value,
				infoModel,
				model,
				existingSelection => model.NextLogId.Value = existingSelection,
				newSelection => model.NextLogId.Value = AppendNewLog(newSelection, infoModel),
				EncounterLogBlankHandling.SpecifiedByModel,
				"- Select Target Log -"
			);
		}

		void OnLogEnd(EncounterInfoModel infoModel, EncounterLogModel model)
		{
			GUILayout.EndVertical();
			GUILayout.Space(2f);
			EditorGUILayoutExtensions.EndVertical();

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				EditorGUILayoutExtensions.PushColor(Color.cyan.NewH(0.55f).NewS(0.7f));
				{
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
					GUILayout.Button(GUIContent.none, EditorStyles.radioButton);
				}
				EditorGUILayoutExtensions.PopColor();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
	}
}
 
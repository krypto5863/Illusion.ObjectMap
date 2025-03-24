using ExtensibleSaveFormat;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using MessagePack;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ObjectMap
{
	internal class ObjectMapController : SceneCustomFunctionController
	{
		protected override void OnSceneLoad(SceneOperationKind operation,
			ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
		{
#if DEBUG
			ObjectMap.Logger.LogDebug($"Loading scene data...");
#endif
			var data = GetExtendedData();

			if (data == null)
			{
				return;
			}

			if (data.data.TryGetValue("ObjectStates", out var states))
			{
				var objectStates = MessagePackSerializer.Deserialize<Dictionary<string, bool>>((byte[])states);

				foreach (var objectState in objectStates)
				{
					if (TryGetObjectInfo(objectState.Key, out var currentObjectInfo) == false)
					{
						continue;
					}

					currentObjectInfo.treeNodeObject.visible = objectState.Value;
				}
			}

			if (data.data.TryGetValue("ObjectPositions", out var positions))
			{
				var objectPositions = MessagePackSerializer.Deserialize<List<ObjectPosition>>((byte[])positions);
				foreach (var objectPosition in objectPositions)
				{
					if (TryGetObjectInfo(objectPosition.Id, out var currentObjectInfo) == false)
					{
						continue;
					}

					var changeAmount = currentObjectInfo.objectInfo.changeAmount;
					changeAmount.pos = objectPosition.Transform[0];
					changeAmount.rot = objectPosition.Transform[1];
					changeAmount.scale = objectPosition.Transform[2];

					changeAmount.OnChange();
				}
			}

			if (data.data.TryGetValue("FolderNames", out var folderNamesData))
			{
				var folderNames = MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])folderNamesData);
				foreach (var folderName in folderNames)
				{
					if (TryGetObjectInfo(folderName.Key, out var currentObjectInfo) == false)
					{
						continue;
					}

					if (currentObjectInfo is OCIFolder folderInfo)
					{
						folderInfo.name = folderName.Value;
					}
				}
			}

			if (data.data.TryGetValue("LightChanges", out var lightChangesData))
			{
				var lightChanges = MessagePackSerializer.Deserialize<List<ObjectLight>>((byte[])lightChangesData);
				foreach (var lightChange in lightChanges)
				{
					if (TryGetObjectInfo(lightChange.Id, out var currentObjectInfo) == false)
					{
						continue;
					}

					if (currentObjectInfo is OCILight light)
					{
						ColorUtility.TryParseHtmlString(lightChange.Color, out var color);
						light.SetColor(color);
						light.SetIntensity(lightChange.Intensity);
						light.SetRange(lightChange.Range);
						light.SetSpotAngle(lightChange.SpotAngle);
						light.SetShadow(lightChange.Shadow);
						light.SetEnable(lightChange.Enable, true);
						light.treeNodeObject.SetVisible(lightChange.Enable);
					}
				}
			}
#if DEBUG
			ObjectMap.Logger.LogDebug($"Scene load complete.");
#endif
		}

		private static bool TryGetObjectInfo(string objectFullPath, out ObjectCtrlInfo currentObjectInfo)
		{
			var gameObject = GameObjectPathUtility.FindByPathWithSiblingIndex(objectFullPath);

			if (gameObject == null)
			{
				currentObjectInfo = null;
				return false;
			}

			return ObjectMap.MapObjects.TryGetValue(gameObject, out currentObjectInfo);
		}

		protected override void OnSceneSave()
		{
			var objectStates = new Dictionary<string, bool>();
			var objectPositions = new List<ObjectPosition>();
			var objectFolderName = new Dictionary<string, string>();
			var objectLight = new List<ObjectLight>();

			foreach (var obj in ObjectMap.MapObjects)
			{
				if (ObjectMap.OriginalProperties.TryGetValue(obj.Key.transform, out var originalValues) == false)
				{
					continue;
				}

				if (originalValues.Visible != obj.Value.treeNodeObject.visible)
				{
					objectStates[obj.Key.GetPathWithSiblingIndex()] = obj.Value.treeNodeObject.visible;
				}

				var ogTransforms = originalValues.OriginalTransform;
				var changeAmount = obj.Value.objectInfo.changeAmount;

				if (Vector3.Distance(ogTransforms[0], changeAmount.pos) > 0.001f || Vector3.Distance(ogTransforms[1], changeAmount.rot) > 0.001f || Vector3.Distance(ogTransforms[2], changeAmount.scale) > 0.001f)
				{
					objectPositions.Add(new ObjectPosition
					{
						Id = obj.Key.GetPathWithSiblingIndex(),
						Transform = new[] { changeAmount.pos, changeAmount.rot, changeAmount.scale }
					});
				}
				if (obj.Value.objectInfo is OIFolderInfo folderInfo && originalValues is FolderProperties folderOriginals)
				{
					if (folderInfo.name.Equals(folderOriginals.FolderName) == false)
					{
						objectFolderName[obj.Key.GetPathWithSiblingIndex()] = folderInfo.name;
					}
				}
				if (obj.Value.objectInfo is OILightInfo lightInfo && originalValues is LightProperties lightProperties)
				{
					if (lightInfo.color != lightProperties.Color || Math.Abs(lightInfo.intensity - lightProperties.Intensity) > 0.0001 || Math.Abs(lightInfo.range - lightProperties.Range) > 0.0001 || Math.Abs(lightInfo.spotAngle - lightProperties.SpotAngle) > 0.0001 || lightInfo.shadow != lightProperties.Shadow || lightInfo.enable != lightProperties.Visible)
					{
						objectLight.Add(new ObjectLight
						{
							Id = obj.Key.GetPathWithSiblingIndex(),
							Color = "#" + ColorUtility.ToHtmlStringRGBA(lightInfo.color),
							Intensity = lightInfo.intensity,
							Range = lightInfo.range,
							SpotAngle = lightInfo.spotAngle,
							Shadow = lightInfo.shadow,
							Enable = lightInfo.enable,
							DrawTarget = lightInfo.drawTarget
						});
					}
				}
			}

			SetExtendedData(new PluginData
			{
				version = 1,
				data = new Dictionary<string, object>
				{
					{
						"ObjectStates", MessagePackSerializer.Serialize(objectStates)
					},
					{
						"ObjectPositions", MessagePackSerializer.Serialize(objectPositions)
					},
					{
						"FolderNames", MessagePackSerializer.Serialize(objectFolderName)
					},
					{
						"LightChanges", MessagePackSerializer.Serialize(objectLight)
					}
				}
			});
		}
	}
}
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using Illusion.Extensions;
using KKAPI;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Core.ObjectMap
{
#if HS2
	[BepInProcess("StudioNEOV2")]
#else
	[BepInProcess("CharaStudio")]
#endif
	[BepInPlugin(Guid, PluginName, Version)]
	[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
	[BepInDependency(ExtendedSave.GUID, ExtendedSave.Version)]
	public class ObjectMap : BaseUnityPlugin
	{
		public const string PluginName = "ObjectMap";
		public const string Guid = "org.krypto5863.ObjectMap";
		public const string Version = "1.1.6";

		public const string IconBase64 = "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";

		internal new static ManualLogSource Logger;

		private static ConfigEntry<int> _objectLimit;

		internal static readonly Dictionary<GameObject, ObjectCtrlInfo> MapObjects = new Dictionary<GameObject, ObjectCtrlInfo>();
		internal static readonly Dictionary<Transform, IObjectProperties> OriginalProperties = new Dictionary<Transform, IObjectProperties>();

		private void Awake()
		{
			Logger = base.Logger;

			_objectLimit = Config.Bind("General", "Object Limit", 3000, "Maps with more objects than this number will be ignored in order to preserve performance.");

			StudioSaveLoadApi.RegisterExtraBehaviour<ObjectMapController>(Guid);
			StudioAPI.StudioLoadedChanged += (sender, args) =>
			{
				var tex2D = new Texture2D(2, 2);
				tex2D.LoadImage(Convert.FromBase64String(IconBase64));
				KKAPI.Studio.UI.CustomToolbarButtons.AddLeftToolbarButton(tex2D, ResetTransformOfSelected);
				Studio.Studio.Instance.onChangeMap += OnInstanceChangeMap;
			};
		}

		internal static void ResetTransformOfSelected()
		{
			if (Singleton<Studio.Studio>.Instance.treeNodeCtrl.selectNodes.Length <= 0)
			{
				return;
			}

			var selectNodes = Singleton<Studio.Studio>.Instance.treeNodeCtrl.selectNodes;
			foreach (var treeNodeObject in selectNodes)
			{
				if (Singleton<Studio.Studio>.Instance.dicInfo.TryGetValue(treeNodeObject, out var objectCtrlInfo) == false)
				{
					return;
				}

				var target = objectCtrlInfo.guideObject.transformTarget;

				if (target == null)
				{
					return;
				}

				if (OriginalProperties.TryGetValue(target, out var originalProperties) == false)
				{
					return;
				}

				var changeAmount = objectCtrlInfo.guideObject.changeAmount;
				changeAmount.pos = originalProperties.OriginalTransform[0];
				changeAmount.rot = originalProperties.OriginalTransform[1];
				changeAmount.scale = originalProperties.OriginalTransform[2];
				changeAmount.OnChange();
			}
		}

		private void OnInstanceChangeMap()
		{
			StartCoroutine(CoroutineCaptureMapChange());
		}

		private IEnumerator CoroutineCaptureMapChange()
		{
#if DEBUG
			var watch = Stopwatch.StartNew();
#endif

			foreach (var mapObj in MapObjects)
			{
				mapObj.Value.treeNodeObject.enableDelete = true;
				Studio.Studio.instance.treeNodeCtrl.DeleteNode(mapObj.Value.treeNodeObject);
				Studio.Studio.DeleteIndex(mapObj.Value.guideObject.dicKey);
			}

			MapObjects.Clear();
			OriginalProperties.Clear();

			while (Studio.Map.Instance.isLoading)
			{
				yield return new WaitForEndOfFrame();
			}

#if HS2
			var mapRoot = Studio.Map.instance?.MapRoot;
#else
			var mapRoot = Map.instance?.mapRoot;
#endif
			if (mapRoot == null)
			{
				yield break;
			}

			var childCount = mapRoot.GetDescendantCount();

#if DEBUG
			Logger.LogDebug($"Map changed, processing {childCount} children!");
#endif

			if (childCount > _objectLimit.Value)
			{
				Logger.LogMessage($"Skipped processing map because it has more objects than the configured limit ({childCount}).");
				yield break;
			}

			GuideObjectManager.Instance.transformWorkplace.gameObject.SetActive(false);

			RecursiveCreateNode(mapRoot, null);

			Studio.Studio.instance.treeNodeCtrl.RefreshHierachy();
			yield return EnableDisabledForAFrame(mapRoot);

			yield return new WaitForEndOfFrame();

			GuideObjectManager.Instance.transformWorkplace.gameObject.SetActive(true);

#if DEBUG
			Logger.LogDebug($"Map changed, processed map objects in {watch.Elapsed}");
#endif
		}

		//This is so mod added components have a chance to do on Awake, if any...
		private IEnumerator EnableDisabledForAFrame(GameObject rootObject)
		{
			var inactiveChildren = rootObject
				.GetComponentsInChildren<Transform>(true)
				.Where(d => d.gameObject.activeSelf == false)
				.ToArray();

			foreach (var inactiveChild in inactiveChildren)
			{
				inactiveChild.gameObject.SetActive(true);
			}

			yield return null;

			foreach (var inactiveChild in inactiveChildren)
			{
				inactiveChild.gameObject.SetActive(false);
			}
		}

		private void RecursiveCreateNode(GameObject gameObj, ObjectCtrlInfo parent)
		{
			ObjectCtrlInfo newItem;

			var hash = gameObj.GetPathWithSiblingIndex().GetHashCode();
			hash = Math.Abs(hash);
			var occupied = Studio.Studio.GetCtrlInfo(hash) != null;

			if (occupied)
			{
				var newIndex = Studio.Studio.GetNewIndex();
				Logger.LogMessage($"Index {hash} for {gameObj.GetPathWithSiblingIndex()} was already occupied! Using {newIndex} instead!");
				hash = newIndex;
			}

			// Maintain the original position, rotation, and scale of the GameObject
			var originalPosition = gameObj.transform.localPosition;
			var originalRotation = gameObj.transform.localEulerAngles;
			var originalScale = gameObj.transform.lossyScale;

			if (gameObj.GetComponent<Renderer>() != null)
			{
				newItem = CreateItemFromGameObject(gameObj, hash);
			}
			else if (gameObj.GetComponent<Light>() != null)
			{
				newItem = CreateLightFromGameObject(gameObj, hash);
			}
			else
			{
				newItem = CreateFolderFromGameObject(gameObj, hash);
			}

			newItem.guideObject.changeAmount.pos = originalPosition;
			newItem.guideObject.changeAmount.rot = originalRotation;
			newItem.guideObject.changeAmount.scale = originalScale;
			newItem.guideObject.changeAmount.OnChange();

			MapObjects[gameObj] = newItem;

			if (parent != null)
			{
				//Studio.Studio.instance.treeNodeCtrl.SetParent(newItem.treeNodeObject, parent.treeNodeObject);
				newItem.treeNodeObject.SetParent(parent.treeNodeObject);
			}
			else
			{
				var nodeCtrl = Studio.Studio.Instance.treeNodeCtrl;
				nodeCtrl.m_TreeNodeObject.Remove(newItem.treeNodeObject);
				nodeCtrl.m_TreeNodeObject.Insert(0, newItem.treeNodeObject);
			}

			foreach (var child in gameObj.Children())
			{
				RecursiveCreateNode(child, newItem);
			}

			newItem.treeNodeObject.enableDelete = false;
			newItem.treeNodeObject.enableAddChild = false;
			newItem.treeNodeObject.enableChangeParent = false;
			newItem.treeNodeObject.enableCopy = false;
		}

		private static OCIItem CreateItemFromGameObject(GameObject gameObj, int hash)
		{
			var renderer = gameObj.GetComponent<Renderer>();
			OriginalProperties[gameObj.transform] = new ItemProperties
			{
				FullPathToObject = gameObj.GetPathWithSiblingIndex(),
				Visible = renderer.enabled,
				OriginalTransform = new[] { gameObj.transform.localPosition, gameObj.transform.localEulerAngles, gameObj.transform.lossyScale },
			};

#if KKS
			var no = 0;
#else
			var no = 399;
#endif

			var newItem = AddObjectItem.Load(new OIItemInfo(0, 0, no, hash), null, null, false, 0);
			Destroy(newItem.objectItem);
			newItem.objectItem = gameObj;
			newItem.childRoot = gameObj.transform;
			newItem.arrayRender = (from v in gameObj.GetComponentsInChildren<Renderer>()
								   select v).ToArray();

			newItem.seComponent = null;

			var componentsInChildren = gameObj.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren.IsNullOrEmpty() == false)
			{
				newItem.arrayParticle = componentsInChildren.Where(v => v.isPlaying).ToArray();
			}
			else
			{
				newItem.arrayParticle = new ParticleSystem[0];
			}

			newItem.itemFKCtrl = null;
			newItem.listBones.Clear();

			newItem.guideObject.transformTarget = gameObj.transform;
			newItem.treeNodeObject.textName = gameObj.name;

			newItem.dynamicBones = new DynamicBone[0];

			/*
			var newItem = AddExistingGameObject.LoadAsItem(gameObj, new OIItemInfo(-1, -1, -1, hash)
			{
				visible = gameObj.activeSelf
			}, null, null, false, false);
			*/
			return newItem;
		}

		private static OCILight CreateLightFromGameObject(GameObject gameObj, int hash)
		{
			var light = gameObj.GetComponent<Light>();
			var existingColor = light.color;
			var existingIntensity = light.intensity;
			var existingShading = light.shadows != LightShadows.None;
			var existingRange = light.range;
			var existingAngle = light.spotAngle;

			// Check which layers are included in the culling mask and set lightTarget accordingly.
			var cullingMask = light.cullingMask;

			var lightProperties = new LightProperties
			{
				FullPathToObject = gameObj.GetPathWithSiblingIndex(),
				Visible = light.enabled,
				OriginalTransform = new[]
				{
					gameObj.transform.localPosition, gameObj.transform.localEulerAngles,
					gameObj.transform.lossyScale
				},
				Color = light.color,
				Intensity = light.intensity,
				Range = light.range,
				SpotAngle = light.spotAngle,
				Shadow = light.shadows != LightShadows.None
			};

			var newLight = AddObjectLight.Load(new OILightInfo(0, hash), null, null, false, 0);

			Destroy(newLight.objectLight);

			newLight.objectLight = gameObj;
			newLight.guideObject.transformTarget = gameObj.GetComponent<Transform>();

			newLight.SetColor(existingColor);
			newLight.SetIntensity(existingIntensity);
			newLight.SetShadow(existingShading);
			newLight.SetRange(existingRange);
			newLight.SetSpotAngle(existingAngle);
			newLight.SetEnable(light.enabled);

			newLight.treeNodeObject.textName = gameObj.name;
			newLight.treeNodeObject.enableAddChild = true;

			var isMapLayerActive = (cullingMask & LayerMask.GetMask("Map")) != 0;
			var isCharaLayerActive = (cullingMask & LayerMask.GetMask("Chara")) != 0;

			if (isMapLayerActive && isCharaLayerActive)
			{
				newLight.lightTarget = Studio.Info.LightLoadInfo.Target.All;
			}
			else if (isMapLayerActive)
			{
				newLight.lightTarget = Studio.Info.LightLoadInfo.Target.Map;
			}
			else if (isCharaLayerActive)
			{
				newLight.lightTarget = Studio.Info.LightLoadInfo.Target.Chara;
			}

			newLight.lightColor = null;

			OriginalProperties[gameObj.transform] = lightProperties;
			return newLight;
		}
		private static OCIFolder CreateFolderFromGameObject(GameObject gameObj, int hash)
		{
			OriginalProperties[gameObj.transform] = new FolderProperties
			{
				FullPathToObject = gameObj.GetPathWithSiblingIndex(),
				Visible = true,
				FolderName = gameObj.name,
				OriginalTransform = new[] { gameObj.transform.localPosition, gameObj.transform.localEulerAngles, gameObj.transform.lossyScale },
			};
			//newItem = AddExistingGameObject.LoadAsFolder(gameObj, new OIFolderInfo(hash) { name = gameObj.name }, null, null, false, false);
			var newFolder = AddObjectFolder.Load(new OIFolderInfo(hash), null, null, false, 0);

			newFolder.guideObject.scaleSelect = 0.1f;
			newFolder.guideObject.scaleRot = 0.05f;
			newFolder.guideObject.enableScale = true;

			Destroy(newFolder.objectItem);
			newFolder.objectItem = gameObj;
			newFolder.childRoot = gameObj.transform;
			newFolder.guideObject.transformTarget = gameObj.transform;
			newFolder.name = gameObj.name;

			return newFolder;
		}
	}
}
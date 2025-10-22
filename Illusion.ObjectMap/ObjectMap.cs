using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const string Version = "1.2.1";

        public const string IconBase64 = "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";

        internal new static ManualLogSource Logger;

        private static ConfigEntry<int> _objectLimit;
        internal static ConfigEntry<bool> LoadLights;

        internal static readonly Dictionary<GameObject, ObjectCtrlInfo> MapObjects = new Dictionary<GameObject, ObjectCtrlInfo>();
        internal static readonly Dictionary<Transform, IObjectProperties> OriginalProperties = new Dictionary<Transform, IObjectProperties>();

        private void Awake()
        {
            Logger = base.Logger;

            _objectLimit = Config.Bind("General", "Object Limit", 3000, "Maps with more objects than this number will be ignored in order to preserve performance.");

            LoadLights = Config.Bind("General", "Manage Lights", true, "In some cases this can conflict with other light managers such as GraphicsMod and should be turned off.");

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

            CleanOldDataAndScene();

            while (Studio.Map.Instance.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }

            //yield return new WaitUntil(() => Studio.Map.Instance.isLoading == false);
            //yield return new WaitForEndOfFrame();

            if (TryGetMapRootObjects(out var mapRootObjects) == false)
            {
                yield break;
            }

            if (IsObjectAboveLimit(mapRootObjects))
            {
                yield break;
            }

            yield return SetupObjectsInWorkplace(mapRootObjects);

#if DEBUG
            Logger.LogDebug($"Map changed, processed map objects in {watch.Elapsed}");
#endif
        }

        private IEnumerator SetupObjectsInWorkplace(GameObject[] mapRootObjects)
        {
            GuideObjectManager.Instance.transformWorkplace.gameObject.SetActive(false);

            foreach (var mapRootObject in mapRootObjects)
            {
                StudioObjectFactory.SetupStudioObjectRecursively(mapRootObject, null);

                Studio.Studio.instance.treeNodeCtrl.RefreshHierachy();
                yield return ToggleActiveForOneFrame(mapRootObject);

                yield return new WaitForEndOfFrame();
            }

            GuideObjectManager.Instance.transformWorkplace.gameObject.SetActive(true);
        }

        private static bool IsObjectAboveLimit(GameObject[] mapRootObjects)
        {
            var childCount = mapRootObjects.Sum(m => m.GetDescendantCount());
#if DEBUG
            Logger.LogDebug($"Map changed, processing {childCount} children!");
#endif

            if (childCount > _objectLimit.Value)
            {
                Logger.LogMessage($"Skipped processing map because it has more objects than the configured limit ({childCount} / {_objectLimit.Value}).");
                return true;
            }

            return false;
        }

        private bool TryGetMapRootObjects(out GameObject[] mapRootObjects)
        {
            if (!TryGetMapRoot(out var mapRoot))
            {
                mapRootObjects = null;
                return false;
            }

            mapRootObjects = mapRoot.scene.GetRootGameObjects();
            return mapRootObjects.Any();
        }

        private static bool TryGetMapRoot(out GameObject mapRoot)
        {
#if HS2
            mapRoot = Studio.Map.Instance.MapRoot;
#else
            mapRoot = Map.Instance.mapRoot;
#endif

            return mapRoot != null;
        }

        private static void CleanOldDataAndScene()
        {
            foreach (var mapObj in MapObjects)
            {
                mapObj.Value.treeNodeObject.enableDelete = true;
                Studio.Studio.instance.treeNodeCtrl.DeleteNode(mapObj.Value.treeNodeObject);
                Studio.Studio.DeleteIndex(mapObj.Value.guideObject.dicKey);
            }

            MapObjects.Clear();
            OriginalProperties.Clear();
        }

        //This is so mod added components have a chance to do on Awake, if any...
        private IEnumerator ToggleActiveForOneFrame(GameObject rootObject)
        {
            var inactiveChildren =
                rootObject
                .GetComponentsInChildren<Transform>()
                .Select(d => d.gameObject)
                .Where(gameObj => gameObj.activeSelf == false)
                .ToArray();

            foreach (var inactiveChild in inactiveChildren)
            {
                if (inactiveChild != null)
                    inactiveChild.SetActive(true);
            }

            yield return null;

            foreach (var inactiveChild in inactiveChildren)
            {
                if (inactiveChild != null)
                    inactiveChild.SetActive(false);
            }
        }
    }
}
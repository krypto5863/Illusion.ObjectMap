using System;
using System.Linq;
using Illusion.Extensions;
using Studio;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.ObjectMap
{
    public static class StudioObjectFactory
    {
        public static void SetupStudioObjectRecursively(GameObject gameObj, ObjectCtrlInfo parent)
        {
            var objectIndex = GetObjectIndex(gameObj);

            // Maintain the original position, rotation, and scale of the GameObject
            var originalPosition = gameObj.transform.localPosition;
            var originalRotation = gameObj.transform.localEulerAngles;
            var originalScale = gameObj.transform.lossyScale;

            if (CreateBasedOnComponents(gameObj, objectIndex, out var newItem) == false)
            {
                return;
            }

            newItem.guideObject.changeAmount.pos = originalPosition;
            newItem.guideObject.changeAmount.rot = originalRotation;
            newItem.guideObject.changeAmount.scale = originalScale;
            newItem.guideObject.changeAmount.OnChange();

            ObjectMap.MapObjects[gameObj] = newItem;

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
                SetupStudioObjectRecursively(child, newItem);
            }

            newItem.treeNodeObject.enableDelete = false;
            newItem.treeNodeObject.enableAddChild = false;
            newItem.treeNodeObject.enableChangeParent = false;
            newItem.treeNodeObject.enableCopy = false;
        }

        private static bool CreateBasedOnComponents(GameObject gameObj, int objectIndex, out ObjectCtrlInfo newItem)
        {
            if (gameObj.GetComponent<Renderer>() != null)
            {
                newItem = CreateItemFromGameObject(gameObj, objectIndex);
            }
            else if (gameObj.GetComponent<Light>() != null)
            {
                if (ObjectMap.LoadLights.Value == false)
                {
                    newItem = null;
                    return false;
                }

                newItem = CreateLightFromGameObject(gameObj, objectIndex);
            }
            else
            {
                newItem = CreateFolderFromGameObject(gameObj, objectIndex);
            }

            return true;
        }

        private static int GetObjectIndex(GameObject gameObj)
        {
            var hash = gameObj.GetPathWithSiblingIndex().GetHashCode();
            hash = Math.Abs(hash);

            var occupied = Studio.Studio.GetCtrlInfo(hash) != null;
            if (occupied)
            {
                var newIndex = Studio.Studio.GetNewIndex();
                ObjectMap.Logger.LogMessage($"Index {hash} for {gameObj.GetPathWithSiblingIndex()} was already occupied! Using {newIndex} instead!");
                hash = newIndex;
            }

            return hash;
        }

        private static OCIItem CreateItemFromGameObject(GameObject gameObj, int hash)
        {
            var renderer = gameObj.GetComponent<Renderer>();
            ObjectMap.OriginalProperties[gameObj.transform] = new ItemProperties
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
            Object.Destroy(newItem.objectItem);
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

            Object.Destroy(newLight.objectLight);

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
                newLight.lightTarget = Info.LightLoadInfo.Target.All;
            }
            else if (isMapLayerActive)
            {
                newLight.lightTarget = Info.LightLoadInfo.Target.Map;
            }
            else if (isCharaLayerActive)
            {
                newLight.lightTarget = Info.LightLoadInfo.Target.Chara;
            }

            newLight.lightColor = null;

            ObjectMap.OriginalProperties[gameObj.transform] = lightProperties;
            return newLight;
        }

        private static OCIFolder CreateFolderFromGameObject(GameObject gameObj, int hash)
        {
            ObjectMap.OriginalProperties[gameObj.transform] = new FolderProperties
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

            Object.Destroy(newFolder.objectItem);
            newFolder.objectItem = gameObj;
            newFolder.childRoot = gameObj.transform;
            newFolder.guideObject.transformTarget = gameObj.transform;
            newFolder.name = gameObj.name;

            return newFolder;
        }
    }
}
using System;
using System.Linq;
using Studio;
using UnityEngine;

namespace Core.ObjectMap
{
	//This class brings me shame, my ancestors do not smile upon me. But this is what drives this whole plugin. And it does it well.
	public static class AddExistingGameObject
	{
		public static OCIItem LoadAsItem(GameObject existingGameObject, OIItemInfo itemInfo, ObjectCtrlInfo parentCtrl,
			TreeNodeObject parentNode, bool addInfo, bool placeAtTarget)
		{
			// Use the existing GameObject passed as parameter
			var gameObject = existingGameObject;

			if (gameObject == null)
			{
				Studio.Studio.DeleteIndex(itemInfo.dicKey);
				return null;
			}

			//Keep original position, rotation, and scale intact
			var originalPosition = gameObject.transform.localPosition;
			var originalRotation = gameObject.transform.localEulerAngles;
			var originalScale = gameObject.transform.lossyScale;

			var ociitem = new OCIItem();
			ociitem.objectInfo = itemInfo;
			ociitem.objectItem = gameObject;

			ociitem.arrayRender = (from v in gameObject.GetComponentsInChildren<Renderer>()
								   where v.enabled
								   select v).ToArray();

			var componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			if (!componentsInChildren.IsNullOrEmpty())
			{
				ociitem.arrayParticle = componentsInChildren.Where(v => v.isPlaying).ToArray();
			}

			var component = gameObject.GetComponent<MeshCollider>();
			if (component)
			{
				component.enabled = false;
			}

			ociitem.dynamicBones = gameObject.GetComponentsInChildren<DynamicBone>();
			var guideObject = Singleton<GuideObjectManager>.Instance.Add(gameObject.transform, itemInfo.dicKey);
			guideObject.isActive = false;
			guideObject.scaleSelect = 0.1f;
			guideObject.scaleRot = 0.05f;
			guideObject.isActiveFunc = (GuideObject.IsActiveFunc)Delegate.Combine(guideObject.isActiveFunc, new GuideObject.IsActiveFunc(ociitem.OnSelect));
			guideObject.enableScale = true;
			guideObject.SetVisibleCenter(true);
			ociitem.guideObject = guideObject;

			guideObject.changeAmount.pos = originalPosition;
			guideObject.changeAmount.scale = originalScale;
			guideObject.changeAmount.rot = originalRotation;

			ociitem.childRoot = gameObject.transform;

			ociitem.animator = gameObject.GetComponentInChildren<Animator>(true);
			if (ociitem.animator)
			{
				ociitem.animator.enabled = true;
			}

			/*
			ociitem.itemComponent = gameObject.GetComponent<ItemComponent>();
			if (ociitem.itemComponent != null)
			{
				ociitem.itemComponent.SetFlag(itemLoadInfo.color, itemLoadInfo.pattren);
				ociitem.itemComponent.SetLine();
				ociitem.itemComponent.SetEmission();

				if (_addInfo)
				{
					var array = ociitem.itemComponent.defColorMain;
					for (var i = 0; i < 3; i++)
					{
						_info.color[i] = array[i];
					}
					array = ociitem.itemComponent.defColorPattern;
					for (var j = 0; j < 3; j++)
					{
						_info.color[j + 3] = array[j];
					}
					_info.color[6] = ociitem.itemComponent.defShadow;
					for (var k = 0; k < 3; k++)
					{
						_info.pattern[k].clamp = ociitem.itemComponent.itemInfo[k].defClamp;
						_info.pattern[k].uv = ociitem.itemComponent.itemInfo[k].defUV;
						_info.pattern[k].rot = ociitem.itemComponent.itemInfo[k].defRot;
					}
					_info.color[7] = ociitem.itemComponent.defGlass;
					_info.lineColor = ociitem.itemComponent.defLineColor;
					_info.lineWidth = ociitem.itemComponent.defLineWidth;
					_info.emissionColor = ociitem.itemComponent.DefEmissionColor;
					_info.emissionPower = ociitem.itemComponent.defEmissionPower;
					_info.lightCancel = ociitem.itemComponent.defLightCancel;
				}
			}
			else
			{
				ociitem.chaAccessoryComponent = gameObject.GetComponent<ChaAccessoryComponent>();
				if (ociitem.chaAccessoryComponent && _addInfo)
				{
					_info.color[0] = ociitem.chaAccessoryComponent.defColor01;
					_info.color[1] = ociitem.chaAccessoryComponent.defColor02;
					_info.color[2] = ociitem.chaAccessoryComponent.defColor03;
					_info.color[7] = ociitem.chaAccessoryComponent.defColor04;
				}
			}
			*/
			/*
			ociitem.particleComponent = gameObject.GetComponent<ParticleComponent>();
			if (ociitem.particleComponent && _addInfo)
			{
				_info.color[0] = ociitem.particleComponent.defColor01;
			}

			//ociitem.enableEmission = itemLoadInfo.isEmission;
			//ociitem.iconComponent = gameObject.GetComponent<IconComponent>();
			//ociitem.panelComponent = gameObject.GetComponent<PanelComponent>();
			//ociitem.seComponent = gameObject.GetComponent<SEComponent>();
			*/

			if (addInfo)
			{
				Studio.Studio.AddInfo(itemInfo, ociitem);
			}
			else
			{
				Studio.Studio.AddObjectCtrlInfo(ociitem);
			}

			var parentTreeNode = parentNode != null ? parentNode : parentCtrl?.treeNodeObject;
			var newTreeNode = Studio.Studio.AddNode(gameObject.name, parentTreeNode);
			newTreeNode.treeState = itemInfo.treeState;

			newTreeNode.onVisible = (TreeNodeObject.OnVisibleFunc)Delegate.Combine(newTreeNode.onVisible, new TreeNodeObject.OnVisibleFunc(ociitem.OnVisible));
			newTreeNode.enableVisible = true;
			newTreeNode.visible = itemInfo.visible;

			guideObject.guideSelect.treeNodeObject = newTreeNode;
			ociitem.treeNodeObject = newTreeNode;

			/*
			if (!itemLoadInfo.bones.IsNullOrEmpty<string>())
			{
				ociitem.itemFKCtrl = gameObject.AddComponent<ItemFKCtrl>();
				ociitem.itemFKCtrl.InitBone(ociitem, itemLoadInfo, _addInfo);
			}
			else
			{
				ociitem.itemFKCtrl = null;
			}
			*/

			ociitem.itemFKCtrl = null;

			if (placeAtTarget)
			{
				itemInfo.changeAmount.pos = Singleton<Studio.Studio>.Instance.cameraCtrl.targetPos;
			}

			itemInfo.changeAmount.OnChange();
			Studio.Studio.AddCtrlInfo(ociitem);

			if (parentCtrl != null)
			{
				parentCtrl.OnLoadAttach(parentNode != null ? parentNode : parentCtrl.treeNodeObject, ociitem);
			}

			/*
			if (ociitem.animator)
			{
				ociitem.SetAnimePattern(_info.animePattern);
				ociitem.animator.speed = _info.animeSpeed;
				if (_info.animeNormalizedTime != 0f && ociitem.animator.layerCount != 0)
				{
					ociitem.animator.Update(1f);
					AnimatorStateInfo currentAnimatorStateInfo = ociitem.animator.GetCurrentAnimatorStateInfo(0);
					ociitem.animator.Play(currentAnimatorStateInfo.shortNameHash, 0, _info.animeNormalizedTime);
				}
			}
			*/

			ociitem.SetupPatternTex();
			ociitem.SetMainTex();
			ociitem.UpdateColor();
			ociitem.ActiveFK(ociitem.itemInfo.enableFK);
			ociitem.UpdateFKColor();
			ociitem.ActiveDynamicBone(ociitem.itemInfo.enableDynamicBone);
			return ociitem;
		}

		public static OCILight LoadAsLight(GameObject existingGameObject, OILightInfo lightInfo, ObjectCtrlInfo parentCtrl,
			TreeNodeObject parentNode, bool addInfo, bool placeAtTarget)
		{
			// Use the existing GameObject passed as parameter
			var gameObject = existingGameObject;

			if (gameObject == null)
			{
				return null; // If no GameObject is provided, exit the method
			}

			// Maintain the original position, rotation, and scale of the GameObject
			var originalPosition = gameObject.transform.localPosition;
			var originalRotation = gameObject.transform.localEulerAngles;
			var originalScale = gameObject.transform.lossyScale;

			var lightComponent = existingGameObject.GetComponent<Light>();
			var existingColor = lightComponent.color;
			var existingIntensity = lightComponent.intensity;
			var existingShading = lightComponent.shadows != LightShadows.None;
			var existingRange = lightComponent.range;
			var existingAngle = lightComponent.spotAngle;

			var ocilight = new OCILight();
			ocilight.objectInfo = lightInfo;
			ocilight.objectLight = gameObject;

			var guideObject = Singleton<GuideObjectManager>.Instance.Add(gameObject.transform, lightInfo.dicKey);
			guideObject.scaleSelect = 0.1f;
			guideObject.scaleRot = 0.05f;
			guideObject.isActive = false;
			guideObject.enableScale = false;
			guideObject.SetVisibleCenter(true);
			ocilight.guideObject = guideObject;

			guideObject.changeAmount.pos = originalPosition;
			guideObject.changeAmount.scale = originalScale;
			guideObject.changeAmount.rot = originalRotation;

			ocilight.lightColor = gameObject.GetComponent<LightColor>();
			if (ocilight.lightColor)
			{
				ocilight.lightColor.color = lightInfo.color;
			}

			ocilight.SetColor(existingColor);
			ocilight.SetIntensity(existingIntensity);
			ocilight.SetShadow(existingShading);
			ocilight.SetRange(existingRange);
			ocilight.SetSpotAngle(existingAngle);
			ocilight.SetEnable(lightComponent.enabled);

			// Check which layers are included in the culling mask and set lightTarget accordingly.
			var cullingMask = ocilight.light.cullingMask;

			var isMapLayerActive = (cullingMask & LayerMask.GetMask("Map")) != 0;
			var isCharaLayerActive = (cullingMask & LayerMask.GetMask("Chara")) != 0;

			if (isMapLayerActive && isCharaLayerActive)
			{
				ocilight.lightTarget = Info.LightLoadInfo.Target.All;
			}
			else if (isMapLayerActive)
			{
				ocilight.lightTarget = Info.LightLoadInfo.Target.Map;
			}
			else if (isCharaLayerActive)
			{
				ocilight.lightTarget = Info.LightLoadInfo.Target.Chara;
			}

			var treeNodeObject = parentNode != null ? parentNode : parentCtrl != null ? parentCtrl.treeNodeObject : null;
			var treeNodeObject2 = Studio.Studio.AddNode(existingGameObject.name, treeNodeObject);
			treeNodeObject2.visible = lightInfo.visible;
			treeNodeObject2.treeState = lightInfo.treeState;
			guideObject.guideSelect.treeNodeObject = treeNodeObject2;
			ocilight.treeNodeObject = treeNodeObject2;

			if (placeAtTarget)
			{
				lightInfo.changeAmount.pos = Singleton<Studio.Studio>.Instance.cameraCtrl.targetPos;
			}

			lightInfo.changeAmount.OnChange();
			Studio.Studio.AddCtrlInfo(ocilight);

			if (parentCtrl != null)
			{
				parentCtrl.OnLoadAttach(parentNode != null ? parentNode : parentCtrl.treeNodeObject, ocilight);
			}

			ocilight.Update();
			return ocilight;
		}

		// Loads an existing GameObject as an OCIFolder.
		public static OCIFolder LoadAsFolder(GameObject existingFolder, OIFolderInfo folderInfo, ObjectCtrlInfo parentCtrl, TreeNodeObject parentNode, bool addInfo, bool placeAtTarget)
		{
			// Use the existing GameObject passed as parameter
			var gameObject = existingFolder;

			if (gameObject == null)
			{
				return null; // If no GameObject is provided, exit the method
			}

			// Maintain the original position, rotation, and scale of the GameObject
			var originalPosition = gameObject.transform.localPosition;
			var originalRotation = gameObject.transform.localEulerAngles;
			var originalScale = gameObject.transform.lossyScale;

			var ocifolder = new OCIFolder
			{
				objectInfo = folderInfo,
				objectItem = existingFolder
			};

			// Add a guide object.
			var guideObject = Singleton<GuideObjectManager>.Instance.Add(existingFolder.transform, folderInfo.dicKey);
			guideObject.isActive = false;
			guideObject.scaleSelect = 0.1f;
			guideObject.scaleRot = 0.05f;
			guideObject.enableScale = true;
			guideObject.SetVisibleCenter(true);
			ocifolder.guideObject = guideObject;

			guideObject.changeAmount.pos = originalPosition;
			guideObject.changeAmount.scale = originalScale;
			guideObject.changeAmount.rot = originalRotation;

			// Set the child root to the folder itself.
			ocifolder.childRoot = existingFolder.transform;

			// Add the folder to Studio's control info.
			if (addInfo)
			{
				Studio.Studio.AddInfo(folderInfo, ocifolder);
			}
			else
			{
				Studio.Studio.AddObjectCtrlInfo(ocifolder);
			}

			// Handle hierarchy and tree node setup.
			var treeNodeObject = parentNode ?? parentCtrl?.treeNodeObject;
			var newNode = Studio.Studio.AddNode(folderInfo.name, treeNodeObject);
			newNode.treeState = folderInfo.treeState;
			newNode.enableVisible = folderInfo.visible;
			newNode.visible = folderInfo.visible;
			newNode.baseColor = Utility.ConvertColor(180, 150, 5);
			newNode.colorSelect = newNode.baseColor;

			// Connect the guide object to the tree node.
			guideObject.guideSelect.treeNodeObject = newNode;
			ocifolder.treeNodeObject = newNode;

			// Handle initial position if specified.
			if (placeAtTarget)
			{
				folderInfo.changeAmount.pos = Singleton<Studio.Studio>.Instance.cameraCtrl.targetPos;
			}

			// Apply positional changes.
			folderInfo.changeAmount.OnChange();

			// Register the folder with Studio.
			Studio.Studio.AddCtrlInfo(ocifolder);

			// Attach the folder to the parentCtrl, if any.
			parentCtrl?.OnLoadAttach(parentNode ?? parentCtrl.treeNodeObject, ocifolder);

			return ocifolder;
		}
	}
}
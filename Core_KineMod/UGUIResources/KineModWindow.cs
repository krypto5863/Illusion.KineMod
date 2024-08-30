using System;
using System.Collections.Generic;
using BepInEx;
using Studio;
using System.IO;
using System.Linq;
using System.Reflection;
using IllusionUtility.GetUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;
using static Studio.OIBoneInfo;

namespace Core_KineMod.UGUIResources
{
	public class BoneInfo
	{
		public string UserFriendlyName { get; set; }
		public string LimbName { get; set; }
		public bool IsBendGoal { get; set; }

		public BoneInfo(string userFriendlyName, string limbName, bool isBendGoal = false)
		{
			UserFriendlyName = userFriendlyName;
			LimbName = limbName;
			IsBendGoal = isBendGoal;
		}
	}


	internal static class KineModWindow
	{
		private static readonly Tuple<BoneGroup, string>[] IkBoneGroupNames =
		{
			new Tuple<BoneGroup, string>(BoneGroup.Body , "Waist"),
			new Tuple<BoneGroup, string>(BoneGroup.RightArm , "R. Arm"),
			new Tuple<BoneGroup, string>(BoneGroup.LeftArm , "L. Arm"),
			new Tuple<BoneGroup, string>(BoneGroup.RightLeg , "R. Leg"),
			new Tuple<BoneGroup, string>(BoneGroup.LeftLeg , "L. Leg")
		};

		private static readonly Tuple<BoneGroup, string>[] FkBoneGroupNames =
		{
			new Tuple<BoneGroup, string>(BoneGroup.Hair , "Hair"),
			new Tuple<BoneGroup, string>(BoneGroup.Neck , "Neck"),
			new Tuple<BoneGroup, string>(BoneGroup.Breast , "Chest"),
			new Tuple<BoneGroup, string>(BoneGroup.Body , "Body"),
			new Tuple<BoneGroup, string>(BoneGroup.RightHand , "R. Hand"),
			new Tuple<BoneGroup, string>(BoneGroup.LeftHand , "L. Hand"),
			new Tuple<BoneGroup, string>(BoneGroup.Skirt , "Skirt")
		};
		private static readonly Dictionary<string, BoneInfo> BonesInfo = new Dictionary<string, BoneInfo>
		{
#if HS2
			{ "f_t_shoulder_r(work)", new BoneInfo("R. Shoulder", "R. Arm") },
			{ "f_t_shoulder_l(work)", new BoneInfo("L. Shoulder", "L. Arm") },
			{ "f_t_elbo_r(work)", new BoneInfo("R. Elbow", "R. Arm", true) },
			{ "f_t_elbo_l(work)", new BoneInfo("L. Elbow", "L. Arm", true) },
			{ "f_t_arm_r(work)", new BoneInfo("R. Hand", "R. Arm") },
			{ "f_t_arm_l(work)", new BoneInfo("L. Hand", "L. Arm") },
			{ "f_t_hips(work)", new BoneInfo("Waist", "Waist") },
			{ "f_t_thigh_r(work)", new BoneInfo("R. Hips", "R. Leg") },
			{ "f_t_thigh_l(work)", new BoneInfo("L. Hips", "L. Leg") },
			{ "f_t_knee_r(work)", new BoneInfo("R. Knee", "R. Leg", true) },
			{ "f_t_knee_l(work)", new BoneInfo("L. Knee", "L. Leg", true) },
			{ "f_t_leg_r(work)", new BoneInfo("R. Foot", "R. Leg") },
			{ "f_t_leg_l(work)", new BoneInfo("L. Foot", "L. Leg") },
#else
		    { "cf_t_shoulder_r(work)", new BoneInfo("R. Shoulder", "R. Arm") },
		    { "cf_t_shoulder_l(work)", new BoneInfo("L. Shoulder", "L. Arm") },
		    { "cf_t_elbo_r(work)", new BoneInfo("R. Elbow", "R. Arm", true) },
		    { "cf_t_elbo_l(work)", new BoneInfo("L. Elbow", "L. Arm", true) },
		    { "cf_t_hand_r(work)", new BoneInfo("R. Hand", "R. Arm") },
		    { "cf_t_hand_l(work)", new BoneInfo("L. Hand", "L. Arm") },
		    { "cf_t_hips(work)", new BoneInfo("Waist", "Waist") },
		    { "cf_t_waist_r(work)", new BoneInfo("R. Hips", "R. Leg") },
		    { "cf_t_waist_l(work)", new BoneInfo("L. Hips", "L. Leg") },
		    { "cf_t_knee_r(work)", new BoneInfo("R. Knee", "R. Leg", true) },
		    { "cf_t_knee_l(work)", new BoneInfo("L. Knee", "L. Leg", true) },
		    { "cf_t_leg_r(work)", new BoneInfo("R. Foot", "R. Leg") },
		    { "cf_t_leg_l(work)", new BoneInfo("L. Foot", "L. Leg") },
#endif
		};




		private static GameObject _menuGameObject;
		private static MPCharCtrl _charCtrl;
		private static MPCharCtrl CharCtrl 
		{
			get
			{
				if (_charCtrl == null)
				{
					_charCtrl = FindObjectOfType<MPCharCtrl>();
				}

				return _charCtrl;
			}
		}

		public static bool InitGui()
		{
			if (LoadAssembly() == false)
			{
				return false;
			}

			if (LoadAssetBundle(out var uiPanel) == false)
			{
				return false;
			}

			if (SetupUi(uiPanel, out _menuGameObject) == false)
			{
				return false;
			}

			if (CreateKinematicsButton() == false)
			{
				return false;
			}

			return true;
		}

		private static bool LoadAssembly()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resource = assembly.GetManifestResourceNames()
				.FirstOrDefault(m => m.ToLower().EndsWith("KineModUILogic.dll".ToLower()));

			if (resource.IsNullOrWhiteSpace())
			{
				return false;
			}

			try
			{
				// Load the resource stream
				using (var stream = assembly.GetManifestResourceStream(resource))
				{
					if (stream != null)
					{
						// Read the stream into a byte array
						var assemblyData = new byte[stream.Length];
						stream.Read(assemblyData, 0, (int)stream.Length);
						Assembly.Load(assemblyData);
					}
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		private static bool LoadAssetBundle(out GameObject uiPanelGameObject)
		{
			uiPanelGameObject = null;


			try
			{
				AssetBundle assetBundle = null;

				if (File.Exists(Path.Combine(Paths.GameRootPath, "kinemodui")))
				{ 
					assetBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.GameRootPath, "kinemodui"));
				}
				else
				{

					var assembly = Assembly.GetExecutingAssembly();
					var resource = assembly.GetManifestResourceNames()
						.FirstOrDefault(m => m.ToLower().EndsWith("kinemodui".ToLower()));

					if (resource.IsNullOrWhiteSpace())
					{
						return false;
					}

					try
					{
						// Load the resource stream
						using (var stream = assembly.GetManifestResourceStream(resource))
						{
							if (stream != null)
							{
								// Read the stream into a byte array
								var resourceFile = new byte[stream.Length];
								stream.Read(resourceFile, 0, (int)stream.Length);
								assetBundle = AssetBundle.LoadFromMemory(resourceFile);
							}
						}
					}
					catch
					{
						return false;
					}
				}
				try
				{
					var allAssets = assetBundle.LoadAllAssets();

					const string objectName = "KineModCanvas";
					var uiCanvas =
						allAssets.FirstOrDefault(m => m.name.Equals(objectName, StringComparison.OrdinalIgnoreCase)) as
							GameObject;
					if (uiCanvas != null)
					{
						uiPanelGameObject = uiCanvas;
						return true;
					}
					else
					{
						return false;
					}

				}
				finally
				{
					assetBundle?.Unload(false);
				}
			}
			catch
			{
				return false;
			}
		}

		private static bool SetupUi(GameObject uiPanel, out GameObject modPanel)
		{
			modPanel = null;

			var tempFullObject = Instantiate(uiPanel);
			var nextUiPanel = tempFullObject.GetComponentInChildren<CanvasRenderer>();
			modPanel = Instantiate(nextUiPanel.gameObject);
			DestroyImmediate(tempFullObject);

			modPanel.SetActive(false);

			var kineMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic");
			modPanel.transform.SetParent(kineMenu.transform);

			SetupMainPage(modPanel);

			SetupEffectorsPage(modPanel);

			return true;
		}

		private static void SetupEffectorsPage(GameObject modPanel)
		{
			var chainSectionTemplate = modPanel.transform.FindLoop("ChainSection");

			/*
			foreach (var limb in BonesInfo)
			{
				var newChainSection = Instantiate(chainSectionTemplate, chainSectionTemplate.transform.parent);
				newChainSection.name = limb.Value.UserFriendlyName;
				var sectionName = newChainSection.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
				sectionName.text = limb.Value.UserFriendlyName;

				var newSliderCluster = newChainSection.transform.FindLoop("SliderCluster");
				newSliderCluster.name = limb.Value.UserFriendlyName;
				var nameText = newSliderCluster.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
				nameText.text = limb.Value.UserFriendlyName;

				var posSliderUnit = newSliderCluster.transform.FindLoop("PosSlider");
				var posSlider = posSliderUnit.GetComponentInChildren<Slider>();

				var rotSliderUnit = newSliderCluster.transform.FindLoop("RotSlider");
				var rotSlider = rotSliderUnit.GetComponentInChildren<Slider>();

				if (limb.Value.IsBendGoal == false)
				{
					posSlider.onValueChanged.AddListener((value) =>
					{
						var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
						effector.positionWeight = value;
					});
					ValueMonitor.AddMonitor(posSlider.gameObject, () =>
					{
						var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
						if (Math.Abs(effector.positionWeight - posSlider.value) > 0.01)
						{
							posSlider.value = effector.positionWeight;
						}
					});

					rotSlider.onValueChanged.AddListener((value) =>
					{
						var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
						effector.rotationWeight = value;
					});
					ValueMonitor.AddMonitor(rotSlider.gameObject, () =>
					{
						var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
						if (effector.isEndEffector == false)
						{
							rotSliderUnit.gameObject.SetActive(false);
						}

						if (Math.Abs(effector.rotationWeight - rotSlider.value) > 0.01)
						{
							rotSlider.value = effector.rotationWeight;
						}
					});
				}
				else
				{
					posSlider.onValueChanged.AddListener((value) =>
					{
						var effector = CharCtrl.ociChar.GetBendGoalByTargetName(limb.Key);
						effector.weight = value;
					});
					ValueMonitor.AddMonitor(posSlider.gameObject, () =>
					{
						var effector = CharCtrl.ociChar.GetBendGoalByTargetName(limb.Key);
						if (Math.Abs(effector.weight - posSlider.value) > 0.01)
						{
							posSlider.value = effector.weight;
						}
					});

					rotSliderUnit.gameObject.SetActive(false);
				}
			}*/
			
			var groupings = BonesInfo.GroupBy(r => r.Value.LimbName);
			foreach (var grouping in groupings)
			{
				var newChainSection = Instantiate(chainSectionTemplate, chainSectionTemplate.transform.parent);
				newChainSection.name = grouping.Key;
				var sectionName = newChainSection.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
				sectionName.text = grouping.Key;

				var sliderCluster = newChainSection.transform.FindLoop("SliderCluster");
				foreach (var limb in grouping)
				{
					var newSliderCluster = Instantiate(sliderCluster, sliderCluster.transform.parent);
					newSliderCluster.name = limb.Value.UserFriendlyName;
					var nameText = newSliderCluster.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
					nameText.text = limb.Value.UserFriendlyName;

					var posSliderUnit = newSliderCluster.transform.FindLoop("PosSlider");
					var posSlider = posSliderUnit.GetComponentInChildren<Slider>();

					var posValueText = posSlider.transform.FindLoop("Value").GetComponent<TextMeshProUGUI>();
					posSlider.onValueChanged.AddListener((value) =>
					{
						posValueText.text = value.ToString("0.00");
					});

					var rotSliderUnit = newSliderCluster.transform.FindLoop("RotSlider");
					var rotSlider = rotSliderUnit.GetComponentInChildren<Slider>();

					var rotValueText = rotSlider.transform.FindLoop("Value").GetComponent<TextMeshProUGUI>();
					rotSlider.onValueChanged.AddListener((value) =>
					{
						rotValueText.text = value.ToString("0.00");
					});

					if (limb.Value.IsBendGoal == false)
					{
						posSlider.onValueChanged.AddListener((value) =>
						{
							var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
							effector.positionWeight = value;
						});
						ValueMonitor.AddMonitor(posSlider.gameObject, () =>
						{
							var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
							if (Math.Abs(effector.positionWeight - posSlider.value) > 0.01)
							{
								posSlider.value = effector.positionWeight;
							}
						});

						rotSlider.onValueChanged.AddListener((value) =>
						{
							var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
							effector.rotationWeight = value;
						});
						ValueMonitor.AddMonitor(rotSlider.gameObject, () =>
						{
							var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb.Key);
							if (effector.isEndEffector == false)
							{
								rotSliderUnit.gameObject.SetActive(false);
							}

							if (Math.Abs(effector.rotationWeight - rotSlider.value) > 0.01)
							{
								rotSlider.value = effector.rotationWeight;
							}
						});
					}
					else
					{
						posSlider.onValueChanged.AddListener((value) =>
						{
							var effector = CharCtrl.ociChar.GetBendGoalByTargetName(limb.Key);
							effector.weight = value;
						});
						ValueMonitor.AddMonitor(posSlider.gameObject, () =>
						{
							var effector = CharCtrl.ociChar.GetBendGoalByTargetName(limb.Key);
							if (Math.Abs(effector.weight - posSlider.value) > 0.01)
							{
								posSlider.value = effector.weight;
							}
						});

						rotSliderUnit.gameObject.SetActive(false);
					}
				}
				sliderCluster.gameObject.SetActive(false);
			}

			chainSectionTemplate.gameObject.SetActive(false);
		}

		private static void SetupMainPage(GameObject modPanel)
		{
			//var panelPath = modPanel.transform.GetPath();
			var templateSection = modPanel.transform.FindLoop("TemplateSection");
			templateSection.gameObject.SetActive(false);

			KineMod.PluginLogger.LogDebug("Setting up refer to active button...");

			var systemActive = modPanel.transform.FindLoop("SystemActive").GetComponentInChildren<Toggle>();
			systemActive.onValueChanged.AddListener((value) =>
			{
				var controller = CharCtrl.ociChar.charInfo.GetComponent<KineModController>();
				controller.SystemActive = value;
				if (value)
				{
					KineMod.EnableFkIk(CharCtrl.ociChar);
				}
				else
				{
					KineMod.DisableFkIk(CharCtrl.ociChar);
				}
			});
			ValueMonitor.AddMonitor(systemActive.gameObject, () =>
			{
				var controller = CharCtrl.ociChar.charInfo.GetComponent<KineModController>();
				if (controller.SystemActive != systemActive.isOn)
				{
					systemActive.isOn = controller.SystemActive;
				}
			});

			KineMod.PluginLogger.LogDebug("Setting up refer to animation button...");

			var referAnimationButton = modPanel.transform.FindLoop("ReferToAnim").GetComponentInChildren<Button>();
			referAnimationButton.onClick.AddListener(() =>
			{
				CharCtrl.SetCopyBoneIK((BoneGroup)31);
				CharCtrl.SetCopyBoneFK((BoneGroup)353);
			});

			KineMod.PluginLogger.LogDebug("Setting up FK section...");
			#region FK

			{
				var fkSection = Instantiate(templateSection, templateSection.transform.parent);
				fkSection.name = "FK";
				var fkText = fkSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
				fkText.text = "FK";

				var templateToggle = fkSection.transform.FindLoop("TemplateToggle");

				foreach (var boneGroup in FkBoneGroupNames)
				{
					var newToggle = Instantiate(templateToggle, templateToggle.transform.parent);
					newToggle.name = boneGroup.Item2;

					var toggle = newToggle.GetComponentInChildren<Toggle>();
					toggle.onValueChanged.AddListener((value) =>
					{
						_charCtrl.fkInfo.OnChangeValueIndividual(boneGroup.Item1, value);
					});

					ValueMonitor.AddMonitor(newToggle.gameObject, () =>
					{
						var state = CharCtrl.ociChar.IsFkBoneGroupActive(boneGroup.Item1);
						if (state != toggle.isOn)
						{
							toggle.isOn = state;
						}
					});

					var resetButton = newToggle.GetComponentInChildren<Button>();
					resetButton.onClick.AddListener(() => { CharCtrl.SetCopyBoneFK(boneGroup.Item1); });

					var label = newToggle.transform.FindLoop("Label").GetComponent<TextMeshProUGUI>();
					label.text = boneGroup.Item2;
				}

				var massToggles = fkSection.transform.FindLoop("MassToggles");
				var onButton = massToggles.transform.FindLoop("On").GetComponent<Button>();
				var offButton = massToggles.transform.FindLoop("Off").GetComponent<Button>();

				onButton.onClick.AddListener(() =>
				{
					var toggles = fkSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = true;
					}
				});

				offButton.onClick.AddListener(() =>
				{
					var toggles = fkSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = false;
					}
				});

				templateToggle.gameObject.SetActive(false);

				fkSection.gameObject.SetActive(true);
			}

			#endregion
			KineMod.PluginLogger.LogDebug("Setting up IK section...");
			#region IK
			{
				var ikSection = Instantiate(templateSection, templateSection.transform.parent);
				ikSection.name = "IK";
				var ikText = ikSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
				ikText.text = "IK";

				var templateToggle = ikSection.transform.FindLoop("TemplateToggle");

				foreach (var boneGroup in IkBoneGroupNames)
				{
					var newToggle = Instantiate(templateToggle, templateToggle.transform.parent);
					newToggle.name = boneGroup.Item2;

					var toggle = newToggle.GetComponentInChildren<Toggle>();
					toggle.onValueChanged.AddListener((value) =>
					{
						_charCtrl.ikInfo.OnChangeValueIndividual(boneGroup.Item1, value);
					});

					ValueMonitor.AddMonitor(newToggle.gameObject, () =>
					{
						var state = CharCtrl.ociChar.IsIkBoneGroupActive(boneGroup.Item1);
						if (state != toggle.isOn)
						{
							toggle.isOn = state;
						}
					});

					var resetButton = newToggle.GetComponentInChildren<Button>();
					resetButton.onClick.AddListener(() => { CharCtrl.SetCopyBoneIK(boneGroup.Item1); });

					var label = newToggle.transform.FindLoop("Label").GetComponent<TextMeshProUGUI>();
					label.text = boneGroup.Item2;
				}

				var massToggles = ikSection.transform.FindLoop("MassToggles");
				var onButton = massToggles.transform.FindLoop("On").GetComponent<Button>();
				var offButton = massToggles.transform.FindLoop("Off").GetComponent<Button>();

				onButton.onClick.AddListener(() =>
				{
					var toggles = ikSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = true;
					}
				});

				offButton.onClick.AddListener(() =>
				{
					var toggles = ikSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = false;
					}
				});

				templateToggle.gameObject.SetActive(false);

				ikSection.gameObject.SetActive(true);
			}
			#endregion IK

			#region CustomFK

			{
				var customFkSection = Instantiate(templateSection, templateSection.transform.parent);
				customFkSection.name = "CustomFK";
				var customFkText = customFkSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
				customFkText.text = "Custom FK";

				var templateToggle = customFkSection.transform.FindLoop("TemplateToggle");

				foreach (var customGroup in KineModController.NodeGroupIds)
				{
					var newToggle = Instantiate(templateToggle, templateToggle.transform.parent);
					newToggle.name = customGroup.Key;

					var toggle = newToggle.GetComponentInChildren<Toggle>();
					toggle.onValueChanged.AddListener((value) =>
					{
						var controller = CharCtrl.ociChar.charInfo.GetComponent<KineModController>();
						var nodeGroup = controller.CustomNodeGroups[customGroup.Key];
						nodeGroup.State = value;
						foreach (var boneName in nodeGroup.BoneNameStrings)
						{
							CharCtrl.ociChar.SetFkBoneState(boneName, nodeGroup.State);
						}
					});

					ValueMonitor.AddMonitor(newToggle.gameObject, () =>
					{
						var controller = CharCtrl.ociChar.charInfo.GetComponent<KineModController>();
						var nodeGroup = controller.CustomNodeGroups[customGroup.Key];
						if (nodeGroup.State != toggle.isOn)
						{
							toggle.isOn = nodeGroup.State;
						}
					});

					var resetButton = newToggle.GetComponentInChildren<Button>();
					resetButton.onClick.AddListener(() =>
					{
						var controller = CharCtrl.ociChar.charInfo.GetComponent<KineModController>();
						var nodeGroup = controller.CustomNodeGroups[customGroup.Key];
						CharCtrl.CopyFkBone(nodeGroup.BoneNameStrings);
					});

					var label = newToggle.transform.FindLoop("Label").GetComponent<TextMeshProUGUI>();
					label.text = customGroup.Key;
				}

				var massToggles = customFkSection.transform.FindLoop("MassToggles");
				var onButton = massToggles.transform.FindLoop("On").GetComponent<Button>();
				var offButton = massToggles.transform.FindLoop("Off").GetComponent<Button>();

				onButton.onClick.AddListener(() =>
				{
					var toggles = customFkSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = true;
					}
				});

				offButton.onClick.AddListener(() =>
				{
					var toggles = customFkSection.gameObject.GetComponentsInChildren<Toggle>();
					foreach (var toggle in toggles)
					{
						toggle.isOn = false;
					}
				});

				templateToggle.gameObject.SetActive(false);

				customFkSection.gameObject.SetActive(true);
			}

			#endregion

			KineMod.PluginLogger.LogDebug("Setting up Node Sliders...");

			#region NodeSection
			var nodeSection = modPanel.transform.FindLoop("NodeSection");
			nodeSection.transform.SetAsLastSibling();

			var fkSlider = nodeSection.transform.FindLoop("FK").GetComponentInChildren<Slider>();
			KineMod.PluginLogger.LogDebug("Doing Slider MinMax");
			fkSlider.minValue = 0.1f;
			fkSlider.maxValue = 1;
			KineMod.PluginLogger.LogDebug("Done with Slider MinMax");
			fkSlider.onValueChanged.AddListener(value =>
			{
				CharCtrl.fkInfo.sliderSize.value = value;
			});
			ValueMonitor.AddMonitor(fkSlider.gameObject, () =>
			{
				if (Math.Abs(CharCtrl.fkInfo.sliderSize.value - fkSlider.value) > 0.001)
				{
					fkSlider.value = CharCtrl.fkInfo.sliderSize.value;
				}
			});

			var ikSlider = nodeSection.transform.FindLoop("IK").GetComponentInChildren<Slider>();
			ikSlider.minValue = 0.1f;
			ikSlider.maxValue = 1;
			ikSlider.onValueChanged.AddListener(value =>
			{
				CharCtrl.ikInfo.sliderSize.value = value;
			});
			ValueMonitor.AddMonitor(ikSlider.gameObject, () =>
			{
				if (Math.Abs(CharCtrl.ikInfo.sliderSize.value - ikSlider.value) > 0.001)
				{
					ikSlider.value = CharCtrl.ikInfo.sliderSize.value;
				}
			});
			#endregion
		}

		private static bool CreateKinematicsButton()
		{
			var listMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content");
			var fkButton = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content/FK");

			var newSelect = Instantiate(fkButton, listMenu.transform, true);
			newSelect.name = "KineMod";
			newSelect.transform.SetAsFirstSibling();

			var textLabel = newSelect.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
			textLabel.text = "KineMod";

			var button = newSelect.GetComponent<Button>();
			button.onClick = new Button.ButtonClickedEvent();

			foreach (var existingButtons in listMenu.GetComponentsInChildren<Button>())
			{
				if (button.Equals(existingButtons))
				{
					continue;
				}

				existingButtons.onClick.AddListener(delegate
				{
					_menuGameObject.SetActive(false);
					button.image.color = Color.white;
				});
			}

			var kineMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic");

			button.onClick.AddListener(delegate
			{
				foreach (Transform child in kineMenu.transform)
				{
					if (child.name == "Viewport" || child.name == "Scrollbar Vertical")
					{
						continue;
					}

					child.gameObject.SetActive(false);
				}

				foreach (var offButtons in listMenu.GetComponentsInChildren<Button>())
				{
					offButtons.image.color = Color.white;
				}

				CharCtrl.kinematic = -1;
				_menuGameObject.SetActive(true);
				button.image.color = Color.green;
			});

			return true;
		}
	}
}

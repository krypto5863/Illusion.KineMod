using IllusionUtility.GetUtility;
using Studio;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Studio.OIBoneInfo;
using Object = UnityEngine.Object;

namespace Core_KineMod.UGUIResources
{
	internal class MainPage
	{
		private static OCIChar CurrentCharacter => KineModWindow.CharCtrl.ociChar;
		private static KineModController Controller => CurrentCharacter.charInfo.GetComponent<KineModController>();

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

		internal static void SetupMainPage(GameObject modPanel)
		{
			var templateSection = modPanel.transform.FindLoop("TemplateSection");
			templateSection.gameObject.SetActive(false);

			var systemActive = modPanel.transform.FindLoop("SystemActive").GetComponentInChildren<Toggle>();
			ToggleSynchronizer.AddMonitor(systemActive, () => Controller.SystemActive, b =>
			{
				Controller.ChangeSystemState(b);
			});

			var referAnimationButton = modPanel.transform.FindLoop("ReferToAnim").GetComponentInChildren<Button>();
			referAnimationButton.onClick.AddListener(() =>
			{
				KineModWindow.CharCtrl.SetCopyBoneIK((BoneGroup)31);
				KineModWindow.CharCtrl.SetCopyBoneFK((BoneGroup)353);
			});

			var ikToFkButton = modPanel.transform.FindLoop("IKToFK").GetComponentInChildren<Button>();
			ikToFkButton.onClick.AddListener(() =>
			{
				KineModWindow.CharCtrl.CopyBoneFK((BoneGroup)353);
			});
			//Disabling for now, really don't see the usage given a proper workflow.
			//ikToFkButton.gameObject.SetActive(true);

			GenerateFkSection(templateSection.gameObject);
			GenerateIkSection(templateSection.gameObject);
			GenerateCustomFkSection(templateSection.gameObject);

			#region NodeSection

			InitNodeSliders(modPanel);

			#endregion NodeSection
		}

		private static void InitNodeSliders(GameObject modPanel)
		{
			var nodeSection = modPanel.transform.FindLoop("NodeSection");
			nodeSection.transform.SetAsLastSibling();

			var fkSlider = nodeSection.transform.FindLoop("FK").GetComponentInChildren<Slider>();
			//KineMod.PluginLogger.LogDebug("Doing Slider MinMax");
			fkSlider.minValue = 0.1f;
			fkSlider.maxValue = 1;
			//KineMod.PluginLogger.LogDebug("Done with Slider MinMax");
			SliderSynchronizer.AddMonitor(fkSlider, () => KineModWindow.CharCtrl.fkInfo.sliderSize.value, f =>
			{
				KineModWindow.CharCtrl.fkInfo.sliderSize.value = f;
			});

			var ikSlider = nodeSection.transform.FindLoop("IK").GetComponentInChildren<Slider>();
			ikSlider.minValue = 0.1f;
			ikSlider.maxValue = 1;
			SliderSynchronizer.AddMonitor(ikSlider, () => KineModWindow.CharCtrl.ikInfo.sliderSize.value, f =>
			{
				KineModWindow.CharCtrl.ikInfo.sliderSize.value = f;
			});
		}

		private static void GenerateCustomFkSection(GameObject templateSection)
		{
			var customFkSection = Object.Instantiate(templateSection, templateSection.transform.parent);
			customFkSection.name = "CustomFK";
			var customFkText = customFkSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
			customFkText.text = "Custom FK";

			var templateToggle = customFkSection.transform.FindLoop("TemplateToggle");

			foreach (var customGroup in CustomBoneInfo.BoneNames)
			{
				var newToggle = Object.Instantiate(templateToggle, templateToggle.transform.parent);
				newToggle.name = customGroup.Key;

				var toggle = newToggle.GetComponentInChildren<Toggle>();
				ToggleSynchronizer.AddMonitor(toggle, () => Controller.CustomNodeGroups[customGroup.Key].State, value =>
				{
					Controller.ChangeCustomBoneState(customGroup.Key, value);
				});

				var resetButton = newToggle.GetComponentInChildren<Button>();
				resetButton.onClick.AddListener(() =>
				{
					var nodeGroup = Controller.CustomNodeGroups[customGroup.Key];
					KineModWindow.CharCtrl.CopyFkBone(nodeGroup.BoneNameStrings, customGroup.Value._resetToZero);
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

		private static void GenerateIkSection(GameObject templateSection)
		{
			var ikSection = Object.Instantiate(templateSection, templateSection.transform.parent);
			ikSection.name = "IK";
			var ikText = ikSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
			ikText.text = "IK";

			var templateToggle = ikSection.transform.FindLoop("TemplateToggle");

			foreach (var boneGroup in IkBoneGroupNames)
			{
				var newToggle = Object.Instantiate(templateToggle, templateToggle.transform.parent);
				newToggle.name = boneGroup.Item2;

				var toggle = newToggle.GetComponentInChildren<Toggle>();
				ToggleSynchronizer.AddMonitor(toggle, () => CurrentCharacter.IsIkBoneGroupActive(boneGroup.Item1), value =>
				{
					KineModWindow.CharCtrl.ikInfo.OnChangeValueIndividual(boneGroup.Item1, value);
				});

				var resetButton = newToggle.GetComponentInChildren<Button>();
				resetButton.onClick.AddListener(() => { KineModWindow.CharCtrl.SetCopyBoneIK(boneGroup.Item1); });

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

		private static void GenerateFkSection(GameObject templateSection)
		{
			var fkSection = Object.Instantiate(templateSection, templateSection.transform.parent);
			fkSection.name = "FK";
			var fkText = fkSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
			fkText.text = "FK";

			var templateToggle = fkSection.transform.FindLoop("TemplateToggle");

			foreach (var boneGroup in FkBoneGroupNames)
			{
				var newToggle = Object.Instantiate(templateToggle, templateToggle.transform.parent);
				newToggle.name = boneGroup.Item2;

				var toggle = newToggle.GetComponentInChildren<Toggle>();
				ToggleSynchronizer.AddMonitor(toggle, () => CurrentCharacter.IsFkBoneGroupActive(boneGroup.Item1), value =>
				{
					KineModWindow.CharCtrl.fkInfo.OnChangeValueIndividual(boneGroup.Item1, value);
				});

				var resetButton = newToggle.GetComponentInChildren<Button>();
				resetButton.onClick.AddListener(() => { KineModWindow.CharCtrl.SetCopyBoneFK(boneGroup.Item1); });

				if (boneGroup.Item1 == BoneGroup.Hair || boneGroup.Item1 == BoneGroup.Skirt)
				{
					//Required for their state resets only.
					resetButton.onClick.AddListener(() =>
					{
						KineModWindow.CharCtrl.fkInfo.OnClickInitSingle(boneGroup.Item1);
					});
				}

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
	}
}
using IllusionUtility.GetUtility;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Studio.OIBoneInfo;
using Object = UnityEngine.Object;

namespace Core_KineMod.UGUIResources
{
	internal class HandPage
	{
		private static OCIChar CurrentCharacter => KineModWindow.CharCtrl.ociChar;
		private static KineModController Controller => CurrentCharacter.charInfo.GetComponent<KineModController>();

		private static readonly Dictionary<BoneGroup, string> FkBoneGroupNames = new Dictionary<BoneGroup, string>
		{
			{BoneGroup.RightHand, "R. Hand"},
			{BoneGroup.LeftHand, "L. Hand"},
		};

		//Todo HS2 hands.
		private static readonly string[] PatternToNames =
		{
#if KKS
			"Flat",
			"Fist",
			"Peace",
			"Flat Spread",
			"Closed",
			"Closed Loose",
			"Closed Looser",
			"Open Relaxed",
			"Relaxed",
			"Holding Card",
			"Picking",
			"Loose Pinch",
			"Loose Point",
			"Cupping",
			"Point",
			"Point Thumb Out",
			"Fist Thumb Out",
			"OK",
			"Grip",
			"Point Two-Fingers",
			"Claw",
			"Grip Large",
			"Grip Two-Fingers",
			"Onani"
#elif HS2
			"Flat",
			"Fist",
			"Peace",
			"Flat Spread",
			"Closed",
			"Closed Loose",
			"Closed Looser",
			"Open Relaxed",
			"Grabbing",
			"Holding Card",
			"Picking",
			"Loose Pinch",
			"Large Pinch",
			"Cupping",
			"Point",
			"Point Thumb Out",
			"Thumb Out",
			"OK",
			"Hold",
			"Point Two-Fingers",
			"Claw",
			"Flat Thumb Low",
#endif
		};

		internal static void SetupHandPage(GameObject modPanel)
		{
			modPanel = modPanel.transform.FindLoop("HandPage").gameObject;

			GenerateHandPresetSections(modPanel);

			var templateSection = modPanel.transform.FindLoop("TemplateSection");
			templateSection.gameObject.SetActive(false);

			GenerateHandsSections(BoneGroup.RightHand, templateSection.gameObject);
			GenerateHandsSections(BoneGroup.LeftHand, templateSection.gameObject);
		}

		private static void GenerateHandPresetSections(GameObject modPanel)
		{
			{
				var presetSaveSec = modPanel.transform.FindLoop("PresetSave");
				var presetSaveName = presetSaveSec.GetComponentInChildren<TMP_InputField>();
				var presetSaveLeft = presetSaveSec.transform.FindLoop("ButtonLeft").GetComponentInChildren<Button>();
				var presetSaveRight = presetSaveSec.transform.FindLoop("ButtonRight").GetComponentInChildren<Button>();

				var presetLoadSec = modPanel.transform.FindLoop("PresetLoad");
				var presetLoadSelect = presetLoadSec.GetComponentInChildren<TMP_Dropdown>();
				var presetLoadLeft = presetLoadSec.transform.FindLoop("ButtonLeft").GetComponentInChildren<Button>();
				var presetLoadRight = presetLoadSec.transform.FindLoop("ButtonRight").GetComponentInChildren<Button>();

				void UpdatePresetList()
				{
					presetLoadSelect.ClearOptions();

					var options = KineMod.HandPoseList
						.Select(d => d.Name)
						.ToList();

					presetLoadSelect.AddOptions(options);
				}

				UpdatePresetList();

				KineMod.HandPoseList.ObserveCountChanged().Subscribe(i => UpdatePresetList());

				presetLoadLeft.onClick.AddListener(() =>
				{
					var presetToLoad = presetLoadSelect.options[presetLoadSelect.value];
					var stringText = presetToLoad.text;
					Controller.LoadHandPose(BoneGroup.LeftHand, stringText);
				});

				presetLoadRight.onClick.AddListener(() =>
				{
					var presetToLoad = presetLoadSelect.options[presetLoadSelect.value];
					var stringText = presetToLoad.text;
					Controller.LoadHandPose(BoneGroup.RightHand, stringText);
				});

				presetSaveLeft.onClick.AddListener(() =>
				{
					Controller.SaveHandPose(presetSaveName.text, BoneGroup.LeftHand);
				});

				presetSaveRight.onClick.AddListener(() =>
				{
					Controller.SaveHandPose(presetSaveName.text, BoneGroup.RightHand);
				});
			}
		}

		private static void GenerateHandsSections(BoneGroup boneGroup, GameObject templateSection)
		{
			var handSection = Object.Instantiate(templateSection, templateSection.transform.parent);
			handSection.name = boneGroup.ToString();
			var sectionText = handSection.transform.FindLoop("HeaderText").GetComponent<TextMeshProUGUI>();
			sectionText.text = FkBoneGroupNames[boneGroup];

			#region ToggleReset

			var templateToggle = handSection.transform.FindLoop("TemplateToggle");
			templateToggle.name = boneGroup.ToString();

			var toggle = templateToggle.GetComponentInChildren<Toggle>();
			ToggleSynchronizer.AddMonitor(toggle, () => CurrentCharacter.IsFkBoneGroupActive(boneGroup),
				value => { KineModWindow.CharCtrl.fkInfo.OnChangeValueIndividual(boneGroup, value); });

			var resetButton = templateToggle.GetComponentInChildren<Button>();
			resetButton.onClick.AddListener(() => { KineModWindow.CharCtrl.SetCopyBoneFK(boneGroup); });

			var label = templateToggle.transform.FindLoop("Label").GetComponent<TextMeshProUGUI>();
			label.text = "FK Control";

			#endregion

			var hand = boneGroup == BoneGroup.LeftHand ? 0 : 1;

			#region Pattern
			{
				var handDropdownSec1 = handSection.transform.FindLoop("HandPattern1");
				var handDropdown1 = handDropdownSec1.GetComponentInChildren<TMP_Dropdown>();
				var handDropdown1Back = handDropdownSec1.transform.FindLoop("Back").GetComponentInChildren<Button>();
				var handDropdown1Next = handDropdownSec1.transform.FindLoop("Forward").GetComponentInChildren<Button>();

				handDropdown1.ClearOptions();

				DropDownSynchronizer.AddMonitor(handDropdown1, () => CurrentCharacter.oiCharInfo.handPtn[hand], i =>
				{
					CurrentCharacter.ChangeHandAnime(hand, i);
				}, () =>
				{
					var handOptions = CurrentCharacter.HandAnimeNum;
#if HS2
					handOptions += 1;
#endif

					if (handDropdown1.options.Count == handOptions)
					{
						return false;
					}

					handDropdown1.ClearOptions();
					var options = Enumerable.Range(0, handOptions)
						.Select(d => PatternToNames[d])
						.ToList();
					handDropdown1.AddOptions(options);

					return true;
				});

				handDropdown1Back.onClick.AddListener(() =>
				{
					var options = handDropdown1.options.Count;

					var newOptions = handDropdown1.value - 1;
					newOptions = newOptions < 0 ? options - 1 : newOptions;

					handDropdown1.SetValue(newOptions);
				});

				handDropdown1Next.onClick.AddListener(() =>
				{
					var options = handDropdown1.options.Count;

					var newOptions = handDropdown1.value + 1;
					newOptions = newOptions >= options ? 0 : newOptions;

					handDropdown1.SetValue(newOptions);
				});
			}

			{
				var handDropdownSec2 = handSection.transform.FindLoop("HandPattern2");
				var handDropdown2 = handDropdownSec2.GetComponentInChildren<TMP_Dropdown>();
				var handDropdown2Back = handDropdownSec2.transform.FindLoop("Back").GetComponentInChildren<Button>();
				var handDropdown2Next = handDropdownSec2.transform.FindLoop("Forward").GetComponentInChildren<Button>();

				handDropdown2.ClearOptions();

				DropDownSynchronizer.AddMonitor(handDropdown2, () => Controller.HandStates[boneGroup].Pattern,
					i => { Controller.UpdateHandPattern(boneGroup, i); },
					() =>
					{
						var handOptions = CurrentCharacter.HandAnimeNum;
#if HS2
						handOptions += 1;
#endif

						if (handDropdown2.options.Count == handOptions)
						{
							return false;
						}

						handDropdown2.ClearOptions();

						var options = Enumerable.Range(0, handOptions)
							.Select(d => PatternToNames[d])
							.ToList();

						handDropdown2.AddOptions(options);

						handDropdown2.RefreshShownValue();

						return true;
					});

				handDropdown2Back.onClick.AddListener(() =>
				{
					var options = handDropdown2.options.Count;

					var newOptions = handDropdown2.value - 1;
					newOptions = newOptions < 0 ? options - 1 : newOptions;

					handDropdown2.SetValue(newOptions);
				});

				handDropdown2Next.onClick.AddListener(() =>
				{
					var options = handDropdown2.options.Count;

					var newOptions = handDropdown2.value + 1;
					newOptions = newOptions >= options ? 0 : newOptions;

					handDropdown2.SetValue(newOptions);
				});
			}
			#endregion

			#region BlendingSlider
			{
				var blendingSlider = handSection.transform.FindLoop("Blending")
					.GetComponentInChildren<Slider>();

				SliderSynchronizer.AddMonitor(blendingSlider, () => Controller.HandStates[boneGroup].Blending, f => Controller.UpdateHandBlending(boneGroup, f));
			}
			#endregion

			#region MirrorButton
			{
				var mirrorButton = handSection.transform.FindLoop("MirrorHand")
					.GetComponentInChildren<Button>();

				mirrorButton.onClick.AddListener(() =>
				{
					Controller.MirrorHand(boneGroup);
				});
			}
			#endregion

			handSection.gameObject.SetActive(true);
		}
	}
}
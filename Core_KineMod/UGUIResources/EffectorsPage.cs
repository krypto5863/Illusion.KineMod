using IllusionUtility.GetUtility;
using Studio;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Core_KineMod.UGUIResources
{
	internal static class EffectorsPage
	{
		private static OCIChar CurrentCharacter => KineModWindow.CharCtrl.ociChar;
		private static KineModController Controller => CurrentCharacter.charInfo.GetComponent<KineModController>();
		internal static void SetupEffectorsPage(GameObject modPanel)
		{
			var chainSectionTemplate = modPanel.transform.FindLoop("ChainSection");

			var enforceSettingsToggle = modPanel.transform.FindLoop("EnforceSettings").GetComponentInChildren<Toggle>();
			ToggleSynchronizer.AddMonitor(enforceSettingsToggle, () => Controller.EnforceEffectors, b =>
			{
				Controller.ChangeEnforceEffectors(b);
			});

			var groupings = EffectorsInfo.BonesInfo.GroupBy(r => r.Value.LimbName);
			foreach (var grouping in groupings)
			{
				var newChainSection = Object.Instantiate(chainSectionTemplate, chainSectionTemplate.transform.parent);
				newChainSection.name = grouping.Key;
				var sectionName = newChainSection.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
				sectionName.text = grouping.Key;

				var sliderCluster = newChainSection.transform.FindLoop("SliderCluster");
				foreach (var limb in grouping)
				{
					var newSliderCluster = Object.Instantiate(sliderCluster, sliderCluster.transform.parent);
					newSliderCluster.name = limb.Value.UserFriendlyName;
					var nameText = newSliderCluster.transform.FindLoop("Name").GetComponent<TextMeshProUGUI>();
					nameText.text = limb.Value.UserFriendlyName;

					var posSliderUnit = newSliderCluster.transform.FindLoop("PosSlider");
					var posSlider = posSliderUnit.GetComponentInChildren<Slider>();

					InitSliderText(posSlider);

					var rotSliderUnit = newSliderCluster.transform.FindLoop("RotSlider");
					var rotSlider = rotSliderUnit.GetComponentInChildren<Slider>();

					InitSliderText(rotSlider);

					if (limb.Value.IsBendGoal)
					{
						SetupBendGoalSliderCluster(limb.Key, posSlider, rotSliderUnit.gameObject);
					}
					else
					{
						SetupEffectorSliderCluster(limb.Key, posSlider, rotSlider, rotSliderUnit.gameObject, limb.Value.HasRotation);
					}
				}
				sliderCluster.gameObject.SetActive(false);
			}

			chainSectionTemplate.gameObject.SetActive(false);
		}

		private static void SetupBendGoalSliderCluster(string limb, Slider posSlider,
			GameObject rotSliderUnit)
		{
			//posSlider.onValueChanged.AddListener(value => ViewModel.BendGoalSliderChanged(limb, value));
			/*
			ValueMonitor.AddMonitor(posSlider.gameObject, () =>
			{
				var effector = CharCtrl.ociChar.GetBendGoalByTargetName(limb);
				if (Math.Abs(effector.weight - posSlider.value) > 0.01)
				{
					posSlider.value = effector.weight;
				}
			});
			*/
			SliderSynchronizer.AddMonitor(posSlider, () => Controller.BendGoals[limb], f =>
			{
				Controller.ChangeBendGoalWeight(limb, f);
			});
			rotSliderUnit.gameObject.SetActive(false);
		}

		private static void SetupEffectorSliderCluster(string limb, Slider posSlider, Slider rotSlider,
			GameObject rotSliderUnit, bool hasRotation)
		{
			//posSlider.onValueChanged.AddListener(value => ViewModel.PositionSliderChanged(limb, value));
			SliderSynchronizer.AddMonitor(posSlider, () => Controller.Effectors[limb][0], f =>
			{
				Controller.ChangeEffectorWeight(limb, f, false);
			});
			/*
			ValueMonitor.AddMonitor(posSlider.gameObject, () =>
			{
				var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb);
				if (Math.Abs(effector.positionWeight - posSlider.value) > 0.01)
				{
					posSlider.value = effector.positionWeight;
				}
			});
			*/
			if (hasRotation)
			{
				//rotSlider.onValueChanged.AddListener(value => ViewModel.RotationSliderChanged(limb, value));
				SliderSynchronizer.AddMonitor(rotSlider, () => Controller.Effectors[limb][1], f =>
				{
					Controller.ChangeEffectorWeight(limb, f, true);
				});
			}
			else
			{
				rotSliderUnit.SetActive(false);
			}
			/*
			ValueMonitor.AddMonitor(rotSlider.gameObject, () =>
			{
				var effector = CharCtrl.ociChar.GetEffectorByTargetName(limb);
				if (effector.isEndEffector == false)
				{
					rotSliderUnit.gameObject.SetActive(false);
				}

				if (Math.Abs(effector.rotationWeight - rotSlider.value) > 0.01)
				{
					rotSlider.value = effector.rotationWeight;
				}
			});
			*/
		}

		private static void InitSliderText(Slider slider)
		{
			var valueText = slider.transform.FindLoop("Value").GetComponent<TextMeshProUGUI>();
			slider.onValueChanged.AddListener(value =>
			{
				valueText.text = value.ToString("0.00");
			});
		}
	}
}
using System;
using Studio;
using UnityEngine;
using static Studio.OIBoneInfo;

internal static class MainPage
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

	internal static void Draw(MPCharCtrl mCharCtrl)
	{
		var character = mCharCtrl.ociChar;
		var controller = character.charInfo.GetComponent<KineModController>();

		DisplayStateToggler(character, controller);
		if (GUILayout.Button("Refer to Animation"))
		{
			mCharCtrl.SetCopyBoneIK((BoneGroup)31);
			mCharCtrl.SetCopyBoneFK((BoneGroup)353);
		}

		DrawFkSection(mCharCtrl);

		DrawIkSection(mCharCtrl);

		DrawCustomSection(mCharCtrl, controller);

		DrawEndSection(mCharCtrl);
	}

	private static void DrawFkSection(MPCharCtrl mCharCtrl)
	{
		GUILayout.BeginVertical(Styles.LightSection);
		GUILayout.BeginHorizontal();
		GUILayout.Label("FK");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Off"))
		{
			foreach (var boneGroup in FkBoneGroupNames)
			{
				mCharCtrl.fkInfo.OnChangeValueIndividual(boneGroup.Item1, false);
			}
		}
		if (GUILayout.Button("On"))
		{
			foreach (var boneGroup in FkBoneGroupNames)
			{
				mCharCtrl.fkInfo.OnChangeValueIndividual(boneGroup.Item1, true);
			}
		}
		GUILayout.EndHorizontal();
		foreach (var fkGroups in FkBoneGroupNames)
		{
			DisplayFkToggleReset(mCharCtrl, fkGroups.Item1, fkGroups.Item2);
		}
		GUILayout.EndVertical();
	}
	private static void DrawIkSection(MPCharCtrl mCharCtrl)
	{
		GUILayout.BeginVertical(Styles.LightSection);
		GUILayout.BeginHorizontal();
		GUILayout.Label("IK");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Off"))
		{
			foreach (var boneGroup in IkBoneGroupNames)
			{
				mCharCtrl.ikInfo.OnChangeValueIndividual(boneGroup.Item1, false);
			}
		}
		if (GUILayout.Button("On"))
		{
			foreach (var boneGroup in IkBoneGroupNames)
			{
				mCharCtrl.ikInfo.OnChangeValueIndividual(boneGroup.Item1, true);
			}
		}
		GUILayout.EndHorizontal();
		foreach (var fkGroups in IkBoneGroupNames)
		{
			DisplayIkToggleReset(mCharCtrl, fkGroups.Item1, fkGroups.Item2);
		}
		GUILayout.EndVertical();
	}
	private static void DrawCustomSection(MPCharCtrl mCharCtrl, KineModController controller)
	{
		GUILayout.BeginVertical(Styles.LightSection);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Custom FK");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Off"))
		{
			foreach (var customGroup in controller.CustomNodeGroups)
			{
				foreach (var boneName in customGroup.Value.BoneNameStrings)
				{
					customGroup.Value.State = false;
					mCharCtrl.ociChar.SetFkBoneState(boneName, false);
				}
			}
		}
		if (GUILayout.Button("On"))
		{
			foreach (var customGroup in controller.CustomNodeGroups)
			{
				foreach (var boneName in customGroup.Value.BoneNameStrings)
				{
					customGroup.Value.State = true;
					mCharCtrl.ociChar.SetFkBoneState(boneName, true);
				}
			}
		}
		GUILayout.EndHorizontal();
		foreach (var customGroup in controller.CustomNodeGroups)
		{
			DisplayCustomToggleReset(mCharCtrl, customGroup.Key, customGroup.Value);
		}
		GUILayout.EndVertical();
	}
	private static void DrawEndSection(MPCharCtrl mCharCtrl)
	{
		var fkNodeSizeSlider = mCharCtrl.fkInfo.sliderSize;
		var ikNodeSizeSlider = mCharCtrl.ikInfo.sliderSize;

		GUILayout.BeginVertical(Styles.LightSection);
		GUILayout.Label("FK Node Size");
		fkNodeSizeSlider.value = GUILayout.HorizontalSlider(fkNodeSizeSlider.value, fkNodeSizeSlider.minValue, fkNodeSizeSlider.maxValue);
		GUILayout.Label("IK Node Size");
		ikNodeSizeSlider.value = GUILayout.HorizontalSlider(ikNodeSizeSlider.value, ikNodeSizeSlider.minValue, ikNodeSizeSlider.maxValue);
		GUILayout.EndVertical();
	}
	private static void DisplayStateToggler(OCIChar character, KineModController controller)
	{
		var guiStyle = controller.SystemActive ? Styles.GreenButton : Styles.RedButton;
		var buttonLabel = controller.SystemActive ? "Online" : "Offline";

		if (GUILayout.Button(buttonLabel, guiStyle))
		{
			controller.SystemActive = !controller.SystemActive;
			if (controller.SystemActive)
			{
				KineMod.EnableFkIk(character);
			}
			else
			{
				KineMod.DisableFkIk(character);
			}
		}
	}

	private static void DisplayFkToggleReset(MPCharCtrl mCharCtrl, BoneGroup boneGroup, string text)
	{
		var character = mCharCtrl.ociChar;
		var state = character.IsFkBoneGroupActive(boneGroup);
		var guiStyle = state ? Styles.GreenButton : Styles.RedButton;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(text, guiStyle))
		{
			mCharCtrl.fkInfo.OnChangeValueIndividual(boneGroup, !state);
		}
		if (GUILayout.Button("Reset", GUILayout.MaxWidth(50), GUILayout.MinWidth(50)))
		{
			mCharCtrl.SetCopyBoneFK(boneGroup);
		}
		GUILayout.EndHorizontal();
	}

	private static void DisplayIkToggleReset(MPCharCtrl mCharCtrl, BoneGroup boneGroup, string text)
	{
		var character = mCharCtrl.ociChar;
		var state = character.IsIkBoneGroupActive(boneGroup);
		var guiStyle = state ? Styles.GreenButton : Styles.RedButton;

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(text, guiStyle))
		{
			mCharCtrl.ikInfo.OnChangeValueIndividual(boneGroup, !state);
		}
		if (GUILayout.Button("Reset", GUILayout.MaxWidth(50), GUILayout.MinWidth(50)))
		{
			mCharCtrl.SetCopyBoneIK(boneGroup);
		}
		GUILayout.EndHorizontal();
	}

	private static void DisplayCustomToggleReset(MPCharCtrl mCharCtrl, string text, CustomNodeGroup nodeGroup)
	{
		var character = mCharCtrl.ociChar;
		GUILayout.BeginHorizontal();
		var guiStyle = nodeGroup.State ? Styles.GreenButton : Styles.RedButton;
		if (GUILayout.Button(text, guiStyle))
		{
			nodeGroup.State = !nodeGroup.State;
			foreach (var boneName in nodeGroup.BoneNameStrings)
			{
				character.SetFkBoneState(boneName, nodeGroup.State);
			}
		}
		if (GUILayout.Button("Reset", GUILayout.MaxWidth(50), GUILayout.MinWidth(50)))
		{
			mCharCtrl.CopyFkBone(nodeGroup.BoneNameStrings);
		}
		GUILayout.EndHorizontal();
	}
}
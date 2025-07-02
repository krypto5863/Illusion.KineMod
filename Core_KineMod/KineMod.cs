using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Core_KineMod.UGUIResources;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;
using MessagePack;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using KineModWindow = Core_KineMod.IMGUIResources.KineModWindow;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[BepInPlugin(Guid, DisplayName, Version)]
#if HS2
[BepInProcess("StudioNEOV2")]
#else
[BepInProcess("CharaStudio")]
#endif
[BepInIncompatibility("com.deathweasel.bepinex.fkik")]
[BepInDependency(KKAPI.KoikatuAPI.GUID)]
internal class KineMod : BaseUnityPlugin
{
	public const string Guid = "com.krypto.plugin.kinemod";
	public const string DisplayName = "KineMod";
	public const string Version = "1.3";

	internal static KineMod PluginInstance;
	internal static ManualLogSource PluginLogger => PluginInstance.Logger;

	internal static ConfigEntry<float> UiPanelScale;
	internal static ConfigEntry<bool> PoseableFixedHead;

	private static MPCharCtrl _charCtrl;
	private static GameObject _fakeMenuObject;

	internal static string HandPosePath = Path.Combine(UserData.Path, Studio.Studio.UserPath, "HandPoses");
	internal static readonly ReactiveCollection<HandPose> HandPoseList = new ReactiveCollection<HandPose>();

	private void Awake()
	{
		PluginInstance = this;
		//Harmony.CreateAndPatchAll(typeof(FKPositionalPatch));
		Harmony.CreateAndPatchAll(typeof(ExtraFKNodes));
		Harmony.CreateAndPatchAll(typeof(FkCtrlPatch));
		Harmony.CreateAndPatchAll(typeof(NeckLookControllerPatch));
		Harmony.CreateAndPatchAll(typeof(Hooks));
		Harmony.CreateAndPatchAll(typeof(HandBlendHook));

		UiPanelScale = Config.Bind("UI", "UI Scale Multiplier", 1.0f, new ConfigDescription("Will change the scale of the UI", new AcceptableValueRange<float>(0.1f, 2f)));
		UiPanelScale.SettingChanged += (value, eventArg) =>
		{
			var menuObject = Core_KineMod.UGUIResources.KineModWindow.MenuGameObject;
			if (!menuObject)
			{
				return;
			}
			menuObject.transform.localScale = Vector3.one * UiPanelScale.Value;
		};

		PoseableFixedHead = Config.Bind("General", "Poseable Fixed Head", true, "Allows you to pose the head with FK when the head is set to Fixed. This is the old behavior, not the intended one, but too many scenes use this.");

		Directory.CreateDirectory(HandPosePath);
		LoadHandPoses();

		//CharacterApi.CharacterReloaded += CharacterApiOnCharacterReloaded;
		StudioAPI.StudioLoadedChanged += StudioAPI_StudioLoadedChanged;
		CharacterApi.RegisterExtraBehaviour<KineModController>(Guid);
	}

	private void LoadHandPoses()
	{
		var files = Directory.EnumerateFiles(HandPosePath);

		foreach (var file in files)
		{
			var handPose = LoadHandPose(Path.GetFileName(file));
			HandPoseList.Add(handPose);
		}
	}

	private static void StudioAPI_StudioLoadedChanged(object sender, EventArgs e)
	{
		//CreateMenuEntry();
		if (Core_KineMod.UGUIResources.KineModWindow.InitGui() == false)
		{
			CreateMenuEntry();
		}
	}

	private void OnGUI()
	{
		if (_fakeMenuObject && _fakeMenuObject.activeInHierarchy)
		{
			KineModWindow.DoADraw();
		}
	}

	private static void CreateMenuEntry()
	{
		var listMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content");
		var fkButton = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content/FK");
		var newSelect = Instantiate(fkButton, listMenu.transform, true);
		newSelect.name = "KineMod";
		newSelect.transform.SetAsFirstSibling();

		var button = newSelect.GetComponent<Button>();
		button.onClick = new Button.ButtonClickedEvent();

		var tmp = newSelect.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		tmp.text = "KineMod";

		var kineMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic");
		_fakeMenuObject = new GameObject("KineMod");
		_fakeMenuObject.SetActive(false);
		_fakeMenuObject.transform.SetParent(kineMenu.transform);

		var parentObject = button.transform.parent;
		while (parentObject != null)
		{
			_charCtrl = parentObject.GetComponent<MPCharCtrl>();

			if (_charCtrl != null)
			{
				break;
			}

			parentObject = parentObject.parent;
		}

		foreach (var existingButtons in listMenu.GetComponentsInChildren<Button>())
		{
			if (button.Equals(existingButtons))
			{
				continue;
			}

			existingButtons.onClick.AddListener(delegate
			{
				_fakeMenuObject.SetActive(false);
				button.image.color = Color.white;
			});
		}

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

			_charCtrl.kinematic = -1;
			_fakeMenuObject.SetActive(true);
			button.image.color = Color.green;
		});
	}

	internal static void AddExtraFKBones(ref Dictionary<int, Info.BoneInfo> boneDic)
	{
		foreach (var bone in CustomBoneNode.CustomNodes)
		{
			var num = bone.Bone.GetHashCode();
#if KKS
			boneDic[num] = new Info.BoneInfo()
			{
				no = num,
				bone = bone.Bone,
				group = bone.Group,
				level = bone.Level,
				name = bone.Name
			};
#else
			boneDic[num] = new Info.BoneInfo(num, bone.Bone, new List<string>())
			{
				no = num,
				bone = bone.Bone,
				group = bone.Group,
				level = bone.Level,
				name = bone.Name
			};
#endif
		}
	}

	internal static void EnableFkIk(OCIChar character)
	{
#if DEBUG
		PluginLogger.LogDebug($"Turning on FKIK for {character.charInfo.chaFile.charaFileName}!");
#endif

		//Initializing IK stuff
		//var ptnNo = character.neckLookCtrl.ptnNo;
		character.oiCharInfo.enableIK = true;
		character.ActiveIK(OIBoneInfo.BoneGroup.Body, character.oiCharInfo.activeIK[0], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.RightLeg, character.oiCharInfo.activeIK[1], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.LeftLeg, character.oiCharInfo.activeIK[2], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.RightArm, character.oiCharInfo.activeIK[3], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.LeftArm, character.oiCharInfo.activeIK[4], true);
		//Todo character.ActiveIK(OIBoneInfo.BoneGroup.Skirt, character.oiCharInfo.activeIK[5], true);
		character.ActiveKinematicMode(OICharInfo.KinematicMode.IK, true, true);

		//Initializing FK stuff
		character.fkCtrl.enabled = true;
		character.oiCharInfo.enableFK = true;
		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			var part = FKCtrl.parts[i];
			var state = character.oiCharInfo.activeFK[i];
			character.ActiveFK(part, state, true);
#if DEBUG
			PluginLogger.LogDebug($"{part.ToString()} is being set to {state}.");
#endif
		}
		/*
		if (ptnNo != character.neckLookCtrl.ptnNo)
		{
			character.ChangeLookNeckPtn(ptnNo);
		}
		*/
	}

	internal static void DisableFkIk(OCIChar character)
	{
#if DEBUG
		PluginLogger.LogDebug($"Turning off FKIK for {character.charInfo.chaFile.charaFileName}!");
#endif

		//Initializing IK stuff
		//var ptnNo = character.neckLookCtrl.ptnNo;

		character.oiCharInfo.enableIK = false;
		character.ActiveKinematicMode(OICharInfo.KinematicMode.IK, false, true);

		//Initializing FK stuff
		character.fkCtrl.enabled = false;
		character.oiCharInfo.enableFK = false;

		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			//ToggleSynchronizer off then on if applicable.
			character.ActiveFK(FKCtrl.parts[i], false, true);
			character.ActiveFK(FKCtrl.parts[i], character.oiCharInfo.activeFK[i]);
		}
		/*
		if (ptnNo != character.neckLookCtrl.ptnNo)
		{
			character.ChangeLookNeckPtn(ptnNo);
		}
		*/
	}

	internal static void SaveHandPose(HandPose handPose)
	{
		for (int i = HandPoseList.Count - 1 - 1; i >= 0; i--)
		{
			var tempHandPose = HandPoseList[i];

			if (tempHandPose.Name.Equals(handPose.Name))
			{
				HandPoseList.RemoveItem(i);
			}
		}

		HandPoseList.Add(handPose);

		var handPoseDestination = Path.Combine(HandPosePath, handPose.Name);
		var poseData= MessagePackSerializer.Serialize(handPose);
		File.WriteAllBytes(handPoseDestination, poseData);
	}

	private static HandPose LoadHandPose(string handPoseName)
	{
		var data = File.ReadAllBytes(Path.Combine(HandPosePath, handPoseName));
		var handPose = MessagePackSerializer.Deserialize<HandPose>(data);
		return handPose;
	}
}
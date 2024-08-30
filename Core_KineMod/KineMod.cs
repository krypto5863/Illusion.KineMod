using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;
using Studio;
using System;
using System.Security;
using System.Security.Permissions;
using Core_KineMod.IMGUIResources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[BepInPlugin(GUID, DisplayName, Version)]
#if HS2
	[BepInProcess("StudioNEOV2")]
#else
	[BepInProcess("CharaStudio")]
#endif
internal class KineMod : BaseUnityPlugin
{
	public const string GUID = "KineMod";
	public const string DisplayName = "KineMod";
	public const string Version = "1.0";

	internal static KineMod PluginInstance;
	internal static ManualLogSource PluginLogger => PluginInstance.Logger;

	private static MPCharCtrl _charCtrl;
	private static GameObject _fakeMenuObject;

	private void Awake()
	{
		PluginInstance = this;
		Harmony.CreateAndPatchAll(typeof(FkCtrlPatch));
		Harmony.CreateAndPatchAll(typeof(Hooks));
		//CharacterApi.CharacterReloaded += CharacterApiOnCharacterReloaded;
		StudioAPI.StudioLoadedChanged += StudioAPI_StudioLoadedChanged;
		CharacterApi.RegisterExtraBehaviour<KineModController>(GUID);
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

	internal static void EnableFkIk(OCIChar character)
	{
		//Initializing IK stuff
		var ptnNo = character.neckLookCtrl.ptnNo;
		character.oiCharInfo.enableIK = true;
		character.ActiveIK(OIBoneInfo.BoneGroup.Body, character.oiCharInfo.activeIK[0], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.RightLeg, character.oiCharInfo.activeIK[1], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.LeftLeg, character.oiCharInfo.activeIK[2], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.RightArm, character.oiCharInfo.activeIK[3], true);
		character.ActiveIK(OIBoneInfo.BoneGroup.LeftArm, character.oiCharInfo.activeIK[4], true);
		character.ActiveKinematicMode(OICharInfo.KinematicMode.IK, true, true);

		//Initializing FK stuff
		character.fkCtrl.enabled = true;
		character.oiCharInfo.enableFK = true;
		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			character.ActiveFK(FKCtrl.parts[i], character.oiCharInfo.activeFK[i], true);
		}
		if (ptnNo != character.neckLookCtrl.ptnNo)
		{
			character.ChangeLookNeckPtn(ptnNo);
		}
	}

	internal static void DisableFkIk(OCIChar character)
	{
		//Initializing IK stuff
		var ptnNo = character.neckLookCtrl.ptnNo;

		character.oiCharInfo.enableIK = false;
		character.ActiveKinematicMode(OICharInfo.KinematicMode.IK, false, true);

		//Initializing FK stuff
		character.fkCtrl.enabled = false;
		character.oiCharInfo.enableFK = false;

		for (var i = 0; i < FKCtrl.parts.Length; i++)
		{
			//Toggle off then on if applicable.
			character.ActiveFK(FKCtrl.parts[i], false, true);
			character.ActiveFK(FKCtrl.parts[i], character.oiCharInfo.activeFK[i]);
		}
		if (ptnNo != character.neckLookCtrl.ptnNo)
		{
			character.ChangeLookNeckPtn(ptnNo);
		}
	}
}
using BepInEx;
using Studio;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace Core_KineMod.UGUIResources
{
	internal static class KineModWindow
	{
		internal static GameObject MenuGameObject { get; private set; }
		private static MPCharCtrl _charCtrl;
		internal static MPCharCtrl CharCtrl
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

			if (SetupUi(uiPanel, out var menuGameObject) == false)
			{
				return false;
			}

			MenuGameObject = menuGameObject;

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

			var fkMenu = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/00_FK");
			modPanel.transform.position = fkMenu.transform.position;
			modPanel.transform.localScale = Vector3.one * KineMod.UiPanelScale.Value;

			MainPage.SetupMainPage(modPanel);
			HandPage.SetupHandPage(modPanel);
			EffectorsPage.SetupEffectorsPage(modPanel);

			return true;
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
					MenuGameObject.SetActive(false);
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
				MenuGameObject.SetActive(true);
				button.image.color = Color.green;
			});

			return true;
		}
	}
}
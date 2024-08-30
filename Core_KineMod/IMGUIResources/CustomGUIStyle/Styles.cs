using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Core_KineMod.IMGUIResources
{
	internal static class Styles
	{
		private static readonly Texture2D WindowBackground =
			LoadTexture2D("CustomGUIStyle.StudioUIImage.png");

		private static readonly Texture2D ButtonBackground =
			LoadTexture2D("CustomGUIStyle.Button.png");

		private static readonly Texture2D ButtonBackgroundHover =
			LoadTexture2D("CustomGUIStyle.ButtonHover.png");

		private static readonly Texture2D ButtonBackgroundClick =
			LoadTexture2D("CustomGUIStyle.ButtonClick.png");

		private static readonly Texture2D SliderHorizontalBackground =
			LoadTexture2D("CustomGUIStyle.HorizontalSliderBackground.png");

		private static readonly Texture2D ScrollerVerticalBackground =
			LoadTexture2D("CustomGUIStyle.ScrollerBackground.png");

		private static readonly Texture2D ScrollerThumb =
			LoadTexture2D("CustomGUIStyle.ScrollerHandle.png");

		private static readonly Texture2D LightSectionTex =
			UiToolbox.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));

		private static readonly Texture2D DarkSectionTex =
			UiToolbox.MakeTex(2, 2, new Color(0, 0, 0, 0.6f));

		private static readonly Texture2D GreenTexture2D =
			UiToolbox.MakeTex(2, 2, new Color(0, 0.5f, 0, 0.75f));

		private static readonly Texture2D RedTexture2D =
			UiToolbox.MakeTex(2, 2, new Color(0.5f, 0, 0, 0.75f));

		private static readonly Texture2D GreenTexture2DHover =
			UiToolbox.MakeTex(2, 2, new Color(0, 0.60f, 0, 0.75f));

		private static readonly Texture2D RedTexture2DHover =
			UiToolbox.MakeTex(2, 2, new Color(0.60f, 0, 0, 0.75f));

		private static readonly Texture2D GreenTexture2DClick =
			UiToolbox.MakeTex(2, 2, new Color(0, 0.30f, 0, 0.75f));

		private static readonly Texture2D RedTexture2DClick =
			UiToolbox.MakeTex(2, 2, new Color(0.30f, 0, 0, 0.75f));

		private static readonly Texture2D GrayTexture2D =
			UiToolbox.MakeTex(2, 2, new Color32(100, 99, 95, 255));

		private static readonly Texture2D GrayTexture2DHover =
			UiToolbox.MakeTex(2, 2, new Color32(96, 95, 91, 255));

		private static readonly Texture2D GrayTexture2DClick =
			UiToolbox.MakeTex(2, 2, new Color32(46, 46, 43, 255));

		private static readonly Texture2D GrayTexture2DOn =
			UiToolbox.MakeTex(2, 2, new Color32(0, 99, 0, 255));

		private static readonly Texture2D GrayTexture2DOnHover =
			UiToolbox.MakeTex(2, 2, new Color32(0, 95, 0, 255));

		private static readonly Texture2D GrayTexture2DOnClick =
			UiToolbox.MakeTex(2, 2, new Color32(0, 46, 0, 255));

		public static GUIStyle MainWindow { get; private set; }
		public static GUIStyle LightSection { get; private set; }
		public static GUIStyle DarkerSection { get; private set; }

		public static GUIStyle RedButton { get; private set; }
		public static GUIStyle GreenButton { get; private set; }
		public static GUIStyle GrayButton { get; private set; }
		public static GUIStyle IllusionButton { get; private set; }

		public static GUIStyle Label { get; private set; }

		public static GUIStyle VerticalScrollbar { get; private set; }
		public static GUIStyle VerticalScrollbarThumb { get; private set; }

		public static GUIStyle HorizontalSlider { get; private set; }
		public static GUIStyle HorizontalSliderThumb { get; private set; }

		public static GUISkin CustomSkin { get; private set; }

		static Styles()
		{
			MakeMainWindow();
			MakeSections();
			MakeSpecialButtons();
			MakeScrollers();
			MakeMisc();
			CustomSkin = Object.Instantiate(GUISkin.current);
			CustomSkin.button = IllusionButton;
			CustomSkin.window = MainWindow;
			CustomSkin.verticalScrollbar = VerticalScrollbar;
			CustomSkin.verticalScrollbarThumb = VerticalScrollbarThumb;
			CustomSkin.horizontalSlider = HorizontalSlider;
			CustomSkin.horizontalSliderThumb = HorizontalSliderThumb;
			CustomSkin.label = Label;
		}

		private static void MakeMisc()
		{
			Label = new GUIStyle(GUI.skin.label)
			{
				fontSize = 16,
				fontStyle = FontStyle.Bold
			};
		}

		private static void MakeScrollers()
		{
			VerticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar)
			{
				alignment = TextAnchor.MiddleCenter,
				fixedWidth = 12,
				normal =
				{
					background = ScrollerVerticalBackground
				}
			};

			VerticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb)
			{
				fixedWidth = 12,
				normal =
				{
					background = ScrollerThumb
				}
			};

			HorizontalSlider = new GUIStyle(GUI.skin.horizontalScrollbar)
			{
				normal =
				{
					background = SliderHorizontalBackground
				}
			};
			HorizontalSliderThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb)
			{
				fixedWidth = 12,
				normal =
				{
					background = ScrollerThumb
				}
			};
		}

		private static Texture2D LoadTexture2D(string path)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var resource = assembly.GetManifestResourceNames()
				.FirstOrDefault(m => m.ToLower().EndsWith(path.ToLower()));

			// Load the resource stream
			using (var stream = assembly.GetManifestResourceStream(resource))
			{
				if (stream != null)
				{
					// Read the stream into a byte array
					var imageData = new byte[stream.Length];
					stream.Read(imageData, 0, (int)stream.Length);
					// Create a new Texture2D
					var newImageTexture2D = new Texture2D(2, 2);
					// Load the image data into the texture
					if (newImageTexture2D.LoadImage(imageData))
					{
						return newImageTexture2D;
					}
				}

				KineMod.PluginLogger.LogError($"Embedded resource not found or could not be loaded: {path}");
			}

			return Texture2D.blackTexture;
		}

		private static void MakeMainWindow()
		{
			MainWindow = new GUIStyle(GUI.skin.window)
			{
				fontStyle = FontStyle.Bold,
				fontSize = 16,
				normal =
				{
					background = WindowBackground,
					textColor = new Color(1, 1, 1, 1)
				},
				hover =
				{
					background = WindowBackground,
					textColor = new Color(1, 1, 1, 1)
				},
				onNormal =
				{
					background = WindowBackground,
					textColor = new Color(1, 1, 1, 1)
				},
				border =
				{
					top = 25
				},
				padding =
				{
					top = 30
				},
				contentOffset = new Vector2(5, -25)
			};
		}

		private static void MakeSections()
		{
			LightSection = new GUIStyle(GUI.skin.box)
			{
				normal =
				{
					background = LightSectionTex
				}
			};
			DarkerSection = new GUIStyle(GUI.skin.box)
			{
				normal =
				{
					background = DarkSectionTex
				}
			};
		}

		private static void MakeSpecialButtons()
		{
			GrayButton = new GUIStyle(GUI.skin.button)
			{
				fontStyle = FontStyle.Bold,
				normal =
				{
					background = GrayTexture2D
				},
				hover =
				{
					background = GrayTexture2DHover
				},
				active =
				{
					background = GrayTexture2DClick
				},
				onNormal =
				{
					background = GrayTexture2DOn
				},
				onHover =
				{
					background = GrayTexture2DOnHover
				},
				onActive =
				{
					background = GrayTexture2DOnClick
				},
			};
			IllusionButton = new GUIStyle(GUI.skin.button)
			{
				fontStyle = FontStyle.Bold,
				normal =
				{
					background = ButtonBackground
				},
				hover =
				{
					background = ButtonBackgroundHover
				},
				active =
				{
					background = ButtonBackgroundClick
				}
			};
			RedButton = new GUIStyle(GUI.skin.button)
			{
				fontStyle = FontStyle.Bold,
				normal =
				{
					background = RedTexture2D
				},
				hover =
				{
					background = RedTexture2DHover
				},
				active =
				{
					background = GreenTexture2DClick
				}
			};
			GreenButton = new GUIStyle(GUI.skin.button)
			{
				fontStyle = FontStyle.Bold,
				normal =
				{
					background = GreenTexture2D
				},
				hover =
				{
					background = GreenTexture2DHover
				},
				active =
				{
					background = RedTexture2DClick
				}
			};
		}
	}
}
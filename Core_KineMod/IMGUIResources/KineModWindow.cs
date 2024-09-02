using Studio;
using UnityEngine;

namespace Core_KineMod.IMGUIResources
{
	internal static class KineModWindow
	{
		//Done: Detect kinematic state changes and feed to toggle button.
		//Done: Detect BoneGroup changes and reflect in toggle button.
		//Done: Enforcement of custom bones but only when system is enabled.
		//Done: Node size slider
		//Done: Much later, add more granular control over node display. Probably hard as fuck.
		private static readonly Rect MRect = new Rect(330, 10, 250, 500);
		private static Vector2 _mScrollView;
		private static int _toolBarSelection;

		private static MPCharCtrl _mpCharCtrl;

		public static void DoADraw()
		{
			if (_mpCharCtrl == null)
			{
				_mpCharCtrl = Object.FindObjectOfType<MPCharCtrl>();
			}

			if (Studio.Studio.Instance.workInfo.visibleFlags[0] == false)
			{
				return;
			}

			GUI.skin = Styles.CustomSkin;
			GUILayout.Window(4321421, MRect, WindowFunction, "KineMod");
			GUI.skin = null;
		}

		private static void WindowFunction(int id)
		{
			var character = _mpCharCtrl.ociChar;

			_mScrollView = GUILayout.BeginScrollView(_mScrollView);
			_toolBarSelection = GUILayout.Toolbar(_toolBarSelection, new[] { "Main", "Effectors" }, Styles.GrayButton);

			if (_toolBarSelection == 0)
			{
				MainPage.Draw(_mpCharCtrl);
			}
			else if (_toolBarSelection == 1)
			{
				EffectorsPage.Draw(character);
			}

			GUILayout.EndScrollView();
			UiToolbox.EatInputInRect(MRect);
		}
	}
}
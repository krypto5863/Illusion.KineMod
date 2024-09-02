using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core_KineMod.UGUIResources
{
	public class ToggleSynchronizer : MonoBehaviour
	{
		private Toggle _toggle;
		private Func<bool> _checkFunc;
		private Action<bool> _onValueChanged;
		private bool _isSyncing;
		public static ToggleSynchronizer AddMonitor(Toggle toggle, Func<bool> onCheckFunc, Action<bool> onValueChangedAction)
		{
			var valueMonitor = toggle.gameObject.AddComponent<ToggleSynchronizer>();
			toggle.onValueChanged.AddListener(valueMonitor.OnToggleValueChanged);
			valueMonitor._toggle = toggle;
			valueMonitor._checkFunc = onCheckFunc;
			valueMonitor._onValueChanged = onValueChangedAction;
			return valueMonitor;
		}

		public void OnToggleValueChanged(bool value)
		{
			if (_isSyncing)
			{
				return;
			}

			_onValueChanged.Invoke(value);
		}

		public void Update()
		{
			var value = _checkFunc.Invoke();
			if (value == _toggle.isOn)
			{
				return;
			}

			_isSyncing = true;
			_toggle.isOn = value;
			_isSyncing = false;
		}
	}
}
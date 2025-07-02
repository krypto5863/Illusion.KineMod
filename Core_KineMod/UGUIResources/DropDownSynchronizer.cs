using System;
using TMPro;
using UnityEngine;

namespace Core_KineMod.UGUIResources
{
	public class DropDownSynchronizer : MonoBehaviour
	{
		private TMP_Dropdown _dropdown;
		private int _previousValue;
		private Func<int> _checkFunc;
		private Func<bool> _updateOptions;
		private Action<int> _onValueChanged;
		private bool _isSyncing;

		public static DropDownSynchronizer AddMonitor(TMP_Dropdown dropDown, Func<int> onCheckFunc, Action<int> onValueChangedAction, Func<bool> updateOptions)
		{
			var valueMonitor = dropDown.gameObject.AddComponent<DropDownSynchronizer>();
			dropDown.onValueChanged.AddListener(valueMonitor.OnDropdownValueChanged);
			valueMonitor._dropdown = dropDown;
			valueMonitor._checkFunc = onCheckFunc;
			valueMonitor._onValueChanged = onValueChangedAction;
			valueMonitor._updateOptions = updateOptions;
			return valueMonitor;
		}

		public void OnDropdownValueChanged(int value)
		{
			if (_isSyncing)
			{
				return;
			}

			//_dropdown.options[value].text, out var newValue);
			_onValueChanged.Invoke(value);
		}

		public void Update()
		{
			_updateOptions.Invoke();

			var value = _checkFunc.Invoke();
			if (value.Equals(_previousValue))
			{
				return;
			}

			_isSyncing = true;
			_dropdown.value = value;
			_dropdown.RefreshShownValue();
			_previousValue = value;
			_isSyncing = false;
		}
	}
}
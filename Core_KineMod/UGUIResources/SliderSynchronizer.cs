using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core_KineMod.UGUIResources
{
	public class SliderSynchronizer : MonoBehaviour
	{
		private Slider _slider;
		private Func<float> _checkFunc;
		private Action<float> _onValueChanged;
		private bool _isSyncing;
		public static SliderSynchronizer AddMonitor(Slider slider, Func<float> onCheckFunc, Action<float> onValueChangedAction)
		{
			var valueMonitor = slider.gameObject.AddComponent<SliderSynchronizer>();
			slider.onValueChanged.AddListener(valueMonitor.OnSliderValueChanged);
			valueMonitor._slider = slider;
			valueMonitor._checkFunc = onCheckFunc;
			valueMonitor._onValueChanged = onValueChangedAction;
			return valueMonitor;
		}

		public void OnSliderValueChanged(float value)
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
			if (!(Math.Abs(_checkFunc.Invoke() - _slider.value) > 0.001))
			{
				return;
			}
			_isSyncing = true;
			_slider.value = value;
			_isSyncing = false;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIValueController : MonoBehaviour
{
    [Header("Bindings")]
    public Text labelField;
    public InputField valueText;
    public Slider valueSlider;
    public Slider nudgeSlider;

    [Header("Parameters")]
    public string label;
    public float minValue;
    public float maxValue;
    public float initialValue;
    public bool wholeNumbers;

    public float CurrentValue => valueSlider.value;
    public event Action ValueChanged;
    
    // Start is called before the first frame update
    void Start()
    {
        _defaultInputTextColor = valueText.textComponent.color;

        labelField.text = label;
        valueSlider.minValue = minValue;
        valueSlider.maxValue = maxValue;
        valueSlider.wholeNumbers = wholeNumbers;
        _nudgeSliderScale = minValue * 2;
        nudgeSlider.minValue = -1;
        nudgeSlider.maxValue = 1;

        valueSlider.value = initialValue;
        valueText.text = valueSlider.value.ToString(wholeNumbers ? IntFormat : FloatFormat);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isNudging && !Input.GetMouseButton(0))
        {
            _isNudging = false;
            nudgeSlider.value = 0;
        }
    }


    #region NudgeScale

    public double NudgeScale
    {
        get { return _NudgeScale; }
        set
        {
            if (_NudgeScale == value) return;
            _NudgeScale = value;

            _nudgeSliderScale = Math.Min(1.0, value) * 2;
        }
    }
    private double _NudgeScale;

    #endregion

    public void OnTextValueChanged()
    {
        if (EventSystem.current.currentSelectedGameObject == valueText.gameObject)
        {
            if (float.TryParse(valueText.text, out float value))
            {
                valueSlider.value = value;
                valueText.textComponent.color = _defaultInputTextColor;
            }
            else
            {
                valueText.textComponent.color = Color.red;
            }
        }
        else
        {
            valueText.textComponent.color = _defaultInputTextColor;
        }
    }

    public void OnSliderValueChanged()
    {
        if(EventSystem.current.currentSelectedGameObject == valueSlider.gameObject)
            valueText.text = valueSlider.value.ToString(wholeNumbers ? IntFormat : FloatFormat);

        ValueChanged?.Invoke();
    }

    public void OnNudgeValueChanged()
    {
        if (nudgeSlider.value == 0) return;

        if (!_isNudging)
        {
            _isNudging = true;
            _nudgeStartingPoint = valueSlider.value;
        }

        var newValue = _nudgeStartingPoint + nudgeSlider.value * _nudgeSliderScale;
        valueSlider.value = (float)newValue;
        valueText.text = newValue.ToString(wholeNumbers ? IntFormat : FloatFormat);
    }

    private static readonly string FloatFormat = "N9";
    private static readonly string IntFormat = "0";
    private bool _isNudging;
    private float _nudgeStartingPoint;
    private double _nudgeSliderScale;
    public Color _defaultInputTextColor;
}

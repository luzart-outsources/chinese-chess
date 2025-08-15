using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Slider))]
public class SnapSlider : MonoBehaviour
{
    public List<float> fixedValues = new List<float> { 1f, 2f, 5f, 10f, 20f };
    public TextMeshProUGUI valueText;

    private Slider slider;
    private bool isDragging = false;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // Setup slider range based on fixed values
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.wholeNumbers = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            float nearest = GetNearestValue(slider.value);
            UpdateText(nearest);
        }
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;

        float nearest = GetNearestValue(slider.value);
        slider.SetValueWithoutNotify(ValueToSlider(nearest));
        UpdateText(nearest);
    }

    float GetNearestValue(float sliderValue)
    {
        float rawValue = SliderToValue(sliderValue);
        float nearest = fixedValues[0];
        float minDist = Mathf.Abs(rawValue - nearest);

        foreach (float val in fixedValues)
        {
            float dist = Mathf.Abs(rawValue - val);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = val;
            }
        }
        return nearest;
    }

    float SliderToValue(float sliderValue)
    {
        // Map slider 0-1 to fixed range
        return Mathf.Lerp(fixedValues[0], fixedValues[fixedValues.Count - 1], sliderValue);
    }

    float ValueToSlider(float value)
    {
        return Mathf.InverseLerp(fixedValues[0], fixedValues[fixedValues.Count - 1], value);
    }

    void UpdateText(float value)
    {
        if (valueText != null)
        {
            valueText.text = value.ToString("0.000");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ColorPickerController : MonoBehaviour
{
    public Slider RedSlider;
    public Slider GreenSlider;
    public Slider BlueSlider;
    public Slider MarkerSizeSlider;
    public Image ColorImage;
    public Image PickerHider;
    public Text redValue;
    public Text blueValue;
    public Text greenValue;
    public Text markerSizeValue;

    Color color = Color.black;
    int markerSize = 2;

    private bool pickerVisible = true;
    private Vector3 pickerOffset = new Vector3(0, 182, 0);
    private Vector3 rot = new Vector3(0, 0, 180);

    public Action<Color> OnColorChanged;
    public Action OnHiderClicked;
    public Action<int> OnMarkerSizeChanged;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(RedSlider != null);
        Debug.Assert(GreenSlider != null);
        Debug.Assert(BlueSlider != null);
        Debug.Assert(MarkerSizeSlider != null);
        Debug.Assert(ColorImage != null);
        Debug.Assert(redValue != null);
        Debug.Assert(blueValue != null);
        Debug.Assert(greenValue != null);
        Debug.Assert(markerSizeValue != null);

        RedSlider.onValueChanged.AddListener(delegate { RedSliderChanged(); });
        GreenSlider.onValueChanged.AddListener(delegate { GreenSliderChanged(); });
        BlueSlider.onValueChanged.AddListener(delegate { BlueSliderChanged(); });
        MarkerSizeSlider.onValueChanged.AddListener(delegate { MarkerSliderChanged(); });
    }
    public void RedSliderChanged()
    {
        redValue.text = RedSlider.value.ToString();
        color.r = (RedSlider.value / 255);
        ColorImage.color = color;
        colorChanged();
    }

    public void GreenSliderChanged()
    {
        greenValue.text = GreenSlider.value.ToString();
        color.g = (GreenSlider.value / 255);
        ColorImage.color = color;
        colorChanged();
    }

    public void BlueSliderChanged()
    {
        blueValue.text = BlueSlider.value.ToString();
        color.b = (BlueSlider.value / 255);
        ColorImage.color = color;
        colorChanged();
        
    }

    private void colorChanged()
    {
        OnColorChanged?.Invoke(color);
    }

    public void MarkerSliderChanged()
    {
        markerSizeValue.text = MarkerSizeSlider.value.ToString();
        markerSize = (int)MarkerSizeSlider.value;
        OnMarkerSizeChanged?.Invoke(markerSize);
    }

    public Color GetColor()
    {
        return color;
    }

    public void SetColor(Color newColor)
    {
        RedSlider.SetValueWithoutNotify((int)newColor.r*255);
        BlueSlider.SetValueWithoutNotify((int)newColor.b*255);
        GreenSlider.SetValueWithoutNotify((int)newColor.g*255);
        color = newColor;
        ColorImage.color = color;
    }

    public int getMarkerSize()
    {
        return markerSize;
    }

    public void setMarkerSize(int val)
    {
        //Dostuff
        MarkerSizeSlider.SetValueWithoutNotify(val);
        markerSize = val;
    }

    public void hiderClicked()
    {
        //PickerHider.transform.rotation.SetEulerAngles(0, 0, 180);
        var rotation = Quaternion.Euler(rot);
        PickerHider.transform.rotation = rotation * PickerHider.transform.rotation;
        transform.localPosition = pickerVisible ? transform.localPosition - pickerOffset : transform.localPosition + pickerOffset;
        pickerVisible = !pickerVisible;
        //OnHiderClicked.Invoke();
    }
}

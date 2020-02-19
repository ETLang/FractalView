
using FractalView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerMain : MonoBehaviour
{
    public Text magnificationText;
    public UIValueController crController;
    public UIValueController ciController;
    public UIValueController mjController;
    public UIValueController maxIController;
    public Slider gradientSpreadSlider;
    public Toggle thumbprintToggle;
    public Toggle burningShipToggle;
    public Toggle showVelocityToggle;
    public Toggle showMagnitudeToggle;
    public GameObject publishPanel;
    public float nudgeSpeed = 1;

    public TheEffect effect;
    private Fractal _fractal;
    private Colorizer _colorizer;

    // Start is called before the first frame update
    void Start()
    {
        _fractal = effect.Fractal;
        _colorizer = _fractal.Colorizer;

        crController.ValueChanged += CrController_ValueChanged;
        ciController.ValueChanged += CiController_ValueChanged;
        mjController.ValueChanged += MjController_ValueChanged;
        maxIController.ValueChanged += MaxIController_ValueChanged;

        gradientSpreadSlider.value = _colorizer.SpeedGradientSpread;
    }

    private void MaxIController_ValueChanged()
    {
        _fractal.MaxIterations = (int)maxIController.CurrentValue;
    }

    private void MjController_ValueChanged()
    {
        _fractal.Mandulia = mjController.CurrentValue;
    }

    private void CiController_ValueChanged()
    {
        _fractal.C = new double2(_fractal.C.x, ciController.CurrentValue);
    }

    private void CrController_ValueChanged()
    {
        _fractal.C = new double2(crController.CurrentValue, _fractal.C.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (_fractal == null) return;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            var mag = _fractal.ViewScale;

            if (_baseMag == 0)
                _baseMag = mag;

            var newNudgeScale = mag * nudgeSpeed;
            if (newNudgeScale != _lastNudgeScale)
            {
                magnificationText.text = $"Magnification: {(_baseMag / mag):N0}X";

                crController.NudgeScale = newNudgeScale;
                ciController.NudgeScale = newNudgeScale;
                mjController.NudgeScale = newNudgeScale;

                _lastNudgeScale = newNudgeScale;
            }
        }
    }

    public void TypeToggleValueChanged()
    {
        if (_fractal.AbsMod == burningShipToggle.isOn)
            return;

        _fractal.AbsMod = burningShipToggle.isOn;
    }

    public void DisplayToggleValueChanged()
    {
        _colorizer.DisplayMagnitude = showMagnitudeToggle.isOn;
        _colorizer.DisplaySpeed = showVelocityToggle.isOn;
    }

    public void OnGradientSpreadChanged()
    {
        _colorizer.SpeedGradientSpread = gradientSpreadSlider.value;
    }

    public void OnNudgeSpeedChange(float newSpeed)
    {
        nudgeSpeed = newSpeed;
    }

    public void SetAsBackground()
    {
        
        //UserProfilePersonalizationSettings
    }

    public void Save()
    {
        const int bpc = 8;

        UnityEngine.Debug.Log("Saving...");

        int w = 3840;
        int h = 2160;

        var photo = effect.RenderPhoto(w, h, SuperSampling._4x4, bpc);
        var photoBytes = photo.EncodeToPNG();

        var fileOut = Help.ChooseSaveFile("png", new string[] { "*.png" });
        if (fileOut == null)
            return;

#if UNITY_EDITOR

        File.WriteAllBytes(fileOut, photoBytes);
        //System.Windows.Forms
        //File.WriteAllBytes(@"C:\Work\Scratch\FractalView\TiffWriter.tmp", photoBytes);

        //var info = new ProcessStartInfo
        //{
        //    FileName = @"C:\Work\Scratch\FractalView\TiffWriter.exe",
        //    Arguments = $"{w} {h} {dpi} {bpc}",
        //    CreateNoWindow = true,
        //    UseShellExecute = false
        //};

        //var process = Process.Start(info);
        //process.WaitForExit();


#elif UNITY_WEBGL
        Help.OfferToDownload(photoBytes, fileOut);
#endif
    }

    public void Publish()
    {
        publishPanel.SetActive(true);
    }

    private Camera _mainCamera;
    private double _baseMag = 0;
    private double _lastNudgeScale = 0;
}

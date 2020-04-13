
using FractalView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerMain : MonoBehaviour
{
    public static UIControllerMain Instance { get; private set; }

    public Text magnificationText;
    public UIValueController crController;
    public UIValueController ciController;
    public UIValueController mjController;
    public UIValueController maxIController;
    public UICBookmarkList bookmarkListController;
    public Slider gradientSpreadSlider;
    public Toggle thumbprintToggle;
    public Toggle burningShipToggle;
    public Toggle showVelocityToggle;
    public Toggle showMagnitudeToggle;
    public GameObject publishPanel;
    public GameObject editBookmarkPanel;
    public float nudgeSpeed = 1;

    public TheEffect effect;
    private Colorizer _colorizer;

    public BookmarkCollection Bookmarks { get; private set; }
    public Bookmark SelectedBookmark { get; private set; }
    public Fractal Fractal { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        Bookmarks = BookmarkCollection.Instance;
        Fractal = new Fractal();
        _colorizer = Fractal.Colorizer;

        crController.ValueChanged += CrController_ValueChanged;
        ciController.ValueChanged += CiController_ValueChanged;
        mjController.ValueChanged += MjController_ValueChanged;
        maxIController.ValueChanged += MaxIController_ValueChanged;

        gradientSpreadSlider.value = _colorizer.SpeedGradientSpread;
    }

    private void MaxIController_ValueChanged()
    {
        Fractal.MaxIterations = (int)maxIController.CurrentValue;
    }

    private void MjController_ValueChanged()
    {
        Fractal.Mandulia = mjController.CurrentValue;
    }

    private void CiController_ValueChanged()
    {
        Fractal.C = new double2(Fractal.C.x, ciController.CurrentValue);
    }

    private void CrController_ValueChanged()
    {
        Fractal.C = new double2(crController.CurrentValue, Fractal.C.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (Fractal == null) return;

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera != null)
        {
            var mag = Fractal.ViewScale;

            if (_baseMag == 0)
                _baseMag = mag;

            var newNudgeScale = mag * nudgeSpeed;
            if (newNudgeScale != _lastNudgeScale)
            {
                magnificationText.text = $"Zoom: {(_baseMag / mag):N0}X";

                crController.NudgeScale = newNudgeScale;
                ciController.NudgeScale = newNudgeScale;
                mjController.NudgeScale = newNudgeScale;

                _lastNudgeScale = newNudgeScale;
            }
        }
    }

    public void TypeToggleValueChanged()
    {
        if (Fractal.AbsMod == burningShipToggle.isOn)
            return;

        Fractal.AbsMod = burningShipToggle.isOn;
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

    public void CreateBookmark()
    {
        editBookmarkPanel.SetActive(true);
    }

    public void EditBookmark()
    {
        editBookmarkPanel.SetActive(true);
    }

    public void DeleteBookmark()
    {
        Debug.Log("Delete Bookmark!");
    }

    #region Private

    private Camera _mainCamera;
    private double _baseMag = 0;
    private double _lastNudgeScale = 0;

    #endregion
}

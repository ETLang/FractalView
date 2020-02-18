using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIPublishController : MonoBehaviour
{
    public TheEffect effect;

    [Header("Bindings")]
    public InputField dpiTextBox;
    public InputField widthTextBox;
    public InputField heightTextBox;
    public Toggle cropToggle;
    public Toggle expandToggle;
    public Toggle pixelsToggle;
    public Toggle inchesToggle;

    [Header("Parameters")]
    public int dpi;
    public float width;
    public float height;
    public bool useInches;
    public bool dither;
    public bool cropVisible;
    public int superSample;
    public bool highPrecision;

    // Start is called before the first frame update
    void Start()
    {
        _defaultTextColor = dpiTextBox.textComponent.color;

        dpiTextBox.text = dpi.ToString();
        widthTextBox.text = width.ToString();
        heightTextBox.text = height.ToString();
        inchesToggle.isOn = useInches;
        cropToggle.isOn = cropVisible;
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    public void Publish()
    {
        const int bpc = 32;

        UnityEngine.Debug.Log("Publishing...");

        int w = useInches ? (int)(dpi * width) : (int)width;
        int h = useInches ? (int)(dpi * height) : (int)height;

        var photo = effect.RenderPhoto(w, h, SuperSampling._4x4, bpc);

        var photoBytes = photo.GetBytes();
#if UNITY_EDITOR
        File.WriteAllBytes(@"C:\Work\Scratch\FractalView\TiffWriter.tmp", photoBytes);

        var info = new ProcessStartInfo
        {
            FileName = @"C:\Work\Scratch\FractalView\TiffWriter.exe",
            Arguments = $"{w} {h} {dpi} {bpc}",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        var process = Process.Start(info);
        process.WaitForExit();


#elif UNITY_WEBGL
        // TODO Encode TIFF
        //Help.OfferToDownload(tiffData, "Fractal.tiff");
       
        Help.OfferToDownload(UTF8Encoding.Default.GetBytes("Successfully downloaded silly text file!"), "Test.txt");
#endif

        gameObject.SetActive(false);
    }

    public void OnFitMethodChanged()
    {
        cropVisible = cropToggle.isOn;
    }

    public void OnUnitsChanged()
    {
        var newUseInches = inchesToggle.isOn;

        if(newUseInches != useInches)
        {
            if(newUseInches)
            {
                width /= dpi;
                height /= dpi;

                widthTextBox.text = width.ToString();
                heightTextBox.text = height.ToString();
            }
            else
            {
                width *= dpi;
                height *= dpi;

                widthTextBox.text = width.ToString();
                heightTextBox.text = height.ToString();
            }
        }

        useInches = inchesToggle.isOn;
    }

    public void OnDpiChanged()
    {
        if (float.TryParse(dpiTextBox.text, out float newdpi))
        {
            dpi = (int)newdpi;
            dpiTextBox.textComponent.color = _defaultTextColor;
        }
        else
            dpiTextBox.textComponent.color = Color.red;
    }

    public void OnWidthChanged()
    {
        if (float.TryParse(widthTextBox.text, out float newwidth))
        {
            width = newwidth;
            widthTextBox.textComponent.color = _defaultTextColor;
        }
        else
            widthTextBox.textComponent.color = Color.red;
    }

    public void OnHeightChanged()
    {
        if (float.TryParse(heightTextBox.text, out float newheight))
        {
            height = newheight;
            heightTextBox.textComponent.color = _defaultTextColor;
        }
        else
            heightTextBox.textComponent.color = Color.red;
    }

    private Color _defaultTextColor;
}

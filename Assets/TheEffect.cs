using FractalView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public enum FractalRenderMode
{
    PrecisionThreshold,
    Mixed,
    GPUOnly,
    CPUOnly
}

public enum SuperSampling
{
    None,
    _2x2,
    _4x4,
    _8x8
}

[ExecuteInEditMode]
public class TheEffect : MonoBehaviour
{
    public Fractal Fractal { get; private set; }

    public FractalRenderMode renderMode = FractalRenderMode.PrecisionThreshold;
    public double lodBias = 0;

    public Camera saveCam;
    public Texture gradient;
    public float tileUpdatesPerFrame;
    public int tileUpdatesLastFrame;

    public bool isMoving = false;

    public RenderTexture RenderPhoto(int w, int h, SuperSampling ss, int bpc)
    {
        int bias = (int)ss;

        var canvasFormat = (bpc == 8) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGBFloat;
        var canvasTexture = new RenderTexture(new RenderTextureDescriptor(w, h, canvasFormat));
        canvasTexture.Create();

        // Set up a special camera to capture the photo. Same settings as main camera, but different target.
        //var cam = new Camera();
        //cam.orthographicSize = 2;
        saveCam.targetTexture = canvasTexture;
        saveCam.projectionMatrix = _cam.projectionMatrix;
        //_renderer.PurgeCache
        var saveProj = saveCam.projectionMatrix;
        var camProj = _cam.projectionMatrix;

        PublishRenderer.Render(Fractal, saveCam, _pool, 3);
        GL.Flush();
        saveCam.targetTexture = null;
        GL.InvalidateState();

        return canvasTexture;
    }

    void Start()
    {
        Fractal = new Fractal();
        Fractal.Colorizer.SpeedGradient = gradient;
        _pool = new FractalBufferPool();
        _cam = GetComponentInParent<Camera>();
        _renderer = new LiveRenderer(Fractal, _pool);
        transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

        //_cam.transform.localScale = new Vector3()
    }

    private void Update()
    {
        if (_cam == null || _pool == null) return;

        _cam.orthographicSize = _cam.pixelHeight / (2 * _pool.TileResolution);
    }

    public void OnPostRender()
    {
        if (_renderer == null)
            return;

        //PublishRenderer.Render(Fractal, _cam, _pool, (int)lodBias);
        _renderer.Render(_cam, lodBias - (isMoving ? 1 : 0));
        tileUpdatesPerFrame = _renderer.tileUpdatesPerFrame;
        tileUpdatesLastFrame = _renderer.tileUpdatesLastFrame;
    }

    Camera _cam;
    FractalBufferPool _pool;
    LiveRenderer _renderer;
}
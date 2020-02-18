using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Help
{
    public static byte[] GetBytes(this RenderTexture rt)
    {
        RenderTexture currentActive = RenderTexture.active;
        RenderTexture.active = rt;

        TextureFormat fmt = TextureFormat.ARGB32;
        switch(rt.format)
        {
            case RenderTextureFormat.ARGB32:
                fmt = TextureFormat.RGBA32;
                break;
            case RenderTextureFormat.ARGBFloat:
                fmt = TextureFormat.RGBAFloat;
                break;
        }
        Texture2D tex = new Texture2D(rt.width, rt.height, fmt, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        return tex.GetRawTextureData();
    }

    public static byte[] EncodeToPNG(this RenderTexture rt)
    {
        RenderTexture currentActive = RenderTexture.active;
        RenderTexture.active = rt;

        TextureFormat fmt = TextureFormat.ARGB32;
        switch (rt.format)
        {
            case RenderTextureFormat.ARGB32:
                fmt = TextureFormat.RGBA32;
                break;
            case RenderTextureFormat.ARGBFloat:
                fmt = TextureFormat.RGBAFloat;
                break;
        }
        Texture2D tex = new Texture2D(rt.width, rt.height, fmt, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        return tex.EncodeToPNG();
    }

    public static Bounds GetViewBoundingBox(Matrix4x4 viewprojMatrix)
    {
        var camMat = viewprojMatrix.inverse;
        Bounds bb = new Bounds();
        for (int i = 0; i < _CamCorners.Length; i++)
            bb.Encapsulate(camMat * _CamCorners[i]);
        return bb;
    }

    public static Bounds GetViewBoundingBox(Camera cam)
    {
        return GetViewBoundingBox(cam.transform.worldToLocalMatrix * cam.projectionMatrix);
    }

    public static Bounds GetViewBoundingBox(RenderTexture tex)
    {
        var ar = (float)tex.width / tex.height;
        var camMat = Matrix4x4.Scale(new Vector3(ar, 1, 1));
        Bounds bb = new Bounds();
        for (int i = 0; i < _CamCorners.Length; i++)
            bb.Encapsulate(camMat * _CamCorners[i]);
        return bb;
    }

    public static void RenderRectNow(RenderTexture target, Material mat, Matrix4x4 transform)
    {
        if (_Quad == null)
            InitQuad();

        var oldTarget = RenderTexture.active;
        RenderTexture.active = target;
        mat.SetPass(0);
        Graphics.DrawMeshNow(_Quad, transform);
        RenderTexture.active = oldTarget;
    }

    public static void RenderRectNow(RenderTexture target, Material mat, Rect viewport)
    {
        if (_Quad == null)
            InitQuad();

        var oldTarget = RenderTexture.active;
        RenderTexture.active = target;
        mat.SetPass(0);
        GL.Viewport(viewport);
        Graphics.DrawMeshNow(_Quad, Matrix4x4.identity);
        RenderTexture.active = oldTarget;

        if(oldTarget != null)
            GL.Viewport(new Rect(0, 0, oldTarget.width, oldTarget.height));
    }

    public static void BlitNow(Texture source, RenderTexture target, Material mat)
    {
        mat.mainTexture = source;

        GL.PushMatrix();
        GL.LoadOrtho();
        RenderRectNow(target, mat, Matrix4x4.identity);
        GL.PopMatrix();
    }

    public static void BlitNow(Texture source, RenderTexture target)
    {
        if (_BlitMat == null)
        {
            _BlitMat = new Material(Shader.Find("Hidden/BlitShader"));
            _BlitMat.SetVector(_FilterId, Vector4.one);
        }

        BlitNow(source, target, _BlitMat);
    }

    public static void Clear(RenderTexture target, Color c)
    {
        var old = RenderTexture.active;
        RenderTexture.active = target;
        GL.Clear(false, true, c);
        RenderTexture.active = old;
    }

    public static void Mark(RenderTexture target, Color c)
    {
        var old = RenderTexture.active;
        RenderTexture.active = target;
        GL.Viewport(new Rect(4, 4, target.width - 8, target.height - 8));
        GL.Clear(false, true, c);
        RenderTexture.active = old;
    }

#if UNITY_WEBGL
    public static void OfferToDownload(byte[] data, string name)
    {
        if (data != null)
        {
            Debug.Log("Offering to Download..." + name);
            ImageDownloader(System.Convert.ToBase64String(data), name);
        }
    }
#endif

    #region Private

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void ImageDownloader(string str, string fn);
#endif

    static Help()
    {
        _CamCorners = new Vector3[]
        {
                new Vector3(-1,-1,0),
                new Vector3(1,-1,0),
                new Vector3(1,1,0),
                new Vector3(-1,1,0)
        };
    }

    static void InitQuad()
    {
        _Quad = new Mesh();
        _Quad.vertices = new Vector3[]
        {
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(0,0,0),
                new Vector3(1,0,0)
        };
        _Quad.SetIndices(new int[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0);
        _Quad.RecalculateBounds();
        _Quad.UploadMeshData(true);
    }

    private static Vector3[] _CamCorners;
    private static Mesh _Quad;
    private static Material _BlitMat;
    static int _FilterId = Shader.PropertyToID("_Filter");

    #endregion
}
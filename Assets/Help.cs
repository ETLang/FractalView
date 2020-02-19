using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Help
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public unsafe struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public byte* lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }

    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetSaveFileName(ref OpenFileName lpofn);
#endif

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

    public static byte[] EncodeToTIFF(this RenderTexture rt, int dpi)
    {
        if (rt.format != RenderTextureFormat.ARGBFloat)
            throw new Exception("EncodeToTIFF only works with ARGBFloat textures right now");

        var pixelData = GetBytes(rt);

        var stream = new TiffMemoryStream();

        using (var tiff = Tiff.ClientOpen("Fractal", "w", null, stream))
        {
            int width = rt.width;
            int height = rt.height;

            tiff.SetField(TiffTag.SOFTWARE, Application.productName + ", Copyright (C) 2020, Evan Lang");

            tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            tiff.SetField(TiffTag.IMAGEWIDTH, width);
            tiff.SetField(TiffTag.IMAGELENGTH, height);
            tiff.SetField(TiffTag.SAMPLESPERPIXEL, 3);
            tiff.SetField(TiffTag.BITSPERSAMPLE, 16, 16, 16);  // 32?
            tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
            tiff.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
            tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

            tiff.SetField(TiffTag.XRESOLUTION, dpi);
            tiff.SetField(TiffTag.YRESOLUTION, dpi);
            tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);

            int pixel_count = width * height;

            if (pixel_count == 0 || (pixel_count / width) != height)
                throw new Exception($"Invalid image size: {width}x{height}");

            int m_rowsPerStrip = tiff.DefaultStripSize(0);
            tiff.SetField(TiffTag.ROWSPERSTRIP, m_rowsPerStrip);

            // Convert float to ushort. The output will be 48-bit RGB color.
            float[] srcPixels = new float[pixel_count * 4];
            Buffer.BlockCopy(pixelData, 0, srcPixels, 0, pixelData.Length);

            ushort[] dstPixels = new ushort[pixel_count * 3];
            for (int i = 0; i < pixel_count; i++)
            {
                dstPixels[3 * i + 0] = (ushort)(Mathf.Clamp(srcPixels[4 * i + 0], 0, 1) * ushort.MaxValue);
                dstPixels[3 * i + 1] = (ushort)(Mathf.Clamp(srcPixels[4 * i + 1], 0, 1) * ushort.MaxValue);
                dstPixels[3 * i + 2] = (ushort)(Mathf.Clamp(srcPixels[4 * i + 2], 0, 1) * ushort.MaxValue);
            }

            byte[] dstBuffer = new byte[dstPixels.Length * sizeof(ushort)];
            Buffer.BlockCopy(dstPixels, 0, dstBuffer, 0, dstBuffer.Length);

            /*
             * Write out the result in strips
             */
            var bytes_per_pixel = 6;
            for (int row = 0; row < height; row += m_rowsPerStrip)
            {
                int rows_to_write;
                if (row + m_rowsPerStrip > height)
                    rows_to_write = height - row;
                else
                    rows_to_write = m_rowsPerStrip;

                int offset = bytes_per_pixel * row * width;
                int count = bytes_per_pixel * rows_to_write * width;
                if (tiff.WriteEncodedStrip(row / m_rowsPerStrip, dstBuffer, offset, count) == -1)
                    return null;
            }

            tiff.WriteDirectory();
            //tiff.Close();
        }

        return stream.BaseStream.GetBuffer();
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

    public static unsafe string ChooseSaveFile(string defaultExt, string[] exts)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.SaveFilePanel("Save Fractal", null, null, defaultExt);

#elif UNITY_STANDALONE_WIN
        var nameBuffer = new byte[1024];

        var cd = Environment.CurrentDirectory;
        string ret = null;

        fixed (byte* nameBufferPtr = nameBuffer)
        {
            var ofn = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                hwndOwner = IntPtr.Zero,
                hInstance = IntPtr.Zero,
                lpstrFilter = string.Join(";", exts),
                lpstrCustomFilter = null,
                nMaxCustFilter = 0,
                nFilterIndex = 0,
                lpstrFile = nameBufferPtr,
                nMaxFile = nameBuffer.Length,
                lpstrFileTitle = null,
                nMaxFileTitle = 0,
                lpstrInitialDir = null,
                lpstrTitle = "Save Fractal",
                Flags = 2, // just overwrite prompt
                nFileOffset = 0,
                nFileExtension = 0,
                lpstrDefExt = defaultExt,
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = null,
                dwReserved = 0,
                pvReserved = IntPtr.Zero,
                flagsEx = 0
            };

            if (GetSaveFileName(ref ofn))
                ret = Encoding.UTF8.GetString(nameBuffer);

            Environment.CurrentDirectory = cd;
        }

        return ret;
        
#elif UNITY_WEBGL
        return "Fractal." + defaultExt;
#endif
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

    private class TiffMemoryStream : TiffStream
    {
        public MemoryStream BaseStream { get; } = new MemoryStream();

        public override int Read(object clientData, byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(object clientData, byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public override long Seek(object clientData, long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override long Size(object clientData)
        {
            return BaseStream.Length;
        }

        public override void Close(object clientData)
        {
            BaseStream.Close();
        }
    }

    #endregion
}
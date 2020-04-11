using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FractalView
{
    public class Colorizer
    {
        public Colorizer()
        {
            Material = new Material(Shader.Find("Hidden/ColorizerShader"));

            DisplaySpeed = true;
            SpeedGradientSpread = MagnitudeGradientSpread = 4;
        }

        public Colorizer(Colorizer copy)
        {
            Material = UnityEngine.Object.Instantiate(copy.Material);

            Material.CopyPropertiesFromMaterial(copy.Material);
            FlipVertical = copy.FlipVertical;
            SwapRedBlue = copy.SwapRedBlue;
            ConvertSRGB = copy.ConvertSRGB;
            DisplaySpeed = copy.DisplaySpeed;
            DisplayMagnitude = copy.DisplayMagnitude;
            SpeedGradient = copy.SpeedGradient;
            MagnitudeGradient = copy.MagnitudeGradient;
            SpeedGradientSpread = copy.SpeedGradientSpread;
            MagnitudeGradientSpread = copy._MagnitudeGradientSpread;
            MaxIterations = copy.MaxIterations;
        }

        public void Colorize(Texture fractal, RenderTexture dest)
        {
            Help.BlitNow(fractal, dest, Material);
        }

        public event Action Changed;

        public Material Material { get; private set; }

        #region FlipVertical

        public bool FlipVertical
        {
            get { return _FlipVertical; }
            set
            {
                if (_FlipVertical == value) return;
                _FlipVertical = value;

                if(value)
                    Material.EnableKeyword("FLIP_VERTICAL");
                else
                    Material.DisableKeyword("FLIP_VERTICAL");
                Changed?.Invoke();
            }
        }
        private bool _FlipVertical;

        #endregion

        #region SwapRedBlue

        public bool SwapRedBlue
        {
            get { return _SwapRedBlue; }
            set
            {
                if (_SwapRedBlue == value) return;
                _SwapRedBlue = value;

                if(value)
                    Material.EnableKeyword("CONVERT_SRGB");
                else
                    Material.DisableKeyword("CONVERT_SRGB");
                Changed?.Invoke();
            }
        }
        private bool _SwapRedBlue;

        #endregion

        #region ConvertSRGB

        public bool ConvertSRGB
        {
            get { return _ConvertSRGB; }
            set
            {
                if (_ConvertSRGB == value) return;
                _ConvertSRGB = value;

                if(value)
                    Material.EnableKeyword("CONVERT_SRGB");
                else
                    Material.DisableKeyword("CONVERT_SRGB");
                Changed?.Invoke();
            }
        }
        private bool _ConvertSRGB;

        #endregion

        #region DisplaySpeed

        public bool DisplaySpeed
        {
            get { return _DisplaySpeed; }
            set
            {
                if (_DisplaySpeed == value) return;
                _DisplaySpeed = value;

                if (value)
                    Material.EnableKeyword("VIZ_VEL");
                else
                    Material.DisableKeyword("VIZ_VEL");
                Changed?.Invoke();
            }
        }
        private bool _DisplaySpeed = false;

        #endregion

        #region DisplayMagnitude

        public bool DisplayMagnitude
        {
            get { return _DisplayMagnitude; }
            set
            {
                if (_DisplayMagnitude == value) return;
                _DisplayMagnitude = value;

                if (value)
                    Material.EnableKeyword("VIZ_MAG");
                else
                    Material.DisableKeyword("VIZ_MAG");
                Changed?.Invoke();
            }
        }
        private bool _DisplayMagnitude = false;

        #endregion

        #region SpeedGradient

        public Texture SpeedGradient
        {
            get { return _SpeedGradient; }
            set
            {
                if (_SpeedGradient == value) return;
                _SpeedGradient = value;

                Material.SetTexture(_GradientTexId, value);
                Changed?.Invoke();
            }
        }
        private Texture _SpeedGradient;

        #endregion

        #region MagnitudeGradient

        public Texture MagnitudeGradient
        {
            get { return _MagnitudeGradient; }
            set
            {
                if (_MagnitudeGradient == value) return;
                _MagnitudeGradient = value;

                // TODO Independent Magnitude gradient
                Changed?.Invoke();
            }
        }
        private Texture _MagnitudeGradient;

        #endregion

        #region SpeedGradientSpread

        public float SpeedGradientSpread
        {
            get { return _SpeedGradientSpread; }
            set
            {
                if (_SpeedGradientSpread == value) return;
                _SpeedGradientSpread = value;

                Material.SetFloat(_GradientPullId, value);
                Changed?.Invoke();
            }
        }
        private float _SpeedGradientSpread;

        #endregion

        #region MagnitudeGradientSpread

        public float MagnitudeGradientSpread
        {
            get { return _MagnitudeGradientSpread; }
            set
            {
                if (_MagnitudeGradientSpread == value) return;
                _MagnitudeGradientSpread = value;

                // TODO
                Changed?.Invoke();
            }
        }
        private float _MagnitudeGradientSpread;

        #endregion

        #region MaxIterations

        public int MaxIterations
        {
            get { return _MaxIterations; }
            set
            {
                if (_MaxIterations == value) return;
                _MaxIterations = value;

                Material.SetInt(_MaxIterationsId, value);
                Changed?.Invoke();
            }
        }
        private int _MaxIterations;

        #endregion

        static int _MaxIterationsId = Shader.PropertyToID("_MaxIterations");
        static int _GradientTexId = Shader.PropertyToID("_GradientTex");
        static int _GradientPullId = Shader.PropertyToID("_GradientPull");
    }
}

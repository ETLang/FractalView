using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace FractalView
{
    public class Fractal
    {
        public Fractal()
        {
            Colorizer = new Colorizer();
        }

        public event Action FractalChanged;
        public event Action ViewChanged;

        public Colorizer Colorizer { get; private set; }

        #region ViewCenter

        public double2 ViewCenter
        {
            get { return _ViewCenter; }
            set
            {
                if (_ViewCenter.Equals(value)) return;
                _ViewCenter = value;
                ViewChanged?.Invoke();
            }
        }
        private double2 _ViewCenter;

        #endregion

        #region ViewScale

        public double ViewScale
        {
            get { return _ViewScale; }
            set
            {
                if (_ViewScale == value) return;
                _ViewScale = value;
                ViewChanged?.Invoke();
            }
        }
        private double _ViewScale = 1;

        #endregion

        #region C

        public double2 C
        {
            get { return _C; }
            set
            {
                if (_C.Equals(value)) return;
                _C = value;
                FractalChanged?.Invoke();
            }
        }
        private double2 _C;

        #endregion

        #region Mandulia

        public double Mandulia
        {
            get { return _Mandulia; }
            set
            {
                if (_Mandulia == value) return;
                _Mandulia = value;
                FractalChanged?.Invoke();
            }
        }
        private double _Mandulia;

        #endregion

        #region MaxIterations

        public int MaxIterations
        {
            get { return _MaxIterations; }
            set
            {
                if (_MaxIterations == value) return;
                _MaxIterations = value;
                Colorizer.MaxIterations = value;
                FractalChanged?.Invoke();
            }
        }
        private int _MaxIterations = 250;

        #endregion

        #region AbsMod

        public bool AbsMod
        {
            get { return _AbsMod; }
            set
            {
                if (_AbsMod == value) return;
                _AbsMod = value;
                FractalChanged?.Invoke();
            }
        }
        private bool _AbsMod;

        #endregion

        public Bookmark CaptureToBookmark(string name, string category)
        {
            return new Bookmark
            {
                Center = ViewCenter,
                Scale = ViewScale,
                C = C,
                MaxIterations = MaxIterations,
                Spread = 1,
                AbsMod = AbsMod,
                Name = name,
                Category = category,
            };
        }
    }
}

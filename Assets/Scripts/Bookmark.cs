using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace FractalView
{
    public class Bookmark
    {
        public event Action<Bookmark> IdentityModified;
        public event Action<Bookmark> ValueModified;

        #region Name

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name == value) return;
                _Name = value.Replace('\n', ' ').Replace("\r", "");
                ValidateKey();
            }
        }
        private string _Name;

        #endregion

        #region Category

        public string Category
        {
            get { return _Category; }
            set
            {
                if (_Category == value) return;
                _Category = value.Replace('\n', ' ').Replace("\r", "");
                ValidateKey();
            }
        }
        private string _Category;

        #endregion

        #region Center

        public double2 Center
        {
            get { return _Center; }
            set
            {
                if (Help.All(_Center == value)) return;
                _Center = value;
                Validate();
            }
        }
        private double2 _Center;

        #endregion

        #region Scale

        public double Scale
        {
            get { return _Scale; }
            set
            {
                if (_Scale == value) return;
                _Scale = value;
                Validate();
            }
        }
        private double _Scale;

        #endregion

        #region C

        public double2 C
        {
            get { return _C; }
            set
            {
                if (Help.All(_C == value)) return;
                _C = value;
                Validate();
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
                Validate();
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
                Validate();
            }
        }
        private int _MaxIterations;

        #endregion

        #region AbsMod

        public bool AbsMod
        {
            get { return _AbsMod; }
            set
            {
                if (_AbsMod == value) return;
                _AbsMod = value;
                Validate();
            }
        }
        private bool _AbsMod;

        #endregion

        #region Spread

        public double Spread
        {
            get { return _Spread; }
            set
            {
                if (_Spread == value) return;
                _Spread = value;
                Validate();
            }
        }
        private double _Spread;

        #endregion

        public void Delete()
        {
            if(!string.IsNullOrWhiteSpace(_prefsKey))
                PlayerPrefs.DeleteKey(_prefsKey);
        }

        public override string ToString()
        {
            return $"{Category}\n{Name}\n{ValueString()}";
        }

        public string ValueString()
        {
            var fData = new double[]
            {
                Center.x,
                Center.y,
                Scale,
                C.x,
                C.y,
                Mandulia,
                (double)MaxIterations,
                Spread,
                AbsMod ? 1 : 0
            };
            var fBuffer = new byte[sizeof(double) * fData.Length];

            Buffer.BlockCopy(fData, 0, fBuffer, 0, fBuffer.Length);
            return Convert.ToBase64String(fBuffer);
        }

        public void ReadValueString(string valueStr)
        {
            if (string.IsNullOrEmpty(valueStr))
                return;

            var fBuffer = Convert.FromBase64String(valueStr);

            if (fBuffer.Length != sizeof(double) * 9)
                throw new ArgumentException("Can't read value string to bookmark");

            var fData = new double[9];
            Buffer.BlockCopy(fBuffer, 0, fData, 0, fBuffer.Length);

            Center = new double2(fData[0], fData[1]);
            Scale = fData[2];
            C = new double2(fData[3], fData[4]);
            Mandulia = fData[5];
            MaxIterations = (int)fData[6];
            Spread = fData[7];
            AbsMod = fData[8] == 1;
        }

        public static Bookmark Parse(string str)
        {
            var lines = str.Split('\n');

            if (lines.Length != 3)
                throw new ArgumentException("Can't parse string to bookmark");

            return Parse(lines, 0);
        }

        public static Bookmark Parse(string[] lines, int lineIndex)
        {
            var fBuffer = Convert.FromBase64String(lines[lineIndex + 2]);

            if (lines.Length < lineIndex + 3)
                throw new ArgumentException("Not enough lines to parse bookmark");

            var ret = new Bookmark
            {
                Category = lines[lineIndex + 0],
                Name = lines[lineIndex + 1],
            };

            ret.ReadValueString(lines[lineIndex + 2]);
            return ret;
        }

        public static Bookmark Load(string category, string name)
        {
            var ret = new Bookmark
            {
                Category = category,
                Name = name
            };

            ret.ReadValueString(PlayerPrefs.GetString(GetPrefsKey(category, name)));

            return ret;
        }

        #region Private

        private static string GetPrefsKey(string category, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
                return null;

            return $"{Application.companyName}/{Application.productName}/Bookmark/{category}/{name}";
        }

        private void Validate()
        {
            if (!string.IsNullOrWhiteSpace(_prefsKey))
                PlayerPrefs.SetString(_prefsKey, ValueString());

            ValueModified?.Invoke(this);
        }

        private void ValidateKey()
        {
            var key = GetPrefsKey(Category, Name);
            if (key != _prefsKey && !string.IsNullOrWhiteSpace(_prefsKey))
                PlayerPrefs.DeleteKey(_prefsKey);

            _prefsKey = key;

            if (_prefsKey == null)
                return;

            IdentityModified?.Invoke(this);
            Validate();
        }

        private string _prefsKey;

        #endregion
    }
}

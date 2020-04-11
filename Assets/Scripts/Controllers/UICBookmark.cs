using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace FractalView
{
    public class UICBookmark : MonoBehaviour
    {
        public Text label;

        #region Bookmark

        [SerializeField]
        [NotKeyable]
        private Bookmark _Bookmark;
        public Bookmark Bookmark
        {
            get { return _Bookmark; }
            set
            {
                if (_Bookmark == value) return;
                var oldValue = _Bookmark;
                _Bookmark = value;
                Check_Bookmark = value;
                OnBookmarkChanged(oldValue, value);
            }
        }

#if UNITY_EDITOR
        private Bookmark Check_Bookmark;
        // TODO call Test_Bookmark() within OnValidate() to properly support edit mode and/or animation clips. Remove [NotKeyable] to enable animation clips.
        private void Test_Bookmark() { if (_Bookmark != Check_Bookmark) { OnBookmarkChanged(Check_Bookmark, _Bookmark); Check_Bookmark = _Bookmark; } }
#else
        private void Test_Bookmark() { }
#endif

        #endregion

        #region Unity Plugs

        void Start()
        {
        }

        void OnValidate()
        {
            Test_Bookmark();
        }

        #endregion

        #region Private

        protected virtual void OnBookmarkChanged(Bookmark oldValue, Bookmark newValue)
        {
            if(oldValue != null)
                oldValue.ValueModified -= OnBookmarkModified;
            if(newValue != null)
                newValue.ValueModified += OnBookmarkModified;
            OnBookmarkModified(newValue);
        }

        private void OnBookmarkModified(Bookmark bookmark)
        {
            if (label != null && Bookmark != null)
                label.text = Bookmark.Name;
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace FractalView
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIExpander : MonoBehaviour, IDynamicLayout
    {
        public Animator expandAnimator;

        public event Action IsExpandedChanged;

        #region IDynamicLayout

        event Action IDynamicLayout.Invalidated
        {
            add { _onLayoutInvalidated += value; }
            remove { _onLayoutInvalidated -= value; }
        }

        #region rectTransform

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;

        #endregion

        #endregion

        #region Text

        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text == value) return;
                _Text = value;
                OnTextChanged();
            }
        }

        [SerializeField]
        private string _Text;
#if UNITY_EDITOR
        private string Check_Text;
        private void CheckText()
        {
            if (Check_Text != Text)
            {
                Check_Text = Text;
                OnTextChanged();
            }
        }
#endif

        protected virtual void OnTextChanged()
        {
            if (_label == null)
                _label = GetComponentInChildren<Text>();

            if(_label != null)
                _label.text = Text;
        }

        #endregion

        #region IsExpanded

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded == value) return;
                _IsExpanded = value;
                OnIsExpandedChanged();
            }
        }

        [SerializeField]
        private bool _IsExpanded;
#if UNITY_EDITOR
        private bool Check_IsExpanded;

        private void CheckIsExpanded()
        {
            if (Check_IsExpanded != IsExpanded)
            {
                Check_IsExpanded = IsExpanded;
                OnIsExpandedChanged();
            }
        }
#endif

        public void ToggleIsExpanded()
        {
            IsExpanded = !IsExpanded;
        }

        protected virtual void OnIsExpandedChanged()
        {
            expandAnimator?.SetBool("IsExpanded", IsExpanded);
            _onLayoutInvalidated?.Invoke();
            IsExpandedChanged?.Invoke();
        }

        #endregion

        // Start is called before the first frame update
        void Start()
        {
        }

        void ComputeLayout()
        {
            
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CheckIsExpanded();
        }
#endif

        public Text _label;
        private Action _onLayoutInvalidated;
    }
}
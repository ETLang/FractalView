using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FractalView
{
    public class UICollapsiblePanel : MonoBehaviour, IDynamicLayout
    {
        #region Label

        public string Label
        {
            get { return _Label; }
            set
            {
                if (_Label == value) return;
                _Label = value;
            }
        }

        [SerializeField]
        private string _Label;

#if UNITY_EDITOR
        private string Prev_Label;
        private void TestLabel() { if(Prev_Label != Label) { Prev_Label = Label; OnLabelChanged(); } }
#else 
        private void TestLabel() { }
#endif

        protected virtual void OnLabelChanged()
        {
            if (_expander != null)
                _expander.Text = Label;

            ComputeLayout();
        }

        #endregion

        #region Content

        public IDynamicLayout Content
        {
            get { return _Content; }
            set
            {
                if (_Content == value) return;

                _Content.Invalidated -= OnChildLayoutInvalidated;
                _Content = value;
                _Content.Invalidated += OnChildLayoutInvalidated;
            }
        }

        [SerializeField]
        private IDynamicLayout _Content;

#if UNITY_EDITOR
        private IDynamicLayout Prev_Content;
        private void TestContent() { if(Prev_Content != Content) { Prev_Content = Content; OnContentChanged(); } }
#else
        private void TestContent() { }
#endif

        protected virtual void OnContentChanged()
        {
            ComputeLayout();
        }

        #endregion


        #region Unity Plugs

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            _expander = GetComponentInChildren<UIExpander>();

            if (_expander != null)
            {
                _expanderRT = _expander.GetComponent<RectTransform>();
                ((IDynamicLayout)_expander).Invalidated += OnChildLayoutInvalidated;
                _expander.IsExpandedChanged += OnIsExpandedChanged;
            }

            if (Content != null)
                Content.Invalidated += OnChildLayoutInvalidated;
        }

        private void OnValidate()
        {
            TestLabel();
        }

        #endregion

        #region IDynamicLayout

        event Action IDynamicLayout.Invalidated { add { _onLayoutInvalidated += value; } remove { _onLayoutInvalidated -= value; } }
        public RectTransform rectTransform { get; private set; }

        #endregion

        #region Private

        private void OnIsExpandedChanged()
        {
            Content.gameObject.SetActive(_expander.IsExpanded);
            ComputeLayout();
        }

        private void OnChildLayoutInvalidated()
        {
            ComputeLayout();
        }

        private void ComputeLayout()
        {
            Vector2 size = Vector2.zero;

            if (_expanderRT != null)
                size = _expanderRT.sizeDelta + new Vector2(_expanderRT.localPosition.x, _expanderRT.localPosition.y);

            if (Content != null && Content.gameObject.activeSelf)
            {
                size.x = Math.Max(size.x, Content.rectTransform.sizeDelta.x + Content.rectTransform.localPosition.x);
                size.y = Math.Max(size.y, Content.rectTransform.sizeDelta.y + Content.rectTransform.localPosition.y);
            }

            rectTransform.sizeDelta = size;
            _onLayoutInvalidated?.Invoke();
        }

        private Action _onLayoutInvalidated;
        private UIExpander _expander;
        private RectTransform _expanderRT;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FractalView
{
    public class UIStackLayout : MonoBehaviour, IDynamicLayout
    {
        #region IDynamicLayout

        event Action IDynamicLayout.Invalidated { add { _onInvalidated += value; } remove { _onInvalidated -= value; } }
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

        public IReadOnlyList<IDynamicLayout> Children => _children;

        public void AddChild(IDynamicLayout child)
        {
            if (child == (IDynamicLayout)this)
                throw new ArgumentException("Can't add self as a child");

            child.rectTransform.SetParent(rectTransform, false);
            child.Invalidated += Child_Invalidated;
            _children.Add(child);
            ComputeLayout();
        }

        public void RemoveChild(IDynamicLayout child)
        {
            if (!_children.Contains(child))
                throw new ArgumentException("Child not found");

            Destroy(child.gameObject);
            child.Invalidated -= Child_Invalidated;
            _children.Remove(child);
            ComputeLayout();
        }

        #region Unity Plugs

        void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).GetComponent<IDynamicLayout>();
                if (child != null)
                    AddChild(child);
            }
        }

        #endregion

        #region Private

        private void Child_Invalidated()
        {
            ComputeLayout();
        }

        private void ComputeLayout()
        {
            if (_computingLayout) return;
            _computingLayout = true;

            Vector2 topleft = Vector2.zero;
            float w = 0;

            foreach(var child in Children)
            {
                topleft.y -= child.rectTransform.sizeDelta.y;
                child.rectTransform.localPosition = topleft;
                w = Mathf.Max(w, child.rectTransform.sizeDelta.x);
            }

            rectTransform.sizeDelta = new Vector2(w, -topleft.y);

            _computingLayout = false;
            _onInvalidated?.Invoke();
        }

        private bool _computingLayout;
        private Action _onInvalidated;
        List<IDynamicLayout> _children = new List<IDynamicLayout>();

        #endregion
    }
}
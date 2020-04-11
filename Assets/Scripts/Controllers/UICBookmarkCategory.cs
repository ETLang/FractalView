using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace FractalView
{
    public class UICBookmarkCategory : MonoBehaviour, IDynamicLayout
    {
        public GameObject BookmarkItemPrefab;
        public UIStackLayout ItemContainer;
        public UIExpander Expander;

        public IReadOnlyList<UICBookmark> Bookmarks => _bookmarks;

        public void AddBookmark(Bookmark bookmark)
        {
            if (BookmarkItemPrefab != null && ItemContainer != null)
            {
                var bookmarkui = Instantiate(BookmarkItemPrefab);

                ItemContainer.AddChild(bookmarkui.GetComponent<IDynamicLayout>());

                var uic = bookmarkui.GetComponent<UICBookmark>();
                uic.Bookmark = bookmark;
                bookmark.ValueModified += Bookmark_Modified;
                Bookmark_Modified(bookmark);
                _bookmarks.Add(uic);
            }
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            var uic = _bookmarks.FirstOrDefault(ui => ui.Bookmark == bookmark);

            if (uic == null)
                return;

            ItemContainer.RemoveChild(uic.GetComponent<IDynamicLayout>());

            _bookmarks.Remove(uic);
            uic.Bookmark.ValueModified -= Bookmark_Modified;
            Destroy(uic.gameObject);
        }

        #region Label

        [SerializeField]
        [NotKeyable]
        private string _Label;
        public string Label
        {
            get { return _Label; }
            set
            {
                if (_Label == value) return;
                var oldValue = _Label;
                _Label = value;
                Check_Label = value;
                OnLabelChanged(oldValue, value);
            }
        }

#if UNITY_EDITOR
        private string Check_Label;
        // TODO call Test_Label() within OnValidate() to properly support edit mode and/or animation clips. Remove [NotKeyable] to enable animation clips.
        private void Test_Label() { if (_Label != Check_Label) { OnLabelChanged(Check_Label, _Label); Check_Label = _Label; } }
#else
        private void Test_Label() { }
#endif

        #endregion

        #region Unity Plugs

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            if(ItemContainer != null)
                ((IDynamicLayout)ItemContainer).Invalidated += UICBookmarkCategory_Invalidated;

            if(Expander != null)
                Expander.IsExpandedChanged += Expander_IsExpandedChanged;
        }

        private void Expander_IsExpandedChanged()
        {
            ItemContainer.gameObject.SetActive(Expander.IsExpanded);
            ComputeLayout();
        }

        void OnValidate()
        {
            Test_Label();
        }

        #endregion

        #region IDynamicLayout

        public RectTransform rectTransform { get; private set; }
        event Action IDynamicLayout.Invalidated { add { _onLayoutInvalidated += value; } remove { _onLayoutInvalidated -= value; } }

        #endregion

        #region Private

        protected virtual void OnLabelChanged(string oldValue, string newValue)
        {
            if (Expander != null)
                Expander.Text = newValue;
            foreach (var item in _bookmarks)
                item.Bookmark.Category = newValue;
        }

        private void UICBookmarkCategory_Invalidated()
        {
            ComputeLayout();
        }

        private void ComputeLayout()
        {
            var contentLayout = (IDynamicLayout)ItemContainer;

            if (Expander.IsExpanded)
                rectTransform.sizeDelta = (Vector2)contentLayout.rectTransform.localPosition + contentLayout.rectTransform.sizeDelta;
            else
                rectTransform.sizeDelta = (Vector2)contentLayout.rectTransform.localPosition;
            _onLayoutInvalidated?.Invoke();
        }

        private void Bookmark_Modified(Bookmark mark)
        {
            if (Label != mark.Category)
                Label = mark.Category;
        }

        private List<UICBookmark> _bookmarks = new List<UICBookmark>();
        private Action _onLayoutInvalidated;

        #endregion
    }
}

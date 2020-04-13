using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FractalView
{
    [RequireComponent(typeof(UIStackLayout))]
    public class UICBookmarkList : MonoBehaviour
    {
        public GameObject CategoryPrefab;

        public void AddBookmark(Bookmark bookmark)
        {
            UICBookmarkCategory category;

            if (!_categories.TryGetValue(bookmark.Category, out category))
            {
                var instance = Instantiate(CategoryPrefab);
                category = instance.GetComponent<UICBookmarkCategory>();
                _stack.AddChild(instance.GetComponent<IDynamicLayout>());
            }

            category.AddBookmark(bookmark);
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            if(_categories.TryGetValue(bookmark.Category, out var uic))
            {
                uic.RemoveBookmark(bookmark);

                if (uic.Bookmarks.Count == 0)
                {
                    _categories.Remove(bookmark.Category);
                    _stack.RemoveChild(uic.GetComponent<IDynamicLayout>());
                }
            }
        }

        #region Unity Plugs

        void Start()
        {
            _stack = GetComponent<UIStackLayout>();
            _bookmarks = UIControllerMain.Instance.Bookmarks;
            _bookmarks.CategoryAdded += _bookmarks_CategoryAdded;
            _bookmarks.CategoryRemoved += _bookmarks_CategoryRemoved;
            _bookmarks.CategoryRenamed += _bookmarks_CategoryRenamed;
            foreach (var category in _bookmarks.AllCategories)
                _bookmarks_CategoryAdded(category);
        }

        private void _bookmarks_CategoryRenamed(string arg1, string arg2)
        {
            var uic = _categories[arg1];
            _categories.Remove(arg1);
            _categories[arg2] = uic;
        }

        private void _bookmarks_CategoryRemoved(string obj)
        {
            var uic = _categories[obj];
            Destroy(uic.gameObject);
            _categories.Remove(obj);
        }

        private void _bookmarks_CategoryAdded(string obj)
        {
            var instance = Instantiate(CategoryPrefab, transform);
            instance.name = obj;
            var uic = instance.GetComponent<UICBookmarkCategory>();
            uic.Label = obj;
            _categories[obj] = uic;
        }

        #endregion

        #region Private

        BookmarkCollection _bookmarks;
        UIStackLayout _stack;
        SortedDictionary<string, UICBookmarkCategory> _categories = new SortedDictionary<string, UICBookmarkCategory>();

        #endregion
    }
}

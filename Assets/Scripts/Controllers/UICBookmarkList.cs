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
        }

        #endregion

        #region Private

        UIStackLayout _stack;
        SortedDictionary<string, UICBookmarkCategory> _categories;

        #endregion
    }
}

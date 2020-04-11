using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FractalView
{
    public class BookmarkCollection
    {
        public event Action<Bookmark> BookmarkAdded;
        public event Action<Bookmark> BookmarkRemoved;

        public event Action<string> CategoryAdded;
        public event Action<string> CategoryRemoved;
        public event Action<string, string> CategoryRenamed;

        public BookmarkCollection()
        {
            foreach (var category in ReadAllCategories())
            {
                var list = _bookmarks[category] = new List<Bookmark>();

                foreach (var name in ReadCategoryManifest(category))
                {
                    var bookmark = Bookmark.Load(category, name);
                    bookmark.IdentityModified += Bookmark_Modified;
                    list.Add(bookmark);
                }
            }
        }

        public IEnumerable<Bookmark> AllBookmarks
        {
            get
            {
                foreach (var kvp in _bookmarks)
                    foreach (var bookmark in kvp.Value)
                        yield return bookmark;
            }
        }

        public IReadOnlyCollection<string> AllCategories => _bookmarks.Keys;

        public IReadOnlyCollection<Bookmark> BookmarksInCategory(string category) => _bookmarks[category];

        public void AddBookmark(Bookmark bookmark)
        {
            List<Bookmark> clist;

            if (!_bookmarks.TryGetValue(bookmark.Category, out clist))
            {
                _bookmarks[bookmark.Category] = clist = new List<Bookmark>();
                WriteAllCategories();
                CategoryAdded?.Invoke(bookmark.Category);
            }

            clist.Add(bookmark);
            bookmark.IdentityModified += Bookmark_Modified;
            WriteCategoryManifest(bookmark.Category);
            BookmarkAdded?.Invoke(bookmark);
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            RemoveBookmark(bookmark, bookmark.Category);
            bookmark.Delete();
        }

        public void RenameCategory(string oldName, string newName)
        {
            if (!_bookmarks.ContainsKey(oldName))
                throw new ArgumentException($"{oldName} is not a known category");

            if (_bookmarks.ContainsKey(newName))
                throw new ArgumentException($"{newName} already exists as a category");

            var list = _bookmarks[newName] = _bookmarks[oldName];
            _bookmarks.Remove(oldName);

            foreach (var bookmark in list)
                bookmark.Category = newName;

            WriteAllCategories();
            WriteCategoryManifest(oldName);
            WriteCategoryManifest(newName);

            CategoryRenamed?.Invoke(oldName, newName);
        }

        #region Private

        private void Bookmark_Modified(Bookmark obj)
        {
            KeyValuePair<string, List<Bookmark>> old = new KeyValuePair<string, List<Bookmark>>(null, null);

            foreach (var category in _bookmarks)
                if (category.Value.Contains(obj))
                {
                    if (category.Key == obj.Category)
                        return;

                    old = category;
                    break;
                }

            if (old.Key == null || old.Value == null)
                throw new Exception("Unexpected error modifying bookmark");

            RemoveBookmark(obj, old.Key);
            AddBookmark(obj);
        }

        private void RemoveBookmark(Bookmark bookmark, string category)
        {
            if (_bookmarks.TryGetValue(category, out var list))
            {
                list.Remove(bookmark);

                WriteCategoryManifest(category);
                BookmarkRemoved?.Invoke(bookmark);
                if (list.Count == 0)
                {
                    _bookmarks.Remove(bookmark.Category);
                    WriteAllCategories();
                    CategoryRemoved?.Invoke(bookmark.Category);
                }
            }
        }

        private Dictionary<string, List<Bookmark>> _bookmarks = new Dictionary<string, List<Bookmark>>();

        private static IEnumerable<string> ReadAllCategories()
        {
            var current = PlayerPrefs.GetString(_AllCategoriesKey);

            if (current == null)
                yield break;
            else
            {
                foreach (var category in current.Split('\n'))
                    yield return category;
            }
        }

        private static IEnumerable<string> ReadCategoryManifest(string category)
        {
            var current = PlayerPrefs.GetString(GetCategoryManifestKey(category));

            if (current == null)
                yield break;
            else
            {
                foreach (var name in current.Split('\n'))
                    yield return name;
            }
        }

        private void WriteAllCategories()
        {
            var value = string.Join("\n", _bookmarks.Keys);
            PlayerPrefs.SetString(_AllCategoriesKey, value);
        }

        private void WriteCategoryManifest(string category)
        {
            if (!_bookmarks.ContainsKey(category))
                PlayerPrefs.DeleteKey(GetCategoryManifestKey(category));
            else
            {
                var value = string.Join("\n", _bookmarks[category].Select(b => b.Name));
                PlayerPrefs.SetString(GetCategoryManifestKey(category), value);
            }
        }

        private static string GetCategoryManifestKey(string category)
        {
            return $"{Application.companyName}/{Application.productName}/Category/{category}";
        }

        private static string _AllCategoriesKey = $"{Application.companyName}/{Application.productName}/AllCategories";

        #endregion
    }
}

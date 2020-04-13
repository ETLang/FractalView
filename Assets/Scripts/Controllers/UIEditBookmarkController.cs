using FractalView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditBookmarkController : MonoBehaviour
{
    public InputField nameBox;
    public InputField categoryBox;
    public UIControllerMain mainController;

    void Awake()
    {
        _bookmarks = BookmarkCollection.Instance;
    }

    void Start()
    {
        _fractal = mainController.Fractal;
    }

    public void OnEnable()
    {
        if (string.IsNullOrWhiteSpace(categoryBox.text))
            categoryBox.text = "Bookmark";

        nameBox.text = _bookmarks.GetUnusedName(categoryBox.text);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    public void OK()
    {
        _bookmarks.AddBookmark(_fractal.CaptureToBookmark(nameBox.text, categoryBox.text));
        gameObject.SetActive(false);
    }

    private BookmarkCollection _bookmarks;
    private Fractal _fractal;
}

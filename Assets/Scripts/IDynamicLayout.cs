using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FractalView
{
    public interface IDynamicLayout
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        RectTransform rectTransform { get; }
        event Action Invalidated;
    }
}
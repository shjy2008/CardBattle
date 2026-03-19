using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork
{
    public interface ListViewDelegate
    {
        Vector2 CellSizeByIndex(int index);

        int CellCount();

        RectTransform CellAtIndex(RectTransform cell, int index);

        void StartUpdate();

    }
}

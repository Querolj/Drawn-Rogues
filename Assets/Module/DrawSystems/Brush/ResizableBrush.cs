using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "ResizableBrush", menuName = "Misc/ResizableBrush", order = 1)]
public class ResizableBrush : ScriptableObject
{
    public Brush[] Brushes;

    private int _indexActiveBrush = 0;
    private Brush _activeBrush = null;
    public Brush ActiveBrush
    {
        get
        {
            if (_activeBrush == null)
            {
                _activeBrush = Brushes[0];
                _indexActiveBrush = 0;
            }
            return _activeBrush;
        }
    }

    public void SetBiggestBrushPossible (int maxPixelsAllowed)
    {
        for (int i = Brushes.Length - 1; i >= 0; i--)
        {
            if (Brushes[i].GetOpaquePixelsCount () <= maxPixelsAllowed)
            {
                _activeBrush = Brushes[i];
                _indexActiveBrush = i;
                return;
            }
        }
    }

    public void ChangeBrushFromIndexOffset (int indexOffset)
    {
        _indexActiveBrush = Mathf.Clamp ((_indexActiveBrush + indexOffset) % Brushes.Length, 0, Brushes.Length - 1);
        _activeBrush = Brushes[_indexActiveBrush];
    }

    public void SetBrushSize (int size)
    {
        if (size < 0 || size >= Brushes.Length)
        {
            throw new System.ArgumentException ("Size " + size + " is out of range");
        }
        _indexActiveBrush = size;
        _activeBrush = Brushes[_indexActiveBrush];
    }

    public bool HasSmallestBrushActive ()
    {
        return _indexActiveBrush == 0;
    }

}
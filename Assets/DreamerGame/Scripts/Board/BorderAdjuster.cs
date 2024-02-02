using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderAdjuster
{
    private readonly RectTransform _boardBorderRect;
    private readonly Vector2 _cellSize;
    private readonly float _borderPadding;
    private readonly float _shadedFaceLength;
    private readonly Camera _activeCamera;
    private readonly Vector2Int _size;
    private readonly SpriteRenderer _borderSprite;
    private readonly Transform _transform;

    public BorderAdjuster(Transform transform, Vector2Int size, RectTransform boardBorderRect, SpriteRenderer borderSprite, float borderPadding, GameObject defaultItem, Vector2 cellSize, Camera activeCamera)
    {
        _transform = transform;
        _boardBorderRect = boardBorderRect;
        _cellSize = cellSize;
        _borderPadding = borderPadding;
        _shadedFaceLength = defaultItem.GetComponent<Item>().spriteRenderer.size.y - cellSize.y;
        _activeCamera = activeCamera;
        _size = size;
        _borderSprite = borderSprite;
    }

    public void Adjust()
    {
        float desiredRatio = GetAspectRatio(_boardBorderRect.rect);
        Vector2 midPoint = _boardBorderRect.transform.position;
        RectOffsets offsets = GetRectOffsets(_boardBorderRect);
        float ratioX = GetAdjustedRatio(_boardBorderRect.rect.width, offsets.left, offsets.right);
        float ratioY = GetAdjustedRatio(_boardBorderRect.rect.height, offsets.top, offsets.bottom);

        _borderSprite.size = CalculateBorderSize();
        float currentRatio = GetAspectRatio(_borderSprite.size);
        float oldSize = _activeCamera.orthographicSize;

        _activeCamera.orthographicSize = CalculateOrthographicSize(desiredRatio, currentRatio, ratioX, ratioY);
        AdjustTransformPosition(midPoint, oldSize);
    }
    
    private float GetAspectRatio(Vector2 size)
    {
        return size.x / size.y;
    }

    private float GetAspectRatio(Rect rect)
    {
        return rect.width / rect.height;
    }

    private RectOffsets GetRectOffsets(RectTransform rectTransform)
    {
        return new RectOffsets
        {
            left = Mathf.Abs(rectTransform.offsetMin.x),
            right = Mathf.Abs(rectTransform.offsetMax.x),
            top = Mathf.Abs(rectTransform.offsetMax.y),
            bottom = Mathf.Abs(rectTransform.offsetMin.y)
        };
    }

    private float GetAdjustedRatio(float size, float offsetA, float offsetB)
    {
        return size / (size + offsetA + offsetB);
    }

    private Vector2 CalculateBorderSize()
    {
        return new Vector2(_cellSize.x * _size.x + _borderPadding, _cellSize.y * _size.y + _borderPadding + _shadedFaceLength);
    }

    private float CalculateOrthographicSize(float desiredRatio, float currentRatio, float ratioX, float ratioY)
    {
        if (currentRatio < desiredRatio)
        {
            return (_cellSize.y * _size.y + _borderPadding) / ratioY / 2;
        }
        else
        {
            return ((_cellSize.x * _size.x + _borderPadding) / ratioX) / (2 * _activeCamera.aspect);
        }
    }

    private void AdjustTransformPosition(Vector2 midPoint, float oldSize)
    {
        float sizeMultiplier = _activeCamera.orthographicSize / oldSize;
        midPoint.y *= sizeMultiplier;
        _transform.position = midPoint;
    }
    
    private struct RectOffsets
    {
        public float left, right, top, bottom;
    }
}

using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourGameOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image optionImage;
    [SerializeField] private Image shapeIconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Vector2 iconBoxSize = new Vector2(80f, 80f);
    [SerializeField, Range(0.5f, 1f)] private float iconFill = 0.86f;
    [SerializeField] private bool useAutoNormalizeScale = false;

    private IOptionSelectionHandler controller;
    private int colourIndex;
    private int shapeIndex;
    private static readonly Dictionary<int, float> SpriteOccupancyCache = new Dictionary<int, float>();

    public void Setup(
        IOptionSelectionHandler owner,
        int colourId,
        int shapeId,
        Color displayColor,
        string displayLabel,
        Sprite shapeSprite)
    {
        controller = owner;
        colourIndex = colourId;
        shapeIndex = shapeId;

        if (optionImage != null)
        {
            // Keep button clickable, but make background visually transparent.
            optionImage.color = new Color(1f, 1f, 1f, 0f);
        }

        if (shapeIconImage != null)
        {
            shapeIconImage.sprite = shapeSprite;
            shapeIconImage.enabled = shapeSprite != null;
            shapeIconImage.color = displayColor;

            RectTransform iconRect = shapeIconImage.rectTransform;
            if (iconRect != null)
            {
                iconRect.sizeDelta = iconBoxSize;
                iconRect.localScale = Vector3.one;

                if (shapeSprite != null && useAutoNormalizeScale)
                {
                    float scale = CalculateAutoScale(shapeSprite);
                    iconRect.localScale = Vector3.one * scale;
                }
            }
        }

        if (labelText != null)
        {
            labelText.text = displayLabel;
            labelText.enabled = shapeSprite == null;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
            button.interactable = true;
        }
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
        {
            button.interactable = value;
        }
    }

    private void OnClicked()
    {
        if (controller != null)
        {
            controller.OnOptionSelected(colourIndex, shapeIndex);
        }
    }

    private float CalculateAutoScale(Sprite sprite)
    {
        float occupancy = GetOpaqueOccupancy(sprite);
        if (occupancy <= 0f)
        {
            return 1f;
        }

        float desiredScale = iconFill / occupancy;
        return Mathf.Clamp(desiredScale, 0.6f, 2f);
    }

    private float GetOpaqueOccupancy(Sprite sprite)
    {
        int spriteId = sprite.GetInstanceID();
        if (SpriteOccupancyCache.TryGetValue(spriteId, out float cachedValue))
        {
            return cachedValue;
        }

        Texture2D texture = sprite.texture;
        if (texture == null || !texture.isReadable)
        {
            // If texture is not readable, fallback to neutral scale behavior.
            SpriteOccupancyCache[spriteId] = 1f;
            return 1f;
        }

        Rect rect = sprite.rect;
        int minX = Mathf.FloorToInt(rect.xMin);
        int maxX = Mathf.CeilToInt(rect.xMax);
        int minY = Mathf.FloorToInt(rect.yMin);
        int maxY = Mathf.CeilToInt(rect.yMax);

        int opaqueMinX = maxX;
        int opaqueMaxX = minX;
        int opaqueMinY = maxY;
        int opaqueMaxY = minY;
        bool hasOpaquePixels = false;

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Color pixel = texture.GetPixel(x, y);
                if (pixel.a <= 0.01f)
                {
                    continue;
                }

                hasOpaquePixels = true;
                if (x < opaqueMinX) opaqueMinX = x;
                if (x > opaqueMaxX) opaqueMaxX = x;
                if (y < opaqueMinY) opaqueMinY = y;
                if (y > opaqueMaxY) opaqueMaxY = y;
            }
        }

        if (!hasOpaquePixels)
        {
            SpriteOccupancyCache[spriteId] = 1f;
            return 1f;
        }

        float opaqueWidth = opaqueMaxX - opaqueMinX + 1f;
        float opaqueHeight = opaqueMaxY - opaqueMinY + 1f;
        float opaqueMax = Mathf.Max(opaqueWidth, opaqueHeight);
        float fullMax = Mathf.Max(rect.width, rect.height);
        float occupancy = fullMax > 0f ? opaqueMax / fullMax : 1f;

        SpriteOccupancyCache[spriteId] = occupancy;
        return occupancy;
    }
}

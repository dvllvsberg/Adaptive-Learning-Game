using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image shapeImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    private ShapeGame1Controller controller;
    private Canvas rootCanvas;
    private Transform dragLayer;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private bool placedSuccessfully;

    public ShapeKind ShapeKind { get; private set; }

    public void Initialize(
        ShapeGame1Controller owner,
        Canvas canvas,
        Transform layerForDrag,
        ShapeKind shapeKind,
        Sprite shapeSprite,
        Color shapeColor,
        float visualScale)
    {
        controller = owner;
        rootCanvas = canvas;
        dragLayer = layerForDrag;
        ShapeKind = shapeKind;

        if (shapeImage != null)
        {
            shapeImage.sprite = shapeSprite;
            shapeImage.color = shapeColor;
            shapeImage.preserveAspect = true;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * visualScale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placedSuccessfully)
        {
            return;
        }

        originalParent = transform.parent;
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        if (dragLayer != null)
        {
            transform.SetParent(dragLayer, true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.92f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placedSuccessfully || rectTransform == null || rootCanvas == null)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        if (placedSuccessfully)
        {
            return;
        }

        ReturnToStart();
    }

    public void TryDropOnZone(ShapeDropZone zone)
    {
        if (controller == null || zone == null || placedSuccessfully)
        {
            return;
        }

        bool isCorrect = controller.TryPlaceShape(this, zone);
        if (isCorrect)
        {
            placedSuccessfully = true;
            return;
        }

        ReturnToStart();
    }

    private void ReturnToStart()
    {
        if (originalParent != null && transform.parent != originalParent)
        {
            transform.SetParent(originalParent, false);
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }
}

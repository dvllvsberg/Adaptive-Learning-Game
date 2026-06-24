using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShapeDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private ShapeKind acceptedShape = ShapeKind.Circle;
    [SerializeField] private Image zoneHighlightImage;
    [SerializeField] private Image silhouetteImage;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.08f);
    [SerializeField] private Color correctFlashColor = new Color(0.2f, 1f, 0.2f, 0.25f);
    [SerializeField] private Color wrongFlashColor = new Color(1f, 0.2f, 0.2f, 0.25f);

    public ShapeKind AcceptedShape => acceptedShape;

    private void Start()
    {
        SetZoneColor(normalColor);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        DraggableShape draggable = eventData.pointerDrag.GetComponent<DraggableShape>();
        if (draggable == null)
        {
            return;
        }

        draggable.TryDropOnZone(this);
    }

    public void FlashCorrect()
    {
        SetZoneColor(correctFlashColor);
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.2f);
    }

    public void FlashWrong()
    {
        SetZoneColor(wrongFlashColor);
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.2f);
    }

    public void ShowSilhouette()
    {
        if (silhouetteImage != null)
        {
            silhouetteImage.enabled = true;
        }
    }

    public void HideSilhouette()
    {
        if (silhouetteImage != null)
        {
            silhouetteImage.enabled = false;
        }
    }

    private void ResetColor()
    {
        SetZoneColor(normalColor);
    }

    private void SetZoneColor(Color color)
    {
        if (zoneHighlightImage != null)
        {
            zoneHighlightImage.color = color;
        }
    }
}

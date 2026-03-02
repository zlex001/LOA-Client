using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

namespace Game.Utils
{
    public class DragElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private GameObject scrollRect;

        private Image raycast_Image;


        private void Start()
        {
            raycast_Image = gameObject.GetComponent<Image>();
        }

        public void SetScrollRect(GameObject obj)
        {
            scrollRect = obj;
        }

        public void OnBeginDrag(PointerEventData pointerEventData)
        {
            // If you only need to pass the drag through use
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect, pointerEventData, ExecuteEvents.beginDragHandler);
            }
            raycast_Image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData pointerEventData)
        {
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect, pointerEventData, ExecuteEvents.dragHandler);
            }
        }

        public void OnEndDrag(PointerEventData pointerEventData)
        {
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect, pointerEventData, ExecuteEvents.endDragHandler);
            }
            raycast_Image.raycastTarget = true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spinner : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private float endValue;

    private RectTransform spin;

    private void Start()
    {
        spin = GetComponent<RectTransform>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        var nextPosition = Vector2.zero;

        if (spin.anchoredPosition.y < endValue)
        {
            nextPosition.y = Input.mousePosition.y;
        }
        else
        {
            nextPosition.y = endValue;
        }

        nextPosition.x = spin.anchoredPosition.x;

        print(nextPosition);

        spin.anchoredPosition = nextPosition;
    }
}

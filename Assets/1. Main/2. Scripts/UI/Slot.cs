using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    protected PlayerController Player => GameManager.Instance.MyPlayer;

    public virtual void OnCursorIn() { }
    public virtual void OnClick() { }
    public virtual void OnCursorOut() { }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnCursorIn();
    }
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnCursorOut();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}

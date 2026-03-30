using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("홀딩 설정")]
    [SerializeField] private float holdDelay = 0.5f; // 홀드 인식까지의 시간
    [SerializeField] private float holdInterval = 0.1f; // 홀드상태에서의 업그레이드 반복 간격

    private ItemSlot slot;
    private Coroutine holdCO;

    private Action onHoldClick; 
    public void Init(Action onHoldClickAction)
    {
        onHoldClick = onHoldClickAction;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        onHoldClick?.Invoke();
        holdCO = StartCoroutine(HoldUpgrade());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (holdCO != null)
        {
            StopCoroutine(holdCO);
            holdCO = null;
        }
    }

    private IEnumerator HoldUpgrade()
    {
        yield return new WaitForSeconds(holdDelay); // 홀드 인식 대기

        while (true)
        {
            onHoldClick?.Invoke();
            yield return new WaitForSeconds(holdInterval);
        }
    }
    public void StopHold()
    {
        if (holdCO != null)
        {
            StopCoroutine(holdCO);
            holdCO = null;
        }
    }

}

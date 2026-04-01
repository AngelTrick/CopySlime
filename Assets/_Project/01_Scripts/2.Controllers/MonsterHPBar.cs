using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHPBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider hpSlider;

    private Camera _mainCam;
    void Start()
    {
        // 게임 시작 시 메인 카메라를 찾아둡니다.
        _mainCam = Camera.main;
    }

    public void UpdateHP(double currentHp, double maxHp)
    {
        if (hpSlider != null)
        {
            float hpRatio = (float)(currentHp / maxHp);

            hpSlider.value = hpRatio;
        }
    }

    void LateUpdate()
    {
        if (_mainCam != null)
        {
            transform.LookAt(transform.position + _mainCam.transform.rotation * Vector3.forward, _mainCam.transform.rotation * Vector3.up);
        }
    }
}

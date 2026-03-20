using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkill : MonoBehaviour
{
    [Header("잠금 설정")]
    public int unlockLevel; // 몇 레벨에 해금되는지 or 보스 클리어나 스테이지로 변경 가능
    public Button skillButton;
    public Image lockImage; // 잠금 이미지 / 이미지 없으면 그냥 비활성화로 처리예정

    void Start()
    {
        SetLock(true); 
    }

    public void SetLock(bool isLocked)
    {
        skillButton.interactable = !isLocked;
        lockImage.gameObject.SetActive(isLocked); 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class UISkill : MonoBehaviour
{
    [Header("스킬 버튼")]
    public Button skillButton;
    [Header("쿨타임")]
    public Image cooldownImage;
    [Header("공유")]
    public UISkill pairingSkill;
    [Header("잠금")]
    public Image lockImage;
    public bool isSkillBI = false; // SkillButton_Image 면 true 체크

    public int unlockLevel = 1;  // 해금 레벨

    private bool isCooldown = false;
    private bool isLocked = true;

    private float duration = 3f;
    private float elapsed = 0f;
    void Start()
    {
        skillButton.onClick.AddListener(OnClick);
        cooldownImage.fillAmount = 0f;

        if (isSkillBI && pairingSkill != null) unlockLevel = pairingSkill.unlockLevel;

        SetLock(true);
        if (!isSkillBI)
        {
            UISkillUpdate usu = FindObjectOfType<UISkillUpdate>();
            if (usu != null) usu.RegisterSkill(this);
        }
    }
    public void SetLock(bool locked)
    {
        isLocked = locked;
        skillButton.interactable = !locked;

        if (lockImage != null) lockImage.gameObject.SetActive(locked);
        if (pairingSkill != null) pairingSkill.SetPreventLock(locked);

    }
    public void SetPreventLock(bool locked) //루프방지
    {
        isLocked = locked;
        skillButton.interactable = !locked;

        if (lockImage != null) lockImage.gameObject.SetActive(locked);

    }
    void OnClick()
    {
        if (isCooldown || isLocked) return;

        TempSkill(); // SO 대체하기

        Debug.Log($"{gameObject.name} 클릭!");
        StartCooldown();

        if (pairingSkill != null) pairingSkill.StartCooldownPair();
    }
    void TempSkill()
    {
        Debug.Log($"스킬 실행"); // 나중에 SO 있으면 그걸로
    }
    void StartCooldown()
    {
        isCooldown = true;
        elapsed = 0f;
        skillButton.interactable = false;
        cooldownImage.fillAmount = 1f;
    }
    public void StartCooldownPair()
    {
        if (!isCooldown)
        {
            isCooldown = true;
            elapsed = 0f;
            skillButton.interactable = false;
            cooldownImage.fillAmount = 1f;
        }
    }
    // 코루틴으로 하니깐 창 비활성화시 오류가 발생함 좋은 방법이 생각나기전까 업데이트로 구현
    public void ManualUpdate()
    {
        if (!isCooldown) return;

        elapsed += Time.deltaTime;
        float fill = 1f - (elapsed / duration);

        cooldownImage.fillAmount = fill;
        if (pairingSkill != null) pairingSkill.cooldownImage.fillAmount = fill;


        if (elapsed >= duration) // 쿨탐 종료
        {
            isCooldown = false;
            elapsed = 0f;
            cooldownImage.fillAmount = 0f;

            if (!isLocked) skillButton.interactable = true;

            if (pairingSkill != null)
            {
                pairingSkill.cooldownImage.fillAmount = 0f;
                pairingSkill.isCooldown = false;
                pairingSkill.elapsed = 0f;

                if (!pairingSkill.isLocked) pairingSkill.skillButton.interactable = true;

            }
        }
    }
}
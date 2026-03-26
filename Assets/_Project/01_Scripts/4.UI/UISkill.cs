using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkill : MonoBehaviour
{
    [Header("스킬 버튼")]
    public Button skillButton;
    [Header("쿨타임")]
    public Image cooldownImage;
    public TextMeshProUGUI cooldownText;
    public Color cooldownTextColor = Color.white; // 쿨타임 텍스트 색상
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

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
            cooldownText.color = cooldownTextColor;
        }
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
        cooldownImage.fillAmount = 0f;
        cooldownImage.color = new Color(cooldownImage.color.r,
                                cooldownImage.color.g,
                                cooldownImage.color.b, 0.4f);
        if (cooldownText != null) cooldownText.gameObject.SetActive(true);
    }
    public void StartCooldownPair()
    {
        if (!isCooldown)
        {
            isCooldown = true;
            elapsed = 0f;
            skillButton.interactable = false;
            cooldownImage.fillAmount = 0f;
            cooldownImage.color = new Color(cooldownImage.color.r,
                                cooldownImage.color.g,
                                cooldownImage.color.b, 0.4f);
            if (cooldownText != null) cooldownText.gameObject.SetActive(true);
        }
    }
    // 코루틴으로 하니깐 창 비활성화시 오류가 발생함 좋은 방법이 생각나기전까 업데이트로 구현
    // 3. 알파값 0 -> 뒤에 블럭깔아놔서 터치안되게 막기 -> 기본이 완성되고 디테일올릴때 생각 해본걸로 개선하기
    public void ManualUpdate()
    {
        if (!isCooldown) return;

        elapsed += Time.deltaTime;
        float fill = elapsed / duration;
        float remaining = duration - elapsed;

        cooldownImage.fillAmount = fill;
        if (cooldownText != null) cooldownText.text = remaining.ToString("F1");

        if (pairingSkill != null)
        {
            pairingSkill.cooldownImage.fillAmount = fill;

            if (pairingSkill.cooldownText != null)
            {
                pairingSkill.cooldownText.text = remaining.ToString("F1");
            }
        }

        if (elapsed >= duration) // 쿨탐 종료
        {
            isCooldown = false;
            elapsed = 0f;
            cooldownImage.fillAmount = 0f;

            if (cooldownText != null) cooldownText.gameObject.SetActive(false);

            if (!isLocked) skillButton.interactable = true;

            if (pairingSkill != null)
            {
                pairingSkill.cooldownImage.fillAmount = 0f;
                pairingSkill.isCooldown = false;
                pairingSkill.elapsed = 0f;

                if (pairingSkill.cooldownText != null)
                {
                    pairingSkill.cooldownText.gameObject.SetActive(false);
                }
                if (!pairingSkill.isLocked) pairingSkill.skillButton.interactable = true;

            }
        }
    }
}
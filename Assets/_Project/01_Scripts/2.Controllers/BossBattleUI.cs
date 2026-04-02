using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBattleUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Image timerFill;
    public Image hpFill;

    private Monster _targetBoss;
    public void Open(Monster boss, string stageTitle)
    {
        if (_targetBoss != null) _targetBoss.OnHpChanged -= UpdateHpBar;

        _targetBoss = boss;

        if (_targetBoss != null)
            {
            _targetBoss.OnHpChanged += UpdateHpBar;
            UpdateHpBar(_targetBoss.currentHp, _targetBoss.GetMaxHp());
        }
        if (timerFill != null) timerFill.fillAmount = 1f;

        InvokeRepeating(nameof(RefreshTimerUI), 0f, 0.1f);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (_targetBoss != null) _targetBoss.OnHpChanged -= UpdateHpBar;
        CancelInvoke(nameof(RefreshTimerUI));

        _targetBoss = null;
        gameObject.SetActive(false);
    }
    private void UpdateHpBar(double currentHp, double maxHp)
    {
        if (hpFill != null && maxHp > 0)
        {
            hpFill.fillAmount = (float)(currentHp / maxHp);
        }
    }
    private void RefreshTimerUI()
    {
        if (timerFill != null && StageManager.Instance != null)
        {
            timerFill.fillAmount = StageManager.Instance.GetBossTimerProgress();
        }
    }
}

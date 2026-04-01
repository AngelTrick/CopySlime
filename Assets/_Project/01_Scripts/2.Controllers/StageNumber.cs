using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageNumber : MonoBehaviour
{
    [Header("텍스트 연결")]
    public TextMeshProUGUI stageText; //스테이지 번호
    public TextMeshProUGUI stageName; //스테이지 이름

    void Update()
    {
        if (StageManager.Instance == null) return;

        int currentLevel = StageManager.Instance.GetCurrentLevel();
        if (stageText != null)
        {
            stageText.text = "STAGE " + currentLevel;
        }

        string currentName = StageManager.Instance.GetStageName();
        if (stageName != null)
        {
            stageName.text = currentName;
        }
    }
}

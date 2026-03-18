using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System;


public class ScriptsMerger : EditorWindow
{
    private static Encoding DetectEncodingAndReadAllText(string filePath, out string fileContent)
    {
        byte[] bytes = File.ReadAllBytes(filePath);

        if(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            fileContent = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            return Encoding.UTF8;
        }

        Encoding strictUtf8 = new UTF8Encoding(false, true);
        try
        {
            fileContent = strictUtf8.GetString(bytes);
            return Encoding.UTF8;
        }
        catch (DecoderFallbackException)
        {
            Encoding eucKr = Encoding.GetEncoding("euc-kr");
            fileContent = eucKr.GetString(bytes);
            return eucKr;
        }
    }

    [MenuItem("Tools/1. 팀원 스크립트 일괄 변환 (깨짐 복구)")]
    public static void FixEncodingToUTF8()
    {
        string targetFolder = Application.dataPath + "/_Project/01_Scripts";

        if (!Directory.Exists(targetFolder))
        {
            Debug.LogError(" Scripts 폴더를 찾을 수 없습니다. 경로를 확인 해주세요");
            return;
        }

        string[] filePaths = Directory.GetFiles(targetFolder, "*.cs", SearchOption.AllDirectories);

        int convertedCount = 0;
        int skipedCount = 0;

        foreach(string path in filePaths)
        {
            Encoding currentEncoding = DetectEncodingAndReadAllText(path, out string content);

            if(currentEncoding == Encoding.UTF8)
            {
                skipedCount++;
                continue;
            }

            File.WriteAllText(path, content, Encoding.UTF8);
            convertedCount++;
        }

        Debug.Log($"인코딩 검사 완료. 변환됨 : {convertedCount}개 | 이미 UTF-8이라 스킵됨: {skipedCount}개");
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/2. AI 컨텍스트용 스크립트 병합기")]
    private static void MergeScriptsForAI()
    {
        string targetFolder = Application.dataPath + "/_Project/01_Scripts";

        if (!Directory.Exists(targetFolder))
        {
            Debug.LogError(" Scripts 폴더를 찾을 수 없습니다. 경로를 확인 해주세요");
            return;
        }

        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string backupFolderPath = Path.Combine(projectRootPath, "AI_Scripts_Backup");

        if (!Directory.Exists(backupFolderPath))
        {
            Directory.CreateDirectory(backupFolderPath);
        }

        string outputPath = Path.Combine(backupFolderPath, "MergedSciptsForAI.md");

        StringBuilder sb = new StringBuilder();

        string[] filePaths = Directory.GetFiles(targetFolder, "*.cs", SearchOption.AllDirectories);

        foreach (string path in filePaths)
        {
            DetectEncodingAndReadAllText(path, out string fileContent);

            string codeBlock = new string('`', 3);

            sb.AppendLine($"\n### File: `{Path.GetFileName(path)}`\n");
            sb.AppendLine($"{codeBlock}csharp");
            sb.AppendLine(fileContent);
            sb.AppendLine(codeBlock + "\n");
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"AI용 스크립트 병합 완료 ! 저장 위치 : {outputPath}");
        EditorUtility.RevealInFinder(outputPath);
    }
}

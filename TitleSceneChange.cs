using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneChange : MonoBehaviour
{
    // 遷移先シーン名を記入
    [Header("遷移先シーン名を記入")]
    public string sceneName;

    /// <summary>
    /// 各ボタンに合わせてシーン先を変化
    /// 今後のボタン増加に対応
    /// </summary>
    public void SceneChange()
    {
        SceneManager.LoadScene(sceneName);
    }
}

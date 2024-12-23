using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class CarutaRanking : MonoBehaviour
{
    /// <summary>
    /// 変数初期化
    /// </summary>
    /// 

    // ランキング名
    private string[] rankNames = { "1st", "2nd", "3rd" };

    // ランキング数
    private const int rankCnt = CarutaSaveData.rankCnt;

    // スコア
    public int score;

    /// <summary>
    /// コンポーネント取得用
    /// </summary>
    /// 

    // ランキングのテキスト
    private TextMeshProUGUI[] rankTexts = new TextMeshProUGUI[rankCnt];
    CarutaSaveData data;


    // Start is called before the first frame update
    void Start()
    {
        // セーブデータをCarutaDataManagerから参照
        data = GetComponent<CarutaDataManager>().carutaSaveData;   
        // スコアを参照
        score = CardChosenController.scoreInt;

        for (int i = 0; i < rankCnt; i++)
        {
            // 子オブジェクト取得
            Transform rankChilds = GameObject.Find("RankTexts").transform.GetChild(i);
            // 子オブジェクトのコンポーネント取得
            rankTexts[i] = rankChilds.GetComponent<TextMeshProUGUI>();
        }

        // ランキング表記
        SetRank(score);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DispRank();
    }

    /// <summary>
    /// ランキング表示
    /// </summary>
    private void DispRank()
    {
        for(int i = 0; i < rankCnt; i++)
        {
            rankTexts[i].text = (rankNames[i] + " : " + data.rank[i]);
        }
    }

    /// <summary>
    /// ランキング保存
    /// </summary>
    public void SetRank(int scoreTxt)
    {
        //TMP_InputField inputField = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>(); 

        // スコアがランキング内の値より大きいときは入れ替え
        for (int i = 0; i < rankCnt; i++)
        {
            if (score > data.rank[i])
            {
                var rep = data.rank[i];
                data.rank[i] = scoreTxt;
                score = rep;
            }
        }
    }

    /// <summary>
    /// ランクデータの削除
    /// </summary>
    public void DelRank()
    {
        for (int i = 0; i < rankCnt; i++)
        {
            data.rank[i] = 0;
        }
    }
}

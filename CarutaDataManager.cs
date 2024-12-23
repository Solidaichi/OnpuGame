using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CarutaDataManager : MonoBehaviour
{
    // json変換するデータのクラス
    [HideInInspector]
    public CarutaSaveData carutaSaveData;
    // jsonファイルのパス
    private string filePath;
    // jsonファイル名
    string fileName = "CarutaData.json";

    /// <summary>
    /// 開始時にファイルチェック、読み込み
    /// </summary>

    void Awake()
    {
        // パス名取得
#if UNITY_EDITOR
        filePath = Application.dataPath + "/" + fileName;

#elif UNITY_IOS
    filepath = Application.persistentDataPath + "/" + fileName;    

#endif

        // ファイルがないとき、ファイル作成
        if (!File.Exists(filePath))
        {
            Save(carutaSaveData);
        }

        // ファイルを読み込んでcarutaSaveDataに格納
        carutaSaveData = Load(filePath);
    }

    /// <summary>
    /// jsonとしてデータを保存
    /// </summary>
    /// <param name="data"></param>
    void Save(CarutaSaveData data)
    {
        Debug.Log("filePath : " + filePath);
        // jsonとして変換
        string json = JsonUtility.ToJson(carutaSaveData);
        // ファイル書き込み指定
        StreamWriter wr = new StreamWriter(filePath, false);
        // json変換した情報を書き込み
        wr.WriteLine(json);
        // ファイル閉じる
        wr.Close();
    }

    /// <summary>
    /// jsonファイル読み込み
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    CarutaSaveData Load(string path)
    {
        // ファイル読み込み指定
        StreamReader rd = new StreamReader(path);
        // ファイル内容全て読み込む
        string json = rd.ReadToEnd();
        // ファイル閉じる
        rd.Close();

        // jsonファイルを型に戻して返す
        return JsonUtility.FromJson<CarutaSaveData>(json);
    }

    /// <summary>
    /// ゲーム終了時に保存
    /// </summary>
    ///
    private void OnDestroy()
    {
        Save(carutaSaveData);
    }
}

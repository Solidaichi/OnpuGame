using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.SceneManagement;

public class CardChosenController : MonoBehaviour
{
    // カードの出現初期位置を設定（Deck）
    [Header("カードの出現初期位置を設定（Deck）"), SerializeField]
    private GameObject cardStartDeck;

    // カードフィールドを設定
    [Header("カードフィールドを設定(配置)"), SerializeField]
    private GameObject[] carutataFieldsObj;

    // Resourcesのカードをすべて配列に保管
    private GameObject[] carutaCardRandom = new GameObject[9];

    // 出題用カルタ・テキスト実装配列
    private GameObject[] curutaCardRandomAfter = new GameObject[8];
    
    // カルタDoTween処理用・出現用配列
    private GameObject[] tweenInstantiateCaruta = new GameObject[8];

    // 問題テキストの処理用・出現用配列
    private GameObject questionTextOpen;

    // 問題テキストの重複しない乱数リスト
    private int[] numbers = new int[8];

    // スコア加点用変数
    [HideInInspector]
    public static int scoreInt = 0;

    // スコア表記用テキスト・ゲームオブジェクト
    [Header("スコア表記用テキスト（ゲームオブジェクト設置）"), SerializeField]
    private GameObject scoreTextObj;

    private TextMeshProUGUI scoreText;

    /// <Summary>=====================================
    /// 効果音の設定用変数
    /// </Summary>=====================================
    /// 

    // AudioSourceの準備
    private AudioSource audioSource;

    [Header("スタート時の笛の音"), SerializeField]
    private AudioClip startSound;

    /// <Summary>=====================================
    /// 制限時間の変数設定
    /// </Summary>=====================================
    /// 
    // 制限時間の設定
    [Header("制限時間の設定(分単位：int)"), SerializeField]
    private int timeLimitMinutes;

    [Header("制限時間の設定(秒単位：float)"), SerializeField]
    private float timeLimitSeconds = 0f;

    // タイマー用GameObject, Text
    [Header("制限時間のタイマー表記ゲームオブジェクト(TMP)"), SerializeField]
    private GameObject timerTextObj;

    private TextMeshProUGUI timerText;

    // 経過時間
    private float time;

    /// <Summary>=====================================
    /// 説明文とスタートを表記するための変数
    /// </Summary>>===================================
    ///
    // 説明文のゲームオブジェクト
    [Header("説明文表記のゲームオブジェクトを設置"), SerializeField]
    private GameObject introductionTextObj;

    // スタート表記のゲームオブジェクト
    [Header("スタート表記のゲームオブジェクトを設置"), SerializeField]
    private GameObject startImageObj;

    // 説明文が消えるまでのストッパー
    private bool introductionStopper = true;
    // スタートが消えるまでのストッパー
    private bool startStopper = true;

    /// <Summary>
    /// 主にUpdateで使用するもの
    /// </Summary>
    /// 
    //クリックされたゲームオブジェクトを代入する変数
    private GameObject clickedGameObject;

    // 表示中の問題テキスト格納用
    private GameObject activeQuestionText;

    // 問題の出題数を記録
    private int questionNum = 0;

    // 問題テキストのタグをStringで取得
    private string questionTag;

    // 出題のストッパー
    private bool questionStop = false;

    // 出題用テキストの要素の乱数用
    private int randQuestionNum;

    // Start is called before the first frame update
    void Start()
    {
        // score用表示のテキストコンポーネントを用意
        scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
        // AudioSourceのコンポーネント準備
        audioSource = this.GetComponent<AudioSource>();

        // Resourcesのカルタフォルダからカルタを読み込む
        carutaCardRandom = Resources.LoadAll("CarutaCard", typeof(GameObject)).Cast<GameObject>().ToArray();

        /// <Summary>=====================================
        /// リソースファイルにあるカードすべてをシャッフル
        /// シャッフルしたカードを出題用の８枚に設定する
        /// カルタのモデルと出題テキストは一緒になっているため、
        /// それぞれ出現させたら、非表示にさせる。
        /// </Summary>>=====================================

        // Resourcesから取得したカードをいったんシャッフルする。
        int n = carutaCardRandom.Length;

        // フィッシャー・イエーツのシャッフルアルゴリズムを実装
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            // 選択した要素と最後の要素を交換
            GameObject tmp = carutaCardRandom[i];
            carutaCardRandom[i] = carutaCardRandom[j];
            carutaCardRandom[j] = tmp;

        }

        // 出題用配列にランダムしたものを８枚まで配列に挿入
        for (int h = 0; h < curutaCardRandomAfter.Length; h++)
        {
            curutaCardRandomAfter[h] = carutaCardRandom[h];
        }

        int o = 0;
        // カルタ表示：問題は非表示
        // DoTween・Instantiate用の配列に格納
        foreach (GameObject carutaTween in curutaCardRandomAfter)
        {
            // Quaternionの値がひっくり返っているときの値
            var caruta = Instantiate(carutaTween, new Vector3(cardStartDeck.transform.position.x, cardStartDeck.transform.position.y, cardStartDeck.transform.position.z), Quaternion.Euler(-180, 180, -90));
            tweenInstantiateCaruta[o] = caruta;
            // Canvasのコンポーネントを非表示 上から数えて1がcanvas
            tweenInstantiateCaruta[o].transform.GetChild(1).gameObject.GetComponent<Canvas>().enabled = false;
            //Debug.Log("tweenInstantiateCaruta : " + tweenInstantiateCaruta[o]);
            o++;
        }

        o = 0;
        // カルタ配置
        foreach (GameObject carutaOpen in tweenInstantiateCaruta)
        {
            carutaOpen.transform.position = new Vector3(carutataFieldsObj[o].transform.position.x, carutataFieldsObj[o].transform.position.y, carutataFieldsObj[o].transform.position.z);
            // カルタオープンの際のQuaternionの数値
            carutaOpen.transform.rotation = Quaternion.Euler(-180, 180, 90);
            //carutaOpen.transform.parent = carutataFieldsObj[o].transform;
            o++;
        }

        ///<Summary>=====================================
        ///問題の乱数を作成
        ///</Summary>>=====================================
        ///
        for (int num = 0; num < numbers.Length; num++)
        {
            numbers[num] = num;
        }

        n = numbers.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            // 選択した要素と最後の要素を交換
            int tmp2 = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = tmp2;
        }

        /// <Summary>
        /// 制限時間の設定
        /// 60をかけて分に直す
        /// textをgetcomponentして反映する。
        /// </Summary>>
        /// 
        timerText = timerTextObj.GetComponent<TextMeshProUGUI>();
        timeLimitSeconds = timeLimitSeconds + timeLimitMinutes * 60;

        // スコアの表記用初期化
        scoreText.text = scoreInt.ToString();
        
    }

    // Update is called once per frame
    void Update()
    {          
        // 説明文の表示が終わるまでは処理を開始させない
        if (!startStopper && !introductionStopper)
        {
            /// <Summary>
            /// 制限時間の始まり
            /// 時間の表記
            /// </Summary>>
            ///
            timeLimitSeconds -= Time.deltaTime;
            var span = new TimeSpan(0, 0, (int)timeLimitSeconds);
            timerText.text = span.ToString(@"mm\:ss");

            /// <Summary>
            /// 問題テキストがなくなるまで続ける
            /// questionNumが8を超えたら
            /// また、時間切れでも終了
            /// 
            /// </Summary>>
            ///
            if (questionNum < 8 && timeLimitSeconds >= 0)
            {
                if (!questionStop)
                {
                    /// <Summary>
                    /// 問題の用意
                    /// カルタの部分は非表示
                    /// </Summary>>
                    /// 

                    int num = numbers[questionNum];
                    activeQuestionText = Instantiate(carutaCardRandom[num], new Vector3(cardStartDeck.transform.position.x, cardStartDeck.transform.position.y, cardStartDeck.transform.position.z), Quaternion.identity);
                    // いらない部分は非アクティブ
                    activeQuestionText.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
                    activeQuestionText.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;
                    questionTag = activeQuestionText.gameObject.tag;
                    // Instantiateをストップ
                    questionStop = true;
                }
                else
                {
                    /// <Summary>
                    /// タッチ操作処理（今はクリック）
                    /// タッチしたタグを取得して問題テキストと一致したら正解 → ストッパーを外す（questionStop = false）
                    /// 一致しなかったら不正解
                    /// 地面だったら何もしない
                    /// </Summary>> 

                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit = new RaycastHit();
                        if (Physics.Raycast(ray, out hit))
                        {
                            clickedGameObject = hit.collider.gameObject;
                            Debug.Log(clickedGameObject.gameObject.tag);//ゲームオブジェクトのタグを出力
                            if (clickedGameObject.gameObject.tag == questionTag)
                            {
                                Debug.Log("正解");
                                Destroy(clickedGameObject);
                                scoreInt += 10;
                                scoreText.text = scoreInt.ToString(); 
                                activeQuestionText.transform.GetChild(1).gameObject.GetComponent<Canvas>().enabled = false;
                                questionNum++;
                                questionStop = false;

                            }
                            else if (clickedGameObject.gameObject.tag == "Ground")
                            {
                                Debug.Log("カード以外タッチ");
                            }
                            else
                            {
                                Debug.Log("不正解");
                                if (scoreInt > 0)
                                {
                                    scoreInt -= 10;
                                    scoreText.text = scoreInt.ToString();
                                }   
                            }
                        }
                    }
                }
            }
            else
            {
                // ゲーム終了処理
                Debug.Log("ゲーム終了");

                SceneManager.LoadScene("ResultScene");
            }
        }else
        {
            /// <Summary>=====================================
            /// 説明文とスタートを表示
            /// コルーチンへ移動（待機）
            /// </Summary>=====================================
            /// 
            if (introductionStopper)
            {
                StartCoroutine(IntroductionActive());
            }

            if (!introductionStopper && startStopper)
            {
                StartCoroutine(StartActive());
            }          
        }
    }

    /// <summary>=====================================
    /// 説明文表示用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator IntroductionActive()
    {
        // 説明文を表示
        introductionTextObj.SetActive(true);
        // 4秒表示
        yield return new WaitForSeconds(4);
        // 説明文は非表示、スタートは表示
        introductionTextObj.SetActive(false);
        introductionStopper = false;
    }

    /// <summary>=====================================
    /// スタート表示用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator StartActive()
    {
        startImageObj.SetActive(true);
        audioSource.PlayOneShot(startSound);
        yield return new WaitForSeconds(2);       
        startImageObj.SetActive(false);
        startStopper = false;
    }
}

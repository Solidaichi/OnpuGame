using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CardMemoryGameController : MonoBehaviour
{
    // カードの出現初期位置を設定（Deck）
    [Header("カードの出現初期位置を設定（Deck）"), SerializeField]
    private GameObject cardStartDeck;

    // カード展開時の音を設定
    [Header("カードを配置する音を設定"), SerializeField]
    private AudioClip cardSettingSound;

    // カードフィールドを設定
    [Header("カードフィールドを設定(配置)"), SerializeField]
    private GameObject[] cardFieldsObj;

    // 意味カードフィールドを設定
    [Header("意味カードフィールドを設定(配置)"), SerializeField]
    private GameObject[] meanFieldsObj;

    // Resourcesのカードをすべて配列に保管
    private GameObject[] cardResourceRandom = new GameObject[10];

    // Resourcesの意味カードをすべて配列に保管
    private GameObject[] meanResourceRandom = new GameObject[10];

    // スコア加点用変数
    [HideInInspector]
    public static int memoryGameScoreInt;

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

    // 問題の出題数を記録
    private int questionNum = 0;

     [Header("カードタップ用のAudioSourceが入ったゲームオブジェクトを設定"), SerializeField]
    private GameObject audioSourceObj;
    // カードタップ時の音を設定
    [Header("カードをタップする音を設定"), SerializeField]
    private AudioClip cardTapSound;

    /// <Summary>====================================
    /// 正解用テキスト表示
    /// </Summary>====================================
    [Header("正解表示用テキスト（ゲームオブジェクト）設定"), SerializeField]
    private GameObject correctTextObj;

    // 正解した時の音
    private AudioSource audioSource2;
    [Header("正解した時の音"), SerializeField]
    private AudioClip correctSound;


    /// <Summary>=====================================
    /// 主にUpdateで使用するもの
    /// </Summary>=====================================
    /// 
    //クリックされたゲームオブジェクトを代入する変数
    private GameObject clickedGameObject;
    // 1回目クリックされた際のサイン
    private bool clickedOne;
    private bool clickedTwo;

    // ゲーム自体が開始するためのストッパー
    private bool startGameBool = true;

    // タッチしたカードを入れておく
    private GameObject[] touchObj = new GameObject[2];

    // カード開閉の演出時間の設定変数
    [Header("カード開閉制限時間の設定(秒単位：float)"), SerializeField]
    private float openCloseLimitSeconds = 0f;

    // 経過時間
    private float gameOpenCloseTime;


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
    // スコア表記用テキスト・ゲームオブジェクト
    [Header("スコア表記用テキスト（ゲームオブジェクト設置）"), SerializeField]
    private GameObject scoreTextObj;

    private TextMeshProUGUI scoreText;

    // 説明文とスタート表記の時間設定
    [Header("説明文の表示時間表記（秒数：float）"), SerializeField]
    private float introductionLimitSeconds = 0f;

    [Header("スタートの表示時間表記（秒数：float）"), SerializeField]
    private float startLimitSeconds = 0f;

    // 経過時間
    private float firstTime = 0;

    // カードスタート時の音を設定
    [Header("ゲームをスタートする音を設定"), SerializeField]
    private AudioClip startSound;

    // 表記が点滅しないようにストッパーをかける
    private bool introductionStopper = false;
    private bool startStopper = false;

    // Start is called before the first frame update
    void Start()
    {
        // スコアを初期化
        memoryGameScoreInt = 0;

        // 経過時間の初期化
        gameOpenCloseTime = 0;

        clickedOne = false;
        clickedTwo = false;

        audioSource2 = audioSourceObj.GetComponent<AudioSource>();

        // score用表示のテキストコンポーネントを用意
        scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();

        // Resourcesのカードフォルダからカルタを読み込む
        cardResourceRandom = Resources.LoadAll<GameObject>("MemoryGameCard").ToArray();
        // Resourcesの意味カードフォルダからカルタを読み込む
        meanResourceRandom = Resources.LoadAll<GameObject>("MemoryGameMean").ToArray();
        
        // Shuffleメソッドからそれぞれ取得
        cardResourceRandom = Shuffle(cardResourceRandom);
        meanResourceRandom = Shuffle(meanResourceRandom);

        // デッキにカードを配置
        DeckPreparationCard(cardResourceRandom);
        DeckPreparationCard(meanResourceRandom);
        audioSource2.PlayOneShot(cardSettingSound);
        // 左右に配置
        LeftandRightDeployment();

        /// <Summary>
        /// 制限時間の設定
        /// 60をかけて分に直す
        /// textをgetcomponentして反映する。
        /// </Summary>>
        /// 
        timerText = timerTextObj.GetComponent<TextMeshProUGUI>();
        timeLimitSeconds = timeLimitSeconds + timeLimitMinutes * 60;
        

    }

    // Update is called once per frame
    void Update()
    {
        Invoke("IntroductionText", 4.0f);
        Invoke("StartText", 10.0f);

        if (!startGameBool) {
            /// <Summary>=====================================
            /// 制限時間の始まり
            /// 時間の表記
            /// </Summary>>=====================================
            ///
            timeLimitSeconds -= Time.deltaTime;
            var span = new TimeSpan(0, 0, (int)timeLimitSeconds);
            timerText.text = span.ToString(@"mm\:ss");

            /// <Summary>
            /// 問題テキストがなくなるまで続ける
            /// questionNumが8を超えたら
            /// また、時間切れでも終了
            /// </Summary>>
            ///
            if (questionNum < 8 && timeLimitSeconds >= 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit))
                    {
                        // 1回目のタッチ
                        if (touchObj[0] == null && !clickedTwo) {
                            audioSource2.PlayOneShot(cardTapSound);
                            touchObj[0] = hit.collider.gameObject;
                            touchObj[0].transform.DOLocalRotate(new Vector3(-180, 180, 90), 0.5f).SetEase(Ease.Linear);
                            clickedOne = true;
                            
                        }else if (clickedOne && touchObj[1] == null) {
                            audioSource2.PlayOneShot(cardTapSound);
                            touchObj[1] = hit.collider.gameObject;
                            touchObj[1].transform.DOLocalRotate(new Vector3(-180, 180, 90), 0.5f).SetEase(Ease.Linear);
                            clickedTwo = true;
                            
                        }
                    }
                }

                Debug.Log("clicked: " + clickedOne + clickedTwo);
                // 2回目タッチが終わった後正誤判定
                if (clickedOne && clickedTwo) {
                    if (touchObj[0].gameObject.tag == touchObj[1].gameObject.tag) {
                        Debug.Log("正解");
                        if (gameOpenCloseTime > openCloseLimitSeconds) {
                            StartCoroutine(CorrectMessage());
                            memoryGameScoreInt += 20;
                            scoreText.text = memoryGameScoreInt.ToString(); 
                            CloseCardGame(touchObj[0]);
                            CloseCardGame(touchObj[1]);
                            clickedOne = false;
                            clickedTwo = false;
                            Destroy(touchObj[0]);
                            Destroy(touchObj[1]);
                            touchObj[0] = null;
                            touchObj[1] = null;
                            gameOpenCloseTime = 0;
                        }else {
                            gameOpenCloseTime += Time.deltaTime;
                        }
                    }else {
                        Debug.Log("不正解");
                        if (gameOpenCloseTime > openCloseLimitSeconds) {
                            audioSource2.PlayOneShot(cardTapSound);
                            CloseCardGame(touchObj[0]);
                            CloseCardGame(touchObj[1]);
                            clickedOne = false;
                            clickedTwo = false;
                            touchObj[0] = null;
                            touchObj[1] = null;
                            gameOpenCloseTime = 0;
                        }else {
                            gameOpenCloseTime += Time.deltaTime;
                        }
                        
                    }
                }     
            }else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
            Application.Quit();//ゲームプレイ終了
#endif
            }
        } 
    }

    private GameObject[] Shuffle (GameObject[] gameObjectList) {
        // Resourcesから取得したカードをいったんシャッフルする。
        int n = meanResourceRandom.Length;

        // フィッシャー・イエーツのシャッフルアルゴリズムを実装
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            // 選択した要素と最後の要素を交換
            GameObject tmp = gameObjectList[i];
            gameObjectList[i] = gameObjectList[j];
            gameObjectList[j] = tmp;
        }

        return gameObjectList;
    }

    /// <Summary>=====================================
    /// デッキにカードを配置する（ゲーム上に表示）
    /// </Summary>>=====================================
    private void DeckPreparationCard(GameObject[] gameObjectList) {        
        int o = 0;
        foreach(GameObject obj in gameObjectList) {
            var card = Instantiate(obj, new Vector3(cardStartDeck.transform.position.x, cardStartDeck.transform.position.y, cardStartDeck.transform.position.z), Quaternion.Euler(-180, 180, -90));
            gameObjectList[o] = card;
            o++;
        }
    }

    /// <Summary>=====================================
    /// カードと意味カードをそれぞれ左右に展開するメソッド
    /// </Summary>>=====================================
    private void LeftandRightDeployment() {
        int o = 0;
        foreach(GameObject obj in cardResourceRandom) {
            obj.transform.DOMove(new Vector3(cardFieldsObj[o].transform.position.x, cardFieldsObj[o].transform.position.y+0.3f, cardFieldsObj[o].transform.position.z), 2f);
            o++;
        }

        int l = 0;
        foreach(GameObject obj in meanResourceRandom) {
            obj.transform.DOMove(new Vector3(meanFieldsObj[l].transform.position.x, meanFieldsObj[l].transform.position.y+0.3f, meanFieldsObj[l].transform.position.z), 2f);
            l++;
        }
    }

    private void IntroductionText() {
        if (firstTime <= introductionLimitSeconds) {
            firstTime += Time.deltaTime;
            if(!introductionStopper) {
                introductionTextObj.SetActive(true);
                introductionStopper = true;
            }
        }else {
           introductionTextObj.SetActive(false);
           firstTime = 0f; 
        }
    }

    private void StartText() {
         if (firstTime <= startLimitSeconds) {
            firstTime += Time.deltaTime;
            if(!startStopper) {
                audioSource2.PlayOneShot(startSound);
                startImageObj.SetActive(true);
                startStopper = true;
            }
        }else {
           startImageObj.SetActive(false);
           firstTime = 0f; 
           startGameBool = false;
        }
    }

    private void CloseCardGame(GameObject gameObj) {
        gameObj.transform.DOLocalRotate(new Vector3(0, 0,90), 1f).SetEase(Ease.Linear);
    }

    /// <summary>=====================================
    /// 正解表示用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator CorrectMessage() 
    {
        correctTextObj.SetActive(true);
        audioSource2.PlayOneShot(correctSound);
        yield return new WaitForSeconds(2);
        correctTextObj.SetActive(false);
    }
}

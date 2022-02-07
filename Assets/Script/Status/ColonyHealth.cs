using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

using static Call.ConstantValue;

public class ColonyHealth : MonoBehaviour
{
    private float maxHp = 5000.0f; //最大HP
    public static float currentHp; //現在のHP
    public static int colonyLevel = 0; //コロニーレベル
    public static float exp = 0.0f; //現在の経験値

    private PlayableDirector playableDirector;
    [SerializeField] private GameObject time;
    [SerializeField] private GameObject levelUpTimeline;
    [SerializeField] private GameObject levelupText;
    [SerializeField] private GameObject levelTextObjcet;
    [SerializeField] private Slider slider; //Slider格納
    [SerializeField] private ParticleSystem fire; //炎パーティクル
    private ParticleSystem fireEffect; //炎エフェクト
    private Text levelText;
    private bool isFire; //炎エフェクトフラグ
    public static bool isLevelUp = false;

    //レベルごとの経験値リスト（必要経験値）
    public static readonly List<float> EXP_LIST = new List<float>{
        100, 550, 1000, 2500, 4000,
        6500, 9000, 12000, 15000, 18000,
        22000, 27000, 33000, 40000, 50000,
        60000, 70000, 80000, 90000, 100000,
        };

    //レベルごとの力の重み
    public static readonly List<float> FORCE_WEIGHT = new List<float>
    {
        1.05f, 1.10f, 1.20f, 1.30f, 1.40f,
        1.55f, 1.70f, 1.85f, 2.00f, 2.20f,
        2.40f, 2.60f, 2.80f, 3.00f, 3.20f,
        3.40f, 3.60f, 3.90f, 4.40f, 5.00f,
    };

    private void Awake()
    {
        playableDirector = time.GetComponent<PlayableDirector>();
        levelUpTimeline.SetActive(false);
        levelupText.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        slider.value = 1; //Sliderを満タン
        currentHp = maxHp; //現在のHPに最大HPを代入
        isFire = false;
        levelText = levelTextObjcet.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("経験値" + exp);
        levelText.text = colonyLevel.ToString(); //レベル表示
        StartLevelEffect(); //レベルアップエフェクト開始

        /* Health管理 */
        slider.value = currentHp / maxHp; //HPスライダーを更新

        if (currentHp > 1000.0f) return;
        GetFireEffect(); //炎エフェクトを取得

        if (currentHp > 0.0f) return;
        SceneManager.LoadScene("LOSE"); //ゲームオーバー
    }

    /// <summary>
    /// 炎エフェクトを取得
    /// </summary>
    private void GetFireEffect()
    {
        if (isFire) return;
        fireEffect = Instantiate(fire);
        fireEffect.transform.position = FIRE_POS;
        fireEffect.transform.rotation = Quaternion.Euler(FIRE_ROT);
        fireEffect.Play(); //炎
        isFire = true;
    }

    private void StartLevelEffect()
    {
        if (!isLevelUp) return;
        levelUpTimeline.SetActive(true);
        levelupText.SetActive(true);

        playableDirector.Play();
        isLevelUp = false; //フラグを戻す
    }
}

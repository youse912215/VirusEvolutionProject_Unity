using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Call.ConstantValue;
using static Call.CommonFunction;
using static Call.VirusData;
using static RAND.CreateRandom;
using static VirusMaterialData;
using static WarriorData;
using static PrepareVirus;
using static ColonyHealth;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    /* private */
    [SerializeField] private Slider slider; //スライダー
    [SerializeField] private ParticleSystem impactPs; //衝撃波のパーティクルシステム
    [SerializeField] private ParticleSystem bloodPs; //血しぶきのパーティクルシステム
    [SerializeField] private ParticleSystem steamPs; //steamのパーティクルシステム
    [SerializeField] private ParticleSystem jewelPs; //宝石のパーティクルシステム

    private ParticleSystem impactEffect; //衝撃波のエフェクト
    private ParticleSystem bloodEffect; //衝撃波のエフェクト
    private ParticleSystem steamEffect; //スチームのエフェクト
    private ParticleSystem jewelEffect; //宝石のエフェクト

    private int enemyRank; //階級
    private const int MAX_RANK = 5; //最大階級
    private float currentHp; //現在のHP
    private float maxHp; //最大HP
    private const float INIT_HEALTH = 300.0f; //初期HP固定値
    private const float HEALTH_WEIGHT = 500.0f;
    private const float ATTACK_DAMAGE = 1.5f; //攻撃力
    private bool isImpactSet; //衝撃エフェクトをセットしたかどうか
    private const float IMPACT_POS_Z = -170.0f; //衝撃エフェクトのZ座標
    private const float EFFECT_HEIGHT = 250.0f; //エフェクト高さ
    private const int ENEMEY1_LAYER = 7; //敵1のレイヤ番号
    private readonly Vector3 STEAM_POS = new Vector3(-65.0f, 145.0f, -180.0f);
    private int getCount = 0;
    private int getMaterial = 99;
    private const int MAX_DROP = 5; //最大ドロップ数
    private List<bool> isVirusDamage = new List<bool> { false, false, false, false, false, false, false, false };
    private float pDefence = 0.0f; //貫通防御

    /* public */
    public uint takenDamage; //被ダメージ
    public float totalDamage; //合計ダメージ
    public bool isInfection; //感染したかどうか
    public bool isDead; //死んだかどうか

    private MoveEnemy mE;
    private DamageManager dM;
    private Vector3 newPos;

    /// <summary>
    /// 開始処理
    /// </summary>
    void Start()
    {
        slider.value = 1; //Sliderを満タン
        takenDamage = 0b0000;
        totalDamage = 0.0f;
        isImpactSet = false;
        impactEffect = Instantiate(impactPs); //エフェクト生成
        impactEffect.Stop(); //エフェクト停止

        mE = this.gameObject.GetComponent<MoveEnemy>();
        dM = this.gameObject.GetComponent<DamageManager>();

        enemyRank = (int)Integerization(rand) % MAX_RANK; //階級を取得
        pDefence = GetArmor(); //リストから貫通防御を取得
        maxHp = INIT_HEALTH * (enemyRank + 1) + HEALTH_WEIGHT * WaveGauge.currentDay; //最大HPを取得
        currentHp = maxHp; //現在のHPに最大HPを代入

        if (this.gameObject.layer != ENEMEY1_LAYER) return; //対象レイヤー以外は、処理をスキップ
        steamEffect = Instantiate(steamPs); //エフェクト生成
        steamEffect.Play(); //エフェクト開始

        steamEffect.transform.rotation =
                Quaternion.Euler(new Vector3(180.0f, QUARTER_CIRCLE * mE.startPos, 0.0f));
        newPos = new Vector3(-150.0f * mE.startPos, 0, 0);
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        UpdateSteamEffect(); //スチームエフェクト更新
        AttackAction(); //攻撃時行動

        if (!isInfection) return; //感染時以外は、処理をスキップ
        InfectionAction(); //ウイルス感染時行動

        if (currentHp > 0.0f) return; //生きている間は、処理をスキップ
        DropMaterial(); //素材をドロップ
        DeadAction(); //死亡時行動
    }

    private float GetArmor()
    {
        if (this.gameObject.layer == ENEMEY1_LAYER)
            return PENETRATION_DEFENCE_LIST0[enemyRank];
        return PENETRATION_DEFENCE_LIST1[enemyRank];
    }

    /// <summary>
    /// 体力を計算する
    /// </summary>
    /// <returns></returns>
    public float CulculationHealth(int type)
    {
        var d = new float[SET_LIST_COUNT];
        //0
        isVirusDamage[virusSetList[0]] = (type == virusSetList[0]) ? true : false;
        d[0] = isVirusDamage[virusSetList[0]] ? FORCE_WEIGHT[colonyLevel] * force[virusSetList[0]].x : 0.0f;
        //1
        isVirusDamage[virusSetList[1]] = (type == virusSetList[1]) ? true : false;
        d[1] = isVirusDamage[virusSetList[1]] ? FORCE_WEIGHT[colonyLevel] * force[virusSetList[1]].x : 0.0f;
        //2
        isVirusDamage[virusSetList[2]] = (type == virusSetList[2]) ? true : false;
        d[2] = isVirusDamage[virusSetList[2]] ? FORCE_WEIGHT[colonyLevel] * force[virusSetList[2]].x : 0.0f;
        return d[0] + d[1] + d[2];
    }

    /// <summary>
    /// 攻撃時行動
    /// </summary>
    private void AttackAction()
    {
        if (transform.position.z <= TARGET_POS)
        {
            ColonyHealth.currentHp -= Integerization(rand) % ATTACK_DAMAGE; //コロニーへの攻撃

            if (isImpactSet) return; //衝撃波をセットしているなら、処理をスキップ            
            if (mE.startPos != 0) StartCoroutine("RotationBody");
            SetEffectPos(impactEffect, IMPACT_POS_Z); //エフェクトの位置をセット
            impactEffect.Play(); //衝撃波エフェクト
            isImpactSet = true; //セットフラグをtrue
            //GetComponent<AudioSource>().Play();
        }
    }

    /// <summary>
    /// ウイルス感染時行動
    /// </summary>
    private void InfectionAction()
    {
        currentHp -= totalDamage; //ウイルスの合計ダメージ分、HPを減らす
        slider.value = currentHp / maxHp; //HPバーの計算
    }

    /// <summary>
    /// 死亡時行動
    /// </summary>
    private void DeadAction()
    {
        if (!gameObject) return;
        exp += dM.GetExp(enemyRank); //経験値取得
        colonyLevel += dM.CulculationColonyLevel(); //コロニーレベルを計算
        deadCount++; //累計の死亡数をカウント
        SetStopAction(impactEffect, true); //衝撃エフェクトを削除
        if (this.gameObject.layer == ENEMEY1_LAYER)
            SetStopAction(steamEffect, true); //敵1のみ、スチームエフェクトを削除  
        DeadEffect(bloodPs, bloodEffect); //血エフェクト
        DeadEffect(jewelPs, jewelEffect); //宝石エフェクト
        Destroy(gameObject); //敵オブジェクトを削除
    }

    /// <summary>
    /// エフェクトの停止設定
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="isLoop"></param>
    private void SetStopAction(ParticleSystem effect, bool isLoop)
    {
        var iMain = effect.main;
        iMain.loop = false;
        iMain.stopAction = ParticleSystemStopAction.Destroy;
        if (!isLoop) return;
        effect.Stop();
    }

    /// <summary>
    /// 死亡時エフェクト
    /// </summary>
    /// <param name="ps"></param>
    /// <param name="effect"></param>
    private void DeadEffect(ParticleSystem ps, ParticleSystem effect)
    {
        effect = Instantiate(ps); //エフェクト生成
        SetEffectPos(effect); //位置をセット   
        SetStopAction(effect, false);  
        effect.Play(); //エフェクト発生
    }

    /// <summary>
    /// エフェクトの位置をセットする
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="diffZ"></param>
    private void SetEffectPos(ParticleSystem effect, float diffZ = 0.0f)
    {
        var effectPos = new Vector3(
                transform.position.x,
                transform.position.y + EFFECT_HEIGHT,
                transform.position.z + diffZ);
        effect.transform.position = effectPos;
    }

    /// <summary>
    /// スチームエフェクト更新
    /// </summary>
    private void UpdateSteamEffect()
    {
        if (this.gameObject.layer != ENEMEY1_LAYER) return; //対象レイヤー以外は、処理をスキップ
        steamEffect.transform.position = transform.position + STEAM_POS
            + newPos; //更新
    }

    /// <summary>
    /// 体を回転する
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotationBody()
    {
        //条件時の間繰り返す
        while ((mE.startPos == -1 && this.gameObject.transform.rotation.y <= 0.01f)
            || (mE.startPos == 1 && this.gameObject.transform.rotation.y >= -0.01f))
        {
            yield return new WaitForSeconds(0.1f); //0.1f待つ

            //対象が敵1レイヤーのとき
            if (this.gameObject.layer == ENEMEY1_LAYER)
            {
                newPos += new Vector3(mE.startPos * 15.0f, 0, 0); //座標を更新
                steamEffect.transform.Rotate(0, mE.startPos * 5.0f, 0); //スチームエフェクトを回転
            }
            this.gameObject.transform.Rotate(0, mE.startPos * -5.0f, 0); //敵オブジェクトを回転
        }
    }

    //アイテムを落とす処理
    private void DropMaterial()
    {
        getCount = (int)Integerization(rand) % MAX_DROP + 1; //1~MAX_DROP個取得
        getMaterial = (int)Integerization(rand) % vMatNam; //素材番号を取得
        vMatOwned[getMaterial] += getCount; //所持素材リストに加える
        //Debug.Log(VIRUS_NAME[getMaterial] + "を" + getCount + "個入手");
    }
}
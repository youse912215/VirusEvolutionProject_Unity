using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Call.ConstantValue;
using static Call.CommonFunction;
using static Call.VirusData;
using static RAND.CreateRandom;
using static WarriorData;

public class EnemyHealth : MonoBehaviour
{
    /* private */
    [SerializeField] private Slider slider; //スライダー
    [SerializeField] private ParticleSystem impactPs; //衝撃波のパーティクルシステム
    [SerializeField] private ParticleSystem bloodPs; //血しぶきのパーティクルシステム
    [SerializeField] private ParticleSystem steamPs; //steamのパーティクルシステム
    [SerializeField] private Material mat;

    private ParticleSystem impactEffect; //衝撃波のエフェクト
    private ParticleSystem bloodEffect; //衝撃波のエフェクト
    private ParticleSystem steamEffect;

    private float currentHp; //現在のHP
    private float maxHp; //最大HP
    private const float INIT_HEALTH = 0.5f;
    private const float ATTACK_DAMAGE = 1.5f; //攻撃力
    private bool isImpactSet; //衝撃エフェクトをセットしたかどうか
    private const float IMPACT_POS_Z = -170.0f; //衝撃エフェクトのZ座標
    private const float EFFECT_HEIGHT = 250.0f; //エフェクト高さ
    private const int ENEMEY1_LAYER = 7; //敵1のレイヤ番号
    private readonly Vector3 STEAM_POS = new Vector3(-65.0f, 145.0f, -180.0f);

    /* public */
    public uint takenDamage; //被ダメージ
    public float totalDamage; //合計ダメージ
    public bool isInfection; //感染したかどうか
    public bool isDead; //死んだかどうか

    void Start()
    {
        slider.value = 1; //Sliderを満タン
        maxHp = Integerization(rand) * INIT_HEALTH; //最大HPをランダムで取得
        currentHp = maxHp; //現在のHPに最大HPを代入
        takenDamage = 0b0000;
        totalDamage = 0.0f;
        isImpactSet = false;
        impactEffect = Instantiate(impactPs); //エフェクト生成
        impactEffect.Stop(); //エフェクト停止
        bloodEffect = Instantiate(bloodPs); //エフェクト生成
        bloodEffect.Stop(); //エフェクト停止
        
        if (this.gameObject.layer != ENEMEY1_LAYER) return; //対象レイヤー以外は、処理をスキップ
        steamEffect = Instantiate(steamPs); //エフェクト生成
        steamEffect.Play(); //エフェクト開始
    }

    void Update()
    {
        UpdateSteamEffect(); //スチームエフェクト更新
        AttackAction(); //攻撃時行動

        if (!isInfection) return; //感染時以外は、処理をスキップ
        InfectionAction(); //ウイルス感染時行動

        if (currentHp > 0.0f) return; //生きている間は、処理をスキップ
        DeadAction(); //死亡時行動
    }

    /// <summary>
    /// 体力を計算する
    /// </summary>
    /// <param name="damage">ダメージ</param>
    /// <returns></returns>
    public int CulculationHealth(uint damage)
    {
        uint d0 = ((damage & 0b0001) >> 0) * (uint)force[0].x; //
        uint d1 = ((damage & 0b0010) >> 1) * (uint)force[1].x; //
        uint d2 = ((damage & 0b0100) >> 2) * (uint)force[2].x; //
        return (int)(d0 + d1 + d2);
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
        deadCount++; //累計の死亡数をカウント
        Destroy(impactEffect); //衝撃波エフェクトを削除
        SetEffectPos(bloodEffect); //エフェクトの位置をセット
        Destroy(steamEffect); //steamエフェクトを削除
        bloodEffect.Play(); //血のエフェクト
        Destroy(gameObject); //オブジェクトを削除
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
        steamEffect.transform.position = transform.position + STEAM_POS; //更新
    }
}
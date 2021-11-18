using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Call.ConstantValue;
using static Call.VirusData;

public class EnemyHealth : MonoBehaviour
{
    //最大HPと現在のHP。
    private float maxHp = 2000;
    private float currentHp;
    //Sliderを入れる
    [SerializeField]
    private Slider slider;

    public bool isInfection;
    public uint damage;
    public float total;

    void Start()
    {   
        slider.value = 1; //Sliderを満タン
        currentHp = maxHp; //現在のHPに最大HPを代入
        damage = 0b0000;
        total = 0.0f;
    }

    void Update()
    {
        if (transform.position.z <= TARGET_POS) ColonyHealth.currentHp -= 0.5f;

        if (!isInfection) return;

        currentHp -= total;
        slider.value = currentHp / maxHp;
    }

    public int CulculationHealth(uint damage)
    {
        uint d0 = ((damage & 0b0001) >> 0) * 1;
        uint d1 = ((damage & 0b0010) >> 1) * 2;
        uint d2 = ((damage & 0b0100) >> 2) * 3;
        return (int)(d0 + d1 + d2);
    }
}
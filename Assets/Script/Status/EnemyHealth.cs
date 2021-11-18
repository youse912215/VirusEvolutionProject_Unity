using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Call.ConstantValue;

public class EnemyHealth : MonoBehaviour
{
    //最大HPと現在のHP。
    float maxHp = 1550;
    float currentHp;
    //Sliderを入れる
    [SerializeField]
    private Slider slider;

    void Start()
    {   
        slider.value = 1; //Sliderを満タン
        currentHp = maxHp; //現在のHPに最大HPを代入
    }

    void Update()
    {
        if (transform.position.z <= TARGET_POS)
        {
            currentHp -= 5;
            slider.value = currentHp / maxHp; ;
        }
    }
}
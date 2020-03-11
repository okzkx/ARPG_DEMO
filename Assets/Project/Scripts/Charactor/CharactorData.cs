using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorData : MonoBehaviour
{
    public static CharactorData Instance;

    //行动
    public float Speed = 5;
    public float JumpSpeed = 5;
    public float GroundCheckDistance = 0.1f;

    //战斗
    private int hp = 100;
    public int Hp {
        get => hp;
        set {
            hp = value;
            if (GameManager.DlgManager.TryGet<DlgCharactorData>(out var dlg))
            {
                dlg.SetHpSlider(hp * 1f / HpTotal);
            }
        }
    }
    public int HpTotal = 100;

    public int Damage = 10;

    private void Awake() => Instance = this;

}

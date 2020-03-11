using OKZKX.UnityTool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackController : AutoSetBehaviour, IAttackSenderController, IAttackReceiverController
{
    AttackMessage attackMessage;

    [AutoSet] AttackSender attackSender;
    [AutoSet] AttackReceiver attackReceiver;
    [AutoSet] EnemyData EnemyData;

    public Action OnAttackBeginEvent { get; set; }
    public Action OnAttackEndEvent { get; set; }
    public Action<object, AttackSender> OnAttackReceiveEvent { get; set; }

    private void Start()
    {
        attackMessage = new AttackMessage();
        attackMessage.Damege = EnemyData.Damage;

        attackSender.Init(LayerMask.GetMask("Charactor"), attackMessage, this);
        attackReceiver.Init(this);
    }

    public void OnAttackReceive(object arg1, AttackSender arg2)
    {
        AttackMessage attackMessage = (AttackMessage)arg1;
        Debug.Log("Enemy Get Hit: " + attackMessage.Damege);
        EnemyData.Hp -= attackMessage.Damege;
        OnAttackReceiveEvent?.Invoke(this.attackMessage, arg2);
    }
}

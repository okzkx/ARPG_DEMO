using OKZKX.UnityTool;
using System;
using UnityEngine;

public class CharactorAttackController : AutoSetLoadBehaviour, IAttackSenderController, IAttackReceiverController
{
    [AutoSet] CharactorData CharactorData;
    [AutoSet] AttackReceiver AttackReceiver;
    [AutoLoad] GameObject Staff;
    [AutoSet("")] Transform weaponHandler;

    AttackMessage attackMessage = new AttackMessage();

    public Action OnAttackBeginEvent { get; set; }
    public Action OnAttackEndEvent { get; set; }
    public Action<object, AttackSender> OnAttackReceiveEvent { get; set; }


    void Start()
    {
        attackMessage.Damege = CharactorData.Damage;
        AttackReceiver.Init(this);

        SwitchStaff();
    }

    public void OnAttackReceive(object arg1, AttackSender arg2)
    {
        AttackMessage attackMessage = (AttackMessage)arg1;
        Debug.Log("Charactor Get Hit: " + attackMessage.Damege);
        CharactorData.Hp -= attackMessage.Damege;
        OnAttackReceiveEvent?.Invoke(arg1, arg2);
    }

    void SwitchStaff()
    {
        GameObject staffGameObject = Instantiate(Staff, weaponHandler);

        AttackSender attackSender = staffGameObject.GetComponent<AttackSender>();
        attackSender.Init(LayerMask.GetMask("Enemy"), attackMessage, this);
    }
}

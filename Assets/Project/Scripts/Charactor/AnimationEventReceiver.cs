/**********************************************************************
* 角色动画事件接收
***********************************************************************/
using OKZKX.UnityTool;
using UnityEngine;

public class AnimationEventReceiver : AutoSetBehaviour
{
    [AutoSet] CharactorAttackController CharactorAttack;

    void MeleeAttackStart()
    {
        CharactorAttack.OnAttackBeginEvent?.Invoke();
    }

    void MeleeAttackEnd()
    {
        CharactorAttack.OnAttackEndEvent?.Invoke();
    }

    void PlayStep()
    {

    }

}

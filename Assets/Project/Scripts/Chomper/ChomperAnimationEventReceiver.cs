using OKZKX.UnityTool;
using UnityEngine;

public class ChomperAnimationEventReceiver : AutoSetBehaviour
{
    [AutoSet] EnemyAttackController EnemyAttack;
    void Grunt() { }

    void PlayStep() { }

    void AttackBegin() =>EnemyAttack.OnAttackBeginEvent?.Invoke();

    void AttackEnd() => EnemyAttack.OnAttackEndEvent?.Invoke();
}

using UnityEngine;

public class WolfAnimationEventReceiver : MonoBehaviour
{
    AttackManager AttackManager;
    EnemyStats stats;
    // Start is called before the first frame update
    void Start()
    {
        AttackManager = transform.parent.GetComponent<AttackManager>();
        stats = transform.parent.GetComponent<StateController>().enemyStats;
    }

    public void Hit()
    {
        AttackManager.ActivateHitbox(ToolType.Default, stats.attackDamage, stats.attackForce, stats.attackRange);
    }
    public void EndHit()
    {
        AttackManager.DeactivateHitbox();
    }
}

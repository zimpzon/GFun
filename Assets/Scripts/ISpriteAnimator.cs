using UnityEngine;

public interface ISpriteAnimator
{
    void UpdateAnimation(Vector3 latestMovementDirection, bool isDead = false, bool isAttacking = false);
}

using UnityEngine;

public class QuestGiverScript : MonoBehaviour
{
    SpriteAnimator_Single anim_;
    Transform transform_;

    void Start()
    {
        anim_ = GetComponent<SpriteAnimator_Single>();
        transform_ = transform;
    }

    void Update()
    {
        Vector3 directionToPlayer = AiBlackboard.Instance.PlayerPosition - transform_.position;
        anim_.UpdateAnimation(directionToPlayer);
    }
}

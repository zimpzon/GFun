using MEC;
using System.Collections.Generic;
using UnityEngine;

public class DamagingParticleSystem : MonoBehaviour
{
    public AudioClip EmitSound;
    public int Damage = 1;
    public Transform Enemy;
    public float DamageCooldown = 0.25f;
    [System.NonSerialized]public bool EnableEmission;

    IEnemy enemy_;
    PlayableCharacterScript player_;
    GameObject playerGO_;
    float timeNextDamage_;
    ParticleSystem system_;

    void Start()
    {
        enemy_ = Enemy.GetComponent<IEnemy>();
        player_ = PlayableCharacters.GetPlayerInScene();
        playerGO_ = player_.gameObject;
        system_ = GetComponent<ParticleSystem>();

        Timing.RunCoroutine(EmitCo().CancelWith(this.gameObject));
    }

    IEnumerator<float> EmitCo()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(1);

            while (!EnableEmission)
                yield return 0;

            for (int i = 0; i < 3; ++i)
            {
                yield return Timing.WaitForSeconds(0.1f);
                system_.Emit(4);
                AudioManager.Instance.PlaySfxClip(EmitSound, 3, 0.2f);
            }
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (playerGO_ == other && Time.time > timeNextDamage_)
        {
            player_.TakeDamage(enemy_, Damage, Vector3.zero);
            timeNextDamage_ = Time.time + DamageCooldown;
        }
    }
}

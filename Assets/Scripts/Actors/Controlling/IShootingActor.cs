using UnityEngine;

public interface IShootingActor
{
    void ShootAtPlayer();
    float ShootCdLeft { get; }
}

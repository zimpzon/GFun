﻿
using UnityEngine;

public interface IMovableActor
{
    void SetMovementVector(Vector3 vector, bool isNormalized = true);
}

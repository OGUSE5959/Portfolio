using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Space]
    [SerializeField] protected float _attackRate;
    protected float _attackTimer = 0f;
}

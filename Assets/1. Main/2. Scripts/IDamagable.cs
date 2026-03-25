using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum BodyParts
{
    Head,
    Body,
}

public interface IDamagable 
{
    public GameObject gameObject { get; }
    public Transform transform { get; }
    public bool IsDie { get; }
    // public void SetHit(IAttackable attacker, string message);
    public void SetHit(IAttackable attacker, float damage);
    public void SetHit(IAttackable attacker, float damage, string message);
    public void SetHit(IAttackable attacker, float damage, Vector3 hitSpot);
    public PhotonView PV { get; }
    public float Health { get; }
}

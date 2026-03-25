using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class HitParts : MonoBehaviour, IDamagable   // 힛파츠는 말 그대로 맞는 부위로 세세하게 대신 맞아주는 역할
{
    IDamagable _master;
    string _tag;

    public IDamagable Master { get { return _master; } }
    public bool IsDie => _master.IsDie;
    PhotonView IDamagable.PV => _master.PV;
    public float Health => _master.Health;

    public void SetHit(IAttackable attacker, float damage)
    {
        switch(_tag)
        {
            case "Head": damage *= 2.5f; break;
            case "Body": break;
            default: damage *= 0.8f; break;
        }
        _master.SetHit(attacker, damage, _tag);
    }
    public void SetHit(IAttackable attacker, float damage, string message) => _master.SetHit(attacker, damage, message);
    void IDamagable.SetHit(IAttackable attacker, float damage, Vector3 hitSpot) { }
    public void Initialize(IDamagable master)
    {
        _master = master;
        _tag = gameObject.tag;
    }
}

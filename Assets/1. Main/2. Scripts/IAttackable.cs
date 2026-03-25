using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public interface IAttackable
{
    public PhotonView PV { get; }
    public GameObject gameObject { get; }
    public Transform transform { get; }
    public IAttackable Entity { get; }  // 자기 자신
    public string Name { get; }
    public float Damage { get; }
}

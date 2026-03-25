using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum ThrowableType
{
    Grenade,
    FlashBang,

    Max
}
public interface IThrowable
{
    public ThrowableType ID { get; }
    public GameObject gameObject { get; }
    public Transform transform { get; }
    public PhotonView PV { get; }
    public bool IsMe { get; }
    public float EffectRadius { get; }
    public float DangerRadius { get; }

    public void OnDetectParts(HitParts parts) { }
    public void OnReleaseParts(HitParts parts) { }
}

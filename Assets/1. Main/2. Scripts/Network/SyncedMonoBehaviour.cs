using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
public class SyncedMonoBehaviour : MonoBehaviour
{
    [SerializeField] protected PhotonView _pv;

    public PhotonView PV => _pv;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;

    public void SyncedSetActive(bool value)
    {
        gameObject.SetActive(value);
        _pv.RPC("RPC_SetActive", RpcTarget.Others, value);
    }
    public void SyncedEnable() => SyncedSetActive(true);
    public void SyncedDisable() => SyncedSetActive(false);
    public void SyncedSetPosition(Vector3 position, bool isLocal = false)
    {
        RPC_SetPosition(position, isLocal);
        _pv.RPC("RPC_SetPosition", RpcTarget.Others, position, isLocal);
    }
    public void SyncedSetDirection(Vector3 dir)
    {
        RPC_SetDirection(dir);
        _pv.RPC("RPC_SetDirection", RpcTarget.Others, dir);
    }
    public void SyncedSetRotation(Vector3 euler, bool isLocal = false)
    {
        RPC_SetRotation(euler, isLocal);
        _pv.RPC("RPC_SetRotation", RpcTarget.Others, euler, isLocal);
    }
    public void SyncedSetRotation(Quaternion rot, bool isLocal = false)
    {
        RPC_SetRotation(rot, isLocal);
        _pv.RPC("RPC_SetRotation", RpcTarget.Others, rot, isLocal);
    }
    public void SyncedSetScale(Vector3 scale)
    {
        RPC_SetScale(scale);
        _pv.RPC("RPC_SetScale", RpcTarget.Others, scale);
    }
    public void SyncedSetTransform(Transform transform)
    {
        SyncedSetPosition(transform.position);
        SyncedSetRotation(transform.rotation.eulerAngles);
        SyncedSetScale(transform.localScale);
    }
    public void SyncedDestroy()
    {
        if(IsMasterClient) PhotonNetwork.Destroy(_pv);
        else _pv.RPC("RPC_SyncedDestroy", RpcTarget.Others);
    }
    public void SyncedDOMove(Vector3 pos, float duration)
    {
        RPC_SyncedDOMove(pos, duration);
        _pv.RPC("RPC_SyncedDOMove", RpcTarget.Others);
    }

    [PunRPC] protected void RPC_Enable() => gameObject.SetActive(true);
    [PunRPC] protected void RPC_Disable() => gameObject.SetActive(false);
    [PunRPC] protected void RPC_SetActive(bool value) => gameObject.SetActive(value);
    [PunRPC] protected void RPC_SetPosition(Vector3 position, bool isLocal)
    {
        if (!isLocal) transform.position = position;
        else transform.localPosition = position;
    }
    
    [PunRPC] protected void RPC_SetDirection(Vector3 dir) => transform.forward = dir;
    [PunRPC] protected void RPC_SetRotation(Vector3 euler, bool isLocal)
    {
        Quaternion rot = Quaternion.Euler(euler);
        if (!isLocal) transform.rotation = rot;
        else transform.localRotation = rot;
    }
    [PunRPC]
    protected void RPC_SetRotation(Quaternion rot, bool isLocal)
    {
        if (!isLocal) transform.rotation = rot;
        else transform.localRotation = rot;
    }
    [PunRPC] protected void RPC_SetScale(Vector3 scale) => transform.localScale = scale;
    [PunRPC] protected void RPC_SyncedDestroy()
    {
        if (IsMasterClient) 
            PhotonNetwork.Destroy(_pv);
    }
    [PunRPC] protected void RPC_SyncedDOMove(Vector3 pos, float duration) => transform.DOMove(pos, duration);

    protected virtual void Awake()
    {
        if (!TryGetComponent<PhotonView>(out _pv))
            _pv = gameObject.AddComponent<PhotonView>();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

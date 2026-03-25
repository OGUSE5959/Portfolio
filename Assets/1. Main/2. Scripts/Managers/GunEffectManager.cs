using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;

public class GunEffectManager : SingletonMonoBehaviour<GunEffectManager>
{
    PhotonView _pv;
    [SerializeField] GameObject _bulletObj;
    [SerializeField] GameObject _cartridgeCase;
    GameObjectPool<Transform> _bulletPool = new GameObjectPool<Transform>();
    GameObjectPool<Transform> _cartridgePool = new GameObjectPool<Transform>();

    public void CreateBullet(Vector3 start, Vector3 dest, float duration = 0.05f) 
        => StartCoroutine(Coroutine_CreateBullet(start, dest, duration));        
    public void SyncedCreateBullet(Vector3 start, Vector3 dest, float duration = 0.05f) // 이 클래스의 pv.IsMine은 항상 IsMasterClient와 같아서 잘못된 방식일듯..
    {
        CreateBullet(start, dest, duration);
        if (PhotonNetwork.IsMasterClient)   // 같은 pv플레이어가 두번 이상 호출하는게 이상적이지만 임시방편
            _pv.RPC("RPC_CreateBullet", RpcTarget.Others, start, dest, duration);
    }
    public void CreateCartridgeCase(Vector3 start, Vector3 dir)
        => StartCoroutine(Coroutine_CreateCartridgeCase(start, dir));
    public void SyncedCreateCartridgeCase(Vector3 start, Vector3 dir)
    {
        StartCoroutine(Coroutine_CreateCartridgeCase(start, dir));
        if (PhotonNetwork.IsMasterClient)
            _pv.RPC("RPC_CreateCartridgeCase", RpcTarget.Others, start, dir);
    }
    public void SyncedCreateBuckshot(Vector3 start, Vector3 dir)
    {

    }
    [PunRPC] void RPC_CreateBullet(Vector3 start, Vector3 dest, float duration) 
        => StartCoroutine(Coroutine_CreateBullet(start, dest, duration));
    [PunRPC] void RPC_CreateCartridgeCase(Vector3 start, Vector3 dir) 
        => StartCoroutine(Coroutine_CreateCartridgeCase(start, dir));

    IEnumerator Coroutine_CreateBullet(Vector3 start, Vector3 dest, float duration)
    {
        Transform bullet = _bulletPool.Get(); //Instantiate(_bulletObj, transform);
        bullet.position = start;
        bullet.forward = Utility.GetNormalizedDir(dest, start);
        bullet.gameObject.SetActive(true);
        bullet.DOMove(dest, duration);
        yield return Utility.GetWaitForSeconds(duration);
        bullet.gameObject.SetActive(false);
        _bulletPool.Set(bullet);
    }
    IEnumerator Coroutine_CreateCartridgeCase(Vector3 start, Vector3 dir)
    {
        Transform cartridge = _cartridgePool.Get(); //Instantiate(_cartridgeCase, transform);
        cartridge.position = start;
        cartridge.forward = dir;
        cartridge.gameObject.SetActive(true);
        var rigid = cartridge.GetComponentInChildren<Rigidbody>(true);
        
        if(rigid == null)
        {
            Debug.Log("rigid == null");
            yield break;
        }
        rigid.AddForce(dir * 5f, ForceMode.Impulse);
        yield return Utility.GetWaitForSeconds(3f);
        cartridge.gameObject.SetActive(false);
        _cartridgePool.Set(cartridge);
    }
    protected override void OnAwake()
    {
        base.OnAwake();
        _pv = GetComponent<PhotonView>();
        _bulletPool.CreatePool(10, () => {
            GameObject bullet = Instantiate(_bulletObj, transform);
            bullet.gameObject.SetActive(false);
            return bullet.transform;
        });
        _cartridgePool.CreatePool(10, () =>
        {
            GameObject cart = Instantiate(_cartridgeCase, transform);
            cart.gameObject.SetActive(false);
            return cart.transform;
        });
    }
}

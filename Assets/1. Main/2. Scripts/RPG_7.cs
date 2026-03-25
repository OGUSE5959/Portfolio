using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RPG_7 : Gun
{
    [Space]
    GameObjectPool<Rocket> _pool;
    [SerializeField] Transform _rocketTr;
    MeshRenderer _rocketTrMesh;
    [SerializeField] float _rocketSpeed;
    [Space]
    [SerializeField] AudioClip[] _boomSFXs;

    public GameObjectPool<Rocket> Pool => _pool;

    public override void Initialize(PlayerController master)
    {
        base.Initialize(master);
        _rocketTrMesh = _rocketTr.GetComponent<MeshRenderer>();
        _pool = new GameObjectPool<Rocket>(2, () =>
        {
            Rocket rocket = PhotonNetwork.Instantiate("PhotonPrefabs/Rocket", Vector3.zero, Utility.QI).GetComponent<Rocket>();
            rocket.Initialize(this);
            rocket.SyncedDisable();
            return rocket;
        });
    }
    public void SyncedPlayOnSpot(Vector3 spot)
    {
        byte index = (byte)Random.Range(0, _boomSFXs.Length);
        RPC_PlayOnSpot(index, spot);
        _pv.RPC("RPC_PlayOnSpot", RpcTarget.Others, index, spot);
    }
    protected override void SyncedScanFire()
    {
        base.SyncedScanFire();
    }
    protected override void HitSomething(RaycastHit hit)
    {
        // base.HitSomething(hit);
        // Debug.Log("HitSomething", hit.collider);
        Vector3 dir = Utility.GetNormalizedDir(hit.point, _muzzleFireTr.position);
        LaunchRocket(dir);
    }
    protected override void MissedShot()
    {
        Vector3 dir = Utility.GetNormalizedDir(_mainCam.transform.position 
            + _mainCam.transform.forward * 100f, _muzzleFireTr.position);
        LaunchRocket(dir);
    }
    protected override void CreateCartridgeCase()
    {
        return;
        // base.CreateCartridgeCase();
    }
    protected override void OnReload()
    {
        // base.OnReload();
        if (Magazine >= 1) return; 
        _rocketTr.gameObject.SetActive(true);
        _animCtrl.Play(GunAnimCtrl.Motion.ReloadNoAmmo);
    }
    public override void ResetAmmo()
    {
        base.ResetAmmo();
        Magazine = 1;
        _master.UI.SetMagazineTxt(Magazine);
        _master.UI.SetTotalAmmoTxt(_master.WeaponCtrl.AmmoCount(AmmoType));
    }
    void LaunchRocket(Vector3 dir)
    {
        Rocket rocket = _pool.Get();
        rocket.Launch(_rocketTr.position, dir);
        // _rocketTrMesh.enabled = false;
        _rocketTr.gameObject.SetActive(false);
    }

    [PunRPC] void RPC_PlayOnSpot(byte clipIndex, Vector3 spot)
    {
        AudioClip clip = _boomSFXs[clipIndex];
        AudioManager.Instance.PlayOnSpot(clip, spot);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Magazine = 1;
    }
}

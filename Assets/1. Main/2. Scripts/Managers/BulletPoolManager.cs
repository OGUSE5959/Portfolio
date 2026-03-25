using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/*public enum BulletType
{
    FMJ_M193,   // 5.56
    FMJ_M80,    // 7.62

    Max
}*/

public class BulletPoolManager : SingletonMonoBehaviour<BulletPoolManager>
{
    [SerializeField] Bullet[] _bulletPrefabs;
    [SerializeField] string[] _bulletRawPaths = new string[(int)AmmoType.Max];
    Dictionary<AmmoType, GameObjectPool<Bullet>> _bulletPool = new Dictionary<AmmoType, GameObjectPool<Bullet>>();

    public Bullet GetBullet(AmmoType type)
    {
        return _bulletPool[type].Get();
    }
    public void SetBullet(AmmoType type, Bullet bullet)
    {
        _bulletPool[type].Set(bullet);
    }
    public Bullet CreateBullet(AmmoType type, Vector3 position)
    {
        Bullet bullet = GetBullet(type);
        bullet.transform.position = position;

        return bullet;
    }
    public Bullet CreateBullet(AmmoType type, Vector3 position, Quaternion rotation)
    {
        Bullet bullet = CreateBullet(type, position);
        bullet.transform.rotation = rotation;

        return bullet;
    }
    public Bullet CreateBullet(AmmoType type, Transform transform, bool setParent = false)
    {
        Bullet bullet = CreateBullet(type, transform.position);
        bullet.transform.forward = transform.forward;
        if (setParent) 
            bullet.transform.SetParent(transform);
        return bullet;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
       if (!PhotonNetwork.IsMasterClient) return;

        // _bulletPrefabs = Resources.LoadAll<Bullet>("PhotonPrefabs/Bullets");
        for (int i = 0; i < _bulletRawPaths.Length; i++)
        {
            string path = Utility.GetResourcesPath(_bulletRawPaths[i]);
            GameObject prefab = Resources.Load<GameObject>(path); //PhotonNetwork.Instantiate(path, Vector3.zero, Utility.QI);
            Bullet bullet = null;
            if (!prefab.TryGetComponent<Bullet>(out bullet))
            {
                Debug.LogWarning("The object of Path : " + path + "doesn't have bulelt component", prefab);
                break;
            }
            if (_bulletPool.ContainsKey(bullet.BulletType))
            {
                Debug.LogWarning("There's Two or more Bullet Prefab that has same BulletType");
                break;
            }

            GameObjectPool<Bullet> pool = new GameObjectPool<Bullet>();
            _bulletPool.Add(bullet.BulletType, pool);
            pool.CreatePool(2, () =>
            {
                Bullet inst = PhotonNetwork.Instantiate(path, Vector3.zero, Utility.QI).GetComponent<Bullet>();
                inst.SyncedSetActive(false);
                inst.transform.SetParent(transform);
                return inst;
            });
        }
        /*for(int i = 0; i < _bulletPrefabs.Length; i++)
        {
            Bullet prefab = _bulletPrefabs[i];
            if(_bulletPool.ContainsKey(prefab.BulletType))
            {
                Debug.LogWarning("There's Two or more Bullet Prefab that has same BulletType");
                break;
            }

            GameObjectPool<Bullet> pool = new GameObjectPool<Bullet>();
            _bulletPool.Add(prefab.BulletType, pool);
            pool.CreatePool(2, () =>
            {
                Bullet inst = Instantiate(prefab, transform);
                inst.gameObject.SetActive(false);
                return inst;
            });
        }*/
    }
    protected override void OnStart()
    {
        base.OnStart();
    }
}

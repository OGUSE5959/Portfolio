using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 실패한 클래스.. 딱히 풀링 할 이유도 없다
public class ItemListUnitPool : SingletonMonoBehaviour<ItemListUnitPool>
{
    [SerializeField] ItemListUnit _unitPrefab;
    [SerializeField] string _rawPath;
    GameObjectPool<ItemListUnit> _pool = new GameObjectPool<ItemListUnit>();
    Dictionary<int, ItemListUnit> _unitList = new Dictionary<int, ItemListUnit>();
    public Dictionary<int, ItemListUnit> UnitList => _unitList;

    public ItemListUnit GetUnit()
    {
        return _pool.Get();
    }
    public void SetUnit(ItemListUnit unit)
    {
        _pool.Set(unit);
    }
    public void AddUnit(ItemListUnit unit)
    {
        if (!UnitList.ContainsKey(unit.PV.ViewID))
            UnitList.Add(unit.PV.ViewID, unit);
    }
    public ItemListUnit CreateUnit(FieldItem item, Transform parent = null)
    {
        string path = Utility.GetResourcesPath(_rawPath);
        ItemListUnit unit = /*PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity)
                    .GetComponent<ItemListUnit>();*/ GetUnit();

        unit.Initialize(item);
        // unit.SyncedSetParent(parent ? parent : transform);
        return unit;
    }
    public void ResetAll()
    {
        foreach (var pair in UnitList)
        {
            var item = pair.Value;
            if (item == null) continue;
            if (item.FieldBody != null) item.FieldBody.SyncedReset();
            item.SyncedDisable();
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (PhotonNetwork.IsMasterClient)
        {
            string path = Utility.GetResourcesPath(_rawPath);
            _pool.CreatePool(2, () =>
            {
                ItemListUnit unit = PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity)
                    .GetComponent<ItemListUnit>(); // Instantiate(_unitPrefab, transform);
                if (GameManager.Instance) GameManager.Instance.RegistPvInst(unit.PV);
                // unit.SyncedSetParent(transform);
                unit.SyncedSetActive(false);
                unit.transform.SetParent(transform);
                return unit;
            });
        }
        
    }
    protected override void OnStart()
    {
        base.OnStart();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Photon.Pun;

public class KillLog : SingletonMonoBehaviour<KillLog>
{
    PhotonView _pv;
    [SerializeField] KillLogUnit _unitPrefab;
    // [SerializeField] Transform _vertiGroup;
    GameObjectPool<KillLogUnit> _pool = new GameObjectPool<KillLogUnit>();
    List<KillLogUnit> _unitList = new List<KillLogUnit>();
    [Space]
    [SerializeField] Transform _leftTr;
    [SerializeField] Transform _rightTr;
    Coroutine _coroutine_Align;

    float UnitHeight => _unitPrefab.Height;

    public void CreateLog(string killer, string victim, Sprite icon = null)
    {
        // Debug.Log("CreateLog " + killer.NullIfEmpty() + ", " + victim.NullIfEmpty());
        KillLogUnit unit = _pool.Get();
        unit.SetUp(killer/* + (Random.Range(0, 10)).ToString()*/, victim, icon);
        AddOnList(unit);
    }
    public void CreateLog(IAttackable attacker, IDamagable hurter, Sprite icon = null)
    {
        KillLogUnit unit = _pool.Get();
        unit.SetUp(attacker, hurter, icon);
        AddOnList(unit);
    }
    public void SyncedCreateLog(IAttackable attacker, IDamagable hurter, Sprite icon = null)
    {
        // CreateLog(attacker, hurter, icon);
        _pv.RPC("RPC_CreateLog", RpcTarget.All, attacker.PV.ViewID, hurter.PV.ViewID);
    }
    public void SyncedCreateLog(int attacker, int hurter, Sprite icon = null)
    {
        _pv.RPC("RPC_CreateLog", RpcTarget.All,attacker, hurter);
    }
    void AddOnList(KillLogUnit unit)
    {
        _unitList.Add(unit);
        StartCoroutine(Coroutine_SetWidth(unit));
        if (_unitList.Count > 5)
            RemoveOnList();
        else Alignment();
    }
    void RemoveOnList()
    {
        KillLogUnit unit = _unitList[0];
        _unitList.Remove(unit);
        unit.gameObject.SetActive(false);
        _pool.Set(unit);
        Alignment();
    }
    void Alignment()
    {
        if (_coroutine_Align != null)
            StopCoroutine(_coroutine_Align);
        _coroutine_Align = StartCoroutine(Coroutine_Alignment());
    }
    float GetHeight(int index) => _leftTr.position.y - UnitHeight * index;

    [PunRPC] void RPC_CreateLog(int attacker, int hurter)
    {
        KillLogUnit unit = _pool.Get();
        unit.SetUp(attacker, hurter);
        AddOnList(unit);
    }
    IEnumerator Coroutine_SetWidth(KillLogUnit unit)
    {
        unit.transform.position = new Vector3(_rightTr.position.x, GetHeight(_unitList.Count), 0f);
        unit.gameObject.SetActive(true);
        yield return Utility.GetWaitForSeconds(0.2f);
        unit.transform.DOMove(new Vector3(_leftTr.position.x, unit.transform.position.y, 0f), 0.3f);
        yield break;
    }
    IEnumerator Coroutine_Alignment()
    {
        for(int i = 0; i < _unitList.Count; i++)
        {
            KillLogUnit unit = _unitList[i];
            unit.transform.DOMove(new Vector3(_leftTr.position.x, GetHeight(i), 0f), 0.2f);
            yield return Utility.GetWaitForSeconds(0.1f);
        }
        if (_unitList.Count > 0)
        {
            yield return Utility.GetWaitForSeconds(1.5f);
            RemoveOnList();
        }
    }
    IEnumerator Coroutine_RemoveAuto()
    {
        while(true)
        {
            if (_unitList.Count > 0)
            {
                yield return Utility.GetWaitForSeconds(0.5f);
                RemoveOnList();
            }
            yield return null;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _pv = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    protected override void OnStart()
    {
        base.OnStart();
        _pool = new GameObjectPool<KillLogUnit>(2, () =>
        {
            KillLogUnit unit = Instantiate(_unitPrefab, transform);
            unit.Initialize();
            unit.gameObject.SetActive(false);
            return unit;
        });
        // StartCoroutine(Coroutine_RemoveAuto());
    }
}

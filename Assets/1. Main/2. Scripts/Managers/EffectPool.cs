using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EffectPool : SingletonMonoBehaviour<EffectPool>
{
    [SerializeField] GameObject[] _effectPrefabs;
    Dictionary<string, GameObjectPool<EffectPoolUnit>> _effectPool = new Dictionary<string, GameObjectPool<EffectPoolUnit>>();
    PhotonView _pv;

    public EffectPoolUnit GetEffect(string effectName)
    {
        if(string.IsNullOrEmpty(effectName) || !_effectPool.ContainsKey(effectName)) return null;
        var pool = _effectPool[effectName];
        for(int i = 0; i < pool.Count; i++)
        {
            var unit = pool.Get();
            if(!unit.IsReady)
            {
                pool.Set(unit);
                continue;
            }
            return unit;
        }

        return _effectPool[effectName].New();
    }
    public void InsertEffect(string effectName, EffectPoolUnit effect)
    {
        if (_effectPool.ContainsKey(effectName))
            _effectPool[effectName].Set(effect);
    }
    [PunRPC] public EffectPoolUnit CreateEffect(string effectName, Vector3 position)
    {
        var effect = GetEffect(effectName);
        if (!effect) return null;
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        return effect;
    }
    public EffectPoolUnit CreateEffect(string effectName, Vector3 position, Quaternion lookAt)
    {
        var effect = GetEffect(effectName);
        effect.transform.position = position;
        effect.transform.rotation = lookAt;
        effect.gameObject.SetActive(true);
        return effect;
    }
    public void SyncedCreateEffect(string effectName, Vector3 position, RpcTarget rpcTarget = RpcTarget.All)
        => _pv.RPC("CreateEffect", rpcTarget, effectName, position);
    public void SyncedCE_MeFirst(string effectName, Vector3 position, RpcTarget rpcTarget = RpcTarget.Others)
    {
        CreateEffect(effectName, position);
        _pv.RPC("CreateEffect", rpcTarget, effectName, position);
    }
    // [PunRPC] void RPC_CreateEffectCreateEffect(string effectName, Vector3 position) => CreateEffect(effectName, position);

    protected override void OnAwake()
    {
        base.OnAwake();
        _pv = GetComponent<PhotonView>();
        _effectPrefabs = Resources.LoadAll<GameObject>("Effects");
        for(int i = 0; i < _effectPrefabs.Length; i++)
        {
            var prefab = _effectPrefabs[i];
            if(prefab == null) { continue; }
            EffectPoolUnit unit = null;
            FXAutoFalse autoFalse = null;
            GameObjectPool<EffectPoolUnit> pool = new GameObjectPool<EffectPoolUnit>(2, () =>
            {
                var effect = Instantiate(prefab);
                if (!effect.TryGetComponent<EffectPoolUnit>(out unit))
                    unit = effect.AddComponent<EffectPoolUnit>();
                if (!effect.TryGetComponent<FXAutoFalse>(out autoFalse))
                    autoFalse = effect.AddComponent<FXAutoFalse>();

                if (!string.IsNullOrEmpty(unit.EffectName))
                    unit.SetEffectPool(unit.EffectName);
                else
                    unit.SetEffectPool(prefab.name);

                effect.gameObject.SetActive(false);
                return unit;
            });
            if(unit)
            {
                if(!string.IsNullOrEmpty(unit.EffectName))
                    _effectPool.Add(unit.EffectName, pool);
                else
                    _effectPool.Add(prefab.name, pool);
            }
        }
    }
    protected override void OnStart()
    {
        base.OnStart();
    }
}

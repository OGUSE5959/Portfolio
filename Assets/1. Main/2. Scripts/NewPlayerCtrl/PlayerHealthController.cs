using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealthController : MonoBehaviour, IDamagable
{
    PlayerController _me;
    PlayerWeaponController _weaponCtrl;
    PhotonView _pv;
    GameManager _gm;

    [SerializeField] float _hpMax;
    [SerializeField] float _hp;
    bool _isDie;

    public float HP => _hp;
    PlayerUI UI => _me.UI;
    public bool IsDie => _isDie;
    public PhotonView PV => _me.PV;
    public bool CanHit => !IsDie && !_gm.IsInvinc && !_gm.IsGameOver;

    public float Health => _hp;
    void Initialize()
    {
        _me = GetComponent<PlayerController>();
        _weaponCtrl = _me.WeaponCtrl;
        _pv = _me.PV;
        _gm = GameManager.Instance;

        _hp = _hpMax;

        var colliders = GetComponentsInChildren<Collider>();
        foreach(Collider col in colliders)
        {
            if (col.TryGetComponent<CharacterController>(out CharacterController ct)
                || col.TryGetComponent<InteractDetector>(out InteractDetector id)) continue;
            col.isTrigger = true;

            HitParts hit = col.gameObject.AddComponent<HitParts>();
            /*Rigidbody rigid = col.gameObject.AddComponent<Rigidbody>();
            rigid.useGravity = false;
            rigid.isKinematic = true;*/
            if (_me.IsMe)
                hit.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
            else hit.gameObject.layer = LayerMask.NameToLayer("OtherPlayer");
            hit.Initialize(this);
        }
    }
    public void ResetHealth()
    {
        RPC_ResetHealth();
        _pv.RPC("RPC_ResetHealth", RpcTarget.Others);
    }
    public void SetHit(IAttackable attacker, float rawDamage, string hitPart)
    {
        // _pv.RPC("RPC_SetHit", RpcTarget.All, attacker.Name, rawDamage, hitPart);
        if (IsDie || _gm.IsInvinc) return;
        //Debug.Log(hitPart);
        float damage = rawDamage;
        if (hitPart.Equals("Head"))
        {
            FieldArmor helmet = _weaponCtrl.CurrHelmet;
            // damage *= 2.5f;  // HitParts에서 먼저 처리!
            if (helmet)
            {
                Debug.Log("Head Shot");
                float rate = helmet.Defense / 100f;
                damage = damage - damage * rate;
                Debug.Log(damage);
                // if (_me.IsMe)
                    _weaponCtrl.SetArmorDamaged(ArmorType.Helmet, damage);
            }
        }
        else if (hitPart.Equals("Body"))
        {
            FieldArmor vest = _weaponCtrl.CurrVest;
            if (vest)
            {
                damage -= vest.Defense;
                float rate = vest.Defense / 100f;
                damage *= rate;
                // if (_me.IsMe)
                    _weaponCtrl.SetArmorDamaged(ArmorType.Vest, damage);
            }
        }
        // else damage *= 0.8f;
        // Debug.Log(damage + " " + hitPart);
        SetHit(attacker, damage);//SetHit(attacker, damage);
    }
    public void SetHit(IAttackable attacker, float damage)
    {
        UI.SyncedRegistHitIndi(_pv.ViewID, attacker.transform.position);
        SetHit(attacker.PV.ViewID, damage);
    }
    void IDamagable.SetHit(IAttackable attacker, float damage, Vector3 hitSpot) { }

    public void SetHit(int attacker, float damage)
    {
        // _pv.RPC("RPC_SetHit", RpcTarget.All, attacker, rawDamage);
        if (IsDie || _gm.IsInvinc) return;
        SyncedSetHP(_hp -= damage, _hpMax);

        if (_me.IsMe)
        {
            if (_me.IsSingle) return;
            // _me.SyncedDisable();

            UI.SetHp(_hp, _hpMax);
        }
        if (_hp <= 0) SetDie(attacker);
    }
    public void SyncedSetHP(float hp, float hpMax)
    {
        RPC_SetSetHP(hp, hpMax);
        _pv.RPC("RPC_SetSetHP", RpcTarget.Others, hp, hpMax);
    }

    void SetDie(int attacker = -1)
    {
        if (_me.IsSingle || IsDie) return;
        _isDie = true;
        _gm.OnPlayerDead(_me);  // PV가Mine이 아니여도 GM한테 보고를 해야한다!!
        _me.SyncedDisable();
        if (_me.IsMe)
        {
            RPC_SetDieForMine(attacker);
            _pv.RPC("RPC_SetDieForClone", RpcTarget.Others/*, attacker*/);
        }
        else
        {
            RPC_SetDieForClone();
            _pv.RPC("RPC_SetDie", RpcTarget.All, attacker);
        }
    }
    /*void SetDie(int attacker = -1)
    {
        // _isDie = true; // RPC가 한프레임보다 늦을 수도 있어서
        //  여기서 하면 두번 이상 죽는걸 방지 못함 고민이네..   => SyncedDisable수정으로 해결
        if (IsDie) return;
        _isDie = true;  // _hp = 0f;
        // KillLog.Instance.CreateLog(attacker, null);

        _gm.OnPlayerDead(_me);
        _me.SyncedDisable();
        CameraManager.Instance.SetTarget(_gm.DeathView);

        _pv.RPC("RPC_SetDie", RpcTarget.Others, attacker);
        if (attacker > 0) KillLog.Instance.SyncedCreateLog(attacker, PV.ViewID);
    }*/
    [PunRPC] void RPC_ResetHealth()
    {
        _isDie = false;
        _hp = _hpMax;
        if (_me.IsMe)
            UI.SetHp(100f, 100f);
    }
    [PunRPC] void RPC_SetDieForClone(/*int attacker*/)
    {
        if (IsDie) return;
        _isDie = true;  // _hp = 0f;      
        _gm.OnPlayerDead(_me);  // PV가Mine이 아니여도 GM한테 보고를 해야한다!!
    }
    [PunRPC]
    void RPC_SetDieForMine(int attacker)
    {
        RPC_SetDieForClone();
        CameraManager.Instance.SetTarget(_gm.DeathView);
        KillLog.Instance.SyncedCreateLog(attacker, PV.ViewID);
        // UI.SetHp(0f, _hpMax);
    }
    [PunRPC] void RPC_SetDie(int attacker)
    {
        if(_me.IsMe)
        {
            RPC_SetDieForMine(attacker);
            return;
        } RPC_SetDieForClone();
    }
    [PunRPC] void RPC_SetSetHP(float hp, float hpMax)
    {
        _hp = hp;
        _hpMax = hpMax;
        if(_me.IsMe) 
            UI.SetHp(hp, hpMax);
    }
    [PunRPC] void RPC_KillLog(string  attacker, string myNickName) => KillLog.Instance.CreateLog(attacker, myNickName);
    #region No User Longer..
    /*[PunRPC] void RPC_SetHit(string attacker, float damage)
    {
        if (!_me.IsMe || IsDie || _gm.IsInvinc) return;
        _hp -= damage;
        if (_hp <= 0)
        {
            RPC_SetDie(attacker); //SetDie();
        }
        if (_me.IsMe)
            UI.SetHp(_hp, _hpMax);
    }
    [PunRPC] void RPC_SetHit(string attacker, float rawDamage, string hitPart)
    {
        if (!_me.IsMe || IsDie || _gm.IsInvinc) return;
        //Debug.Log(hitPart);
        float damage = rawDamage;
        if (hitPart.Equals("Head"))
        {
            FieldArmor helmet = _weaponCtrl.CurrHelmet;
            damage *= 2.5f;
            if (helmet)
            {
                Debug.Log("Head Shot");
                float rate = helmet.Defense / 100f;
                damage *= rate;
                if (_me.IsMe)
                    _weaponCtrl.SetArmorDamaged(ArmorType.Helmet, damage);
            }
        }
        else if (hitPart.Equals("Body"))
        {
            FieldArmor vest = _weaponCtrl.CurrVest;
            if (vest)
            {
                damage -= vest.Defense;
                float rate = vest.Defense / 100f;
                damage *= rate;
                if (_me.IsMe)
                    _weaponCtrl.SetArmorDamaged(ArmorType.Vest, damage);
            }
        }
        else
        {
            damage *= 0.8f;
        }
        Debug.Log(damage + " " + hitPart);
        SetHit(attacker, damage);//SetHit(attacker, damage);
    }*/
    #endregion
    void Respawn()=> _me.Respawn();

    private void Awake()
    {
        Initialize();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }    

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}

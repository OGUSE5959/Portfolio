using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractController : MonoBehaviourPunCallbacks, IInteractor
{
    PlayerController _me;
    PlayerInputController _input;
    PlayerCameraController _camCtrl;
    public PhotonView PV => _me.PV;

    PlayerUI UI => _me.UI;
    bool CanUI => UI && _me.IsMe;
    // Transform CameraTarget => _camCtrl.CameraTarget;
    Camera _mainCam;
    [SerializeField] InteractDetector _interactDetector;
    [SerializeField] bool _detectContinuous;
    [SerializeField] Vector3 _detectCenter = new Vector3(0f, -0.5f, 0f);
    [SerializeField] float _detectRadius = 1.4f;
    [Space]
    List<IInteractable> _nearInteractList = new List<IInteractable>();
    public List<IInteractable> NearInteractList
    {
        get { _nearInteractList.RemoveAll(t => !t.gameObject.activeSelf); UpdateClosestOne(); return _nearInteractList; }
        set { _nearInteractList = value; }
    }

    bool IInteractor.DetectContinuous => _detectContinuous;

    IInteractable _closestInteract;
    IInteractable _targetInteract;

    StringBuilder _sb = new StringBuilder();

    public void OnDetect(IInteractable it)
    {
        if (!_me.IsMe) return;
        AddInteractList(it);
        // _master.NearInteractList.Add(it);
        // _master.UpdateNearItems();
    }
    public void OnRelease(IInteractable it)
    {
        if (!_me.IsMe) return;
        RemoveInteractList(it);
        // _master.NearInteractList.Remove(it);
        // _master.UpdateNearItems();
    }

    IInteractable GetClosestInteract(Vector3 standard, List<IInteractable> list, float maxDistance = 1000f)
    {
        if (!_me.IsMe || list.Count == 0) return null;
        if (list.Count == 1) return list[0];

        float closestDist = maxDistance;
        IInteractable closestOne = null;
        foreach (IInteractable t in list)
        {
            float dist = Vector3.Distance(standard, t.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestOne = t;
            }
        }
        return closestOne;
    }
    void Initialize()
    {
        _me = GetComponent<PlayerController>();

        _input = _me.Inputter;
        _camCtrl = _me.CamCtrl;
        _mainCam = Camera.main;

        _interactDetector = GetComponentInChildren<InteractDetector>();
        _interactDetector.SetInteractRange(_detectCenter, _detectRadius);

        if (!_me.IsMe)
        {
            _interactDetector.gameObject.SetActive(false);
            return;
        }
        _input.Actions.Interact.started += input => { if (_me.IsMe) OnInteract(); };
    }
    public void AddInteractList(IInteractable it)
    {
        if (!_me.IsMe) return;
        if (!NearInteractList.Contains(it))
        {
            NearInteractList.Add(it);
            // inven            
            if (UI && it.gameObject.TryGetComponent<FieldItem>(out FieldItem fieldItem))
                UI.AddFieldItem(fieldItem);
        }
    }
    public void RemoveInteractList(IInteractable it)
    {
        if (!_me.IsMe) return;
        if (NearInteractList.Contains(it))
        {
            NearInteractList.Remove(it);
            // inven
            if (it.gameObject.TryGetComponent<FieldItem>(out FieldItem fieldItem))
                UI.HideFieldItem(fieldItem);
        }
    }
    public void UpdateClosestOne()
    {
        var closest = GetClosestInteract(transform.position, _nearInteractList);
        _closestInteract = closest;
    }
    public RaycastHit? GetClosestOne(Vector3 standard, List<RaycastHit> list, float maxDistance = 1000f)
    {
        if (list.Count == 0) return null;
        if (list.Count == 1) return list[0];

        float closestDist = maxDistance;
        RaycastHit? closestOne = null;
        foreach (RaycastHit? t in list)
        {
            float dist = (Utility.ScreenCenter - _mainCam.WorldToScreenPoint(t.Value.point)).magnitude; //Vector3.Distance(standard, t.Value.point);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestOne = t;
            }
        }
        return closestOne;
    }
    void UpdateTargetInteract()
    {
        if(!UI) return;
        Transform camTr = _mainCam.transform; //_camCtrl.CameraTarget;
        Debug.DrawRay(camTr.position, camTr.forward * _detectRadius, Color.red);
        Ray ray = new Ray(camTr.position, camTr.forward);
        RaycastHit[] hits;

        if ((hits = Physics.SphereCastAll(ray, 0.1f, _detectRadius * 1.2f 
            + (CameraManager.Instance.IsFPS ? 1f : 2f), 1 << LayerMask.NameToLayer("Interactable"))).Length > 0
            /*Physics.Raycast(ray, out RaycastHit hitInfo, _detectRadius * 1.2f, 1 << LayerMask.NameToLayer("Interactable"))*/)
        {
            RaycastHit hit;
            RaycastHit? clo = GetClosestOne(_mainCam.transform.position, hits.ToList<RaycastHit>());
            if (clo != null) hit = clo.Value;
            else return;

            if ((_camCtrl.CameraTarget.position.z > hit.point.z && _camCtrl.CameraTarget.forward.z > 0)
                || (_camCtrl.CameraTarget.position.z < hit.point.z && _camCtrl.CameraTarget.forward.z < 0)) return;
            if (hit.collider.gameObject.TryGetComponent<IInteractable>(out IInteractable it))
            {
                _targetInteract = it;
                //if (it.InteractType == InteractType.Item) { }
                _sb.Clear();
                _sb.Append(it.Name);
                string verb = "";
                verb += it.Purpose;

                _sb.Append(verb);
                UI.Interact.TurnOn("F", _sb.ToString());
                UI.Interact.transform.position = _mainCam.WorldToScreenPoint(it.transform.position);
            }
        }

        else
        {
            if (_targetInteract != null && _targetInteract.InteractType == InteractType.Item)
            {

            }

            _targetInteract = null;
            UI.Interact.TurnOff();
        }
    }
    // void Interactor.Interact() => Interact();
    void OnInteract()
    {
        if (_targetInteract != null)
        {
            Debug.Log("Interact");
            // Debug.Log("OnInteract");
            Interact();
        }
    }
    void Interact()
    {
        _targetInteract.OnInteracted(this);
        if (!_targetInteract.gameObject.activeSelf)
        {
            _targetInteract = null;
            UI.Interact.TurnOff();
        }
        // UpdateTargetInteract();
    }
    public void ThrowItem(FieldItem item, Vector3 dir, float force)
    {
        item.SyncedSetPosition(_me.CamCtrl.CameraTarget.position + transform.forward * 0.5f);
        item.OnThrown(dir, force);
    }
    public void ThrowItem(FieldItem item)
    {
        ThrowItem(item, (_mainCam.transform.forward + Vector3.up * 0.1f).normalized, 5f);
    }
    public bool TryAddItem()
    {
        return false;
    }

    void OnUpdate_Network()
    {
        if (!_me.IsMe) return;
        UpdateTargetInteract();
        // _nearInteractList.RemoveAll(t => !t.gameObject.activeSelf);
    }
    void OnUpdate()
    {
        // UpdateTargetInteract();
    }

    private void Awake()
    {
        Initialize();
    }
    private void Update()
    {
        OnUpdate_Network();
        OnUpdate();
    }
}

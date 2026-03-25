using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public interface IWatchable
{
    public GameObject gameObject { get; }
    public Transform transform { get; }

    public Vector3 WatchOffset { get; }
    public Vector3 WatchPoint { get; }
    public void OnWatched();
    public void OnQuited();
}

public class TitleCamera : MonoBehaviour
{
    Camera _mainCam;
    Vector3 _startPos;
    Quaternion _startRot;
    IWatchable _watchObj;

    [SerializeField] bool _isWork = true;
    [SerializeField] float _distMin;
    [SerializeField] float _distMax;
    float _fixedDist;

    Vector3 _watchRot;
    bool _isWatchAround;

    bool HaveWatchObj => _watchObj != null;

    void OnClick()
    {
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 999f, Color.red, 2f);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f))
            if (hit.collider.TryGetComponent<IWatchable>(out IWatchable watchable))
            {
                _watchObj = watchable;
                watchable.OnWatched();
            }
    }
    void ResetWatchable()
    {
        _watchObj.OnQuited();
        _watchObj = null;
        // transform.DOMove(_startPos, 1f);
    }

    // Start is called before the first frame update
    void Awake()
    {
        _mainCam = Camera.main;
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isWork) return;
        float deltaTime = Time.deltaTime;
        if (MenuManager.Instance.CurrentMenu != MenuType.Title) return;
        if (!HaveWatchObj)
        {
            if (Input.GetMouseButtonDown(0))
                OnClick();
            transform.position = Vector3.Lerp(transform.position, _startPos, 10f * deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _startRot, 50f * deltaTime);
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                ResetWatchable();
                return;
            }
            // ====== 위치
            float distFromStart = Vector3.Distance(_watchObj.WatchPoint, _startPos);
            _distMin = distFromStart / 4f; _distMax = distFromStart * 3f / 4f;
            _fixedDist -= Input.GetAxis("Mouse ScrollWheel");
            _fixedDist = Mathf.Clamp(_fixedDist, _distMin, _distMax);

            Vector3 dir = Utility.GetNormalizedDir(_watchObj.WatchPoint, transform.position);
            float currDist = Vector3.Distance(transform.position, _watchObj.WatchPoint);

            // if (!_isWatchAround)
            float distOffset = 0.1f;
            if (currDist > _fixedDist + distOffset)
                transform.position += dir * deltaTime;
            else if (currDist < _fixedDist - distOffset)
                transform.position -= dir * deltaTime;
            // ====== 회전
            if (Input.GetMouseButton(0))
            {
                if (!_isWatchAround) _isWatchAround = true;
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                Vector3 mouseInput = new Vector3(0, mouseX, mouseY);
                // transform.RotateAround(_watchObj.WatchPoint, mouseInput, 300f * deltaTime);
                transform.RotateAround(_watchObj.WatchPoint, Vector3.up * mouseX, 300f * deltaTime);
                transform.RotateAround(_watchObj.WatchPoint, Vector3.forward * mouseY, 300f * deltaTime);
            }
            else // if (Input.GetMouseButtonUp(0))
            {
                if (_isWatchAround) _isWatchAround = false;
                transform.rotation = Quaternion.Lerp(transform.rotation, _startRot, 50f * deltaTime);
                var originQ = VectorRotationQ(_startPos, _watchObj.WatchPoint);
                transform.RotateAround(_watchObj.WatchPoint, originQ.eulerAngles, 500f * deltaTime);
                Vector3 fixedPoint = _startPos + Utility.GetNormalizedDir(_watchObj.WatchPoint, _startPos) / _fixedDist;
                transform.position = Vector3.Lerp(transform.position, fixedPoint, 10f * deltaTime);
            }

        }
    }
    public Quaternion VectorRotationQ(Vector3 from, Vector3 target)
    {
        //	Vector3 target = this.m_CurrentTarget.position - this.m_Transform.position;
        target.Normalize();
        from.Normalize();
        float dot = Vector3.Dot(from, target);
        //	Debug.Log(dot);
        float angle = Mathf.Acos(dot) * 0.5f;

        Quaternion rotation = new Quaternion(0.0f, Mathf.Sin(angle), 0.0f, Mathf.Cos(angle));
        rotation = rotation * Quaternion.LookRotation(from);
        return rotation;
        //this.m_Transform.forward = target;
    }
}

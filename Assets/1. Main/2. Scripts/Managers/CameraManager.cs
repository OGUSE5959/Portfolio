using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
    PhotonView _pv;
    Camera _mainCam;
    CinemachineBrain _brain;
    [SerializeField] LayerMask _fpsCulling;
    [SerializeField] LayerMask _tpsLayer;
    bool _isFPS = true;
    [Space]
    [SerializeField] CinemachineVirtualCamera _fpsVC;
    [SerializeField] CinemachineVirtualCamera _tpsVC;
    [SerializeField] Transform _tpsTarget;
    CinemachineBasicMultiChannelPerlin _fpsPerlin;
    CinemachineBasicMultiChannelPerlin _tpsPerlin;
    Coroutine _coroutine_Shake;

    public List<Action> _onFPSCallbacks = new List<Action>();
    public List<Action> _onTPSCallbacks = new List<Action>();

    public bool IsFPS => _isFPS;
    public CinemachineVirtualCamera VC_FPS => _fpsVC;
    public CinemachineVirtualCamera VC_TPS => _tpsVC;
    public Transform TPSTarget => _tpsTarget;
    float CurrShakeIntensity => _fpsPerlin.m_AmplitudeGain;
    public float CurrentFOV => IsFPS ? VC_FPS.m_Lens.FieldOfView : VC_TPS.m_Lens.FieldOfView;

    public void SetTarget(Transform target)
    {
        _fpsVC.Follow = target;
        _fpsVC.LookAt = target;

        _tpsVC.Follow = target;
        _tpsVC.LookAt = target;
    }
    public void SetTPSTarget(Transform target)
    {
        _tpsVC.Follow = target;
        _tpsVC.LookAt = target;
    }

    public void SetView(bool isFPS)
    {
        _isFPS = isFPS;
        if (isFPS)
        {
            foreach (Action action in _onFPSCallbacks)
                action();
            _fpsVC.gameObject.SetActive(true);
            _tpsVC.gameObject.SetActive(false);
            _mainCam.cullingMask = ~_tpsLayer;
        }
        else {
            foreach (Action action in _onTPSCallbacks)
                action();
            _tpsVC.gameObject.SetActive(true);
            _fpsVC.gameObject.SetActive(false);
            _mainCam.cullingMask = _fpsCulling;
        }
    }
    public void SetTPSDir(Vector3 dir) => _tpsTarget.forward = dir;
    public void SetTPSPos(Vector3 pos) => _tpsTarget.position = pos;
    public void SetTPSTarget(Vector3 position, Vector3 dir)
    {
        SetTPSDir(dir);
        SetTPSPos(position);
    }
    public void ShakeCamera(float intensity, float duration)
    {
        // Debug.Log("ShakeCam " + intensity + ", " + duration);
        StartCoroutine(Coroutine_ProcessShake(intensity, duration));
        /*if (_coroutine_Shake == null)
        {
            _coroutine_Shake = StartCoroutine(Coroutine_ProcessShake(intensity, duration));
            return;
        }
        if(CurrShakeIntensity < intensity)
        {
            StopCoroutine(_coroutine_Shake);
            _coroutine_Shake = StartCoroutine(Coroutine_ProcessShake(intensity, duration));
        }
        else
        {

        }*/
    }
    public void ShakeCameraToAClient(string userID, float intensity, float duration)
    {
        if(PhotonNetwork.LocalPlayer.UserId.Equals(userID)) 
            ShakeCamera(intensity, duration);
        else _pv.RPC("RPC_ShakeCamera", RpcTarget.All, userID, intensity, duration);
    }
    public void SyncedNoticShake(Vector3 point, float radius, float intensity, float duration)
    {
        RPC_NoticShakeCamera(point, radius, intensity, duration);
        _pv.RPC("RPC_NoticShakeCamera", RpcTarget.Others, point, radius, intensity, duration);
    }
    [PunRPC] void RPC_NoticShakeCamera(Vector3 point, float radius, float intensity, float duration)
    {
        float dist = Vector3.Distance(_mainCam.transform.position, point);
        if (dist <= radius)
            ShakeCamera(intensity * radius / dist, duration);
    }
    public void Noise(float amplitudeGain, float frequencyGain)
    {
        _fpsPerlin.m_AmplitudeGain = amplitudeGain;
        _tpsPerlin.m_AmplitudeGain = amplitudeGain;

        _fpsPerlin.m_FrequencyGain = frequencyGain;
        _tpsPerlin.m_FrequencyGain = frequencyGain;
    }
    [PunRPC] void RPC_ShakeCamera(string userID, float intensity, float duration)
    {
        if (PhotonNetwork.LocalPlayer.UserId.Equals(userID))
            ShakeCamera(intensity, duration);
    }

    private IEnumerator Coroutine_ProcessShake(float shakeIntensity = 5f, float shakeTiming = 0.5f)
    {
        /*if(_coroutine_Shake != null)
        {
            while(true)
            {
                shakeTiming -= Time.deltaTime;
                yield return _coroutine_Shake;
            }
            if (shakeTiming <= 0) yield break;
        }*/

        Noise(shakeIntensity, 1f);
        yield return new WaitForSeconds(shakeTiming);
        Noise(0, 0);
    }
    protected override void OnAwake()
    {
        base.OnAwake();
        _pv = GetComponent<PhotonView>();
        _mainCam = Camera.main;
        _brain = _mainCam.GetComponent<CinemachineBrain>();

        _fpsPerlin = _fpsVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _tpsPerlin = _tpsVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (!_tpsVC.Follow || !_tpsVC.LookAt)
            SetTPSTarget(_tpsTarget);
    }
    // Start is called before the first frame update
    protected override  void OnStart()
    {
        SetView(true);
    }
}

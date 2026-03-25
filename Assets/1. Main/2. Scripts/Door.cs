using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Door : SyncedMonoBehaviour, IInteractable, IPunObservable
{
    [SerializeField] string _name = "¹®";
    Quaternion _syncedRotation;
    // [SerializeField] Transform _hinges;
    string IInteractable.Name => _name;
    string IInteractable.Purpose => IsClosed() ? " ¿­±â" : " ´Ý±â";

    InteractType IInteractable.InteractType => InteractType.Door;
    float _startYaw;
    float CurrentYaw => Utility.NormalizeAngle(transform.eulerAngles.y);

    Material[] IInteractable.Materials => throw new System.NotImplementedException();

    AudioSource _audioSFX;
    [Space]
    [SerializeField] AudioClip _openSound;
    [SerializeField] AudioClip _closeSound;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            transform.position = (Vector3)stream.ReceiveNext();
            _syncedRotation = (Quaternion)stream.ReceiveNext();
        }
    }
    void IInteractable.OnInteracted(IInteractor interactor)
    {
        // float zOffset = transform.position.z - interactor.transform.position.z;
        bool sameDir = transform.forward.z * interactor.transform.forward.z > 0f;

        _pv.RPC("RPC_Work", RpcTarget.All, sameDir);
    }
    [PunRPC] void RPC_Work(bool sameDir)
    {
        float goalHingeYaw;
        if (sameDir)
            goalHingeYaw = -89f;
        else goalHingeYaw = 89f;
        StartCoroutine(Coroutine_Work(goalHingeYaw));
    }
    public bool IsClosed()
    {
        float currYaw = Utility.NormalizeAngle(transform.eulerAngles.y);
        // currYaw = Mathf.CeilToInt(currYaw * 1000) / 1000f;
        // Debug.Log (currYaw + ", " + (currYaw == _startYaw));
        return currYaw == _startYaw;
    }
    [PunRPC] void RPCPlayOneshot() => _audioSFX.PlayOneShot(_openSound);

    IEnumerator Coroutine_Work(float goalHingeYaw, float duration = 0.3f)
    {
        float goal = _startYaw + goalHingeYaw;
        if (!IsClosed())
        {
            transform.DORotateQuaternion(Quaternion.Euler(0f, _startYaw, 0f), duration / 2f);
            yield return Utility.GetWaitForSeconds(duration / 2f);
            _audioSFX.PlayOneShot(_closeSound);
        }
        else
        {
            transform.DORotate(new Vector3(0f, _startYaw + goalHingeYaw, 0f), duration);
            // _pv.RPC("RPCPlayOneshot", RpcTarget.All);
            _audioSFX.PlayOneShot(_openSound);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        InteractableManager im = InteractableManager.Instance;
        if (im) im.SetInteractable(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        _startYaw = CurrentYaw;
        if (!TryGetComponent<AudioSource>(out _audioSFX))
            _audioSFX = gameObject.AddComponent<AudioSource>();
        _audioSFX.spatialBlend = 1f;
        _audioSFX.outputAudioMixerGroup = AudioManager.Instance.Group_SFX;
    }
}

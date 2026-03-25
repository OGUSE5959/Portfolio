using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum InteractType
{
    Item,
    Door,
    Vehicle,
    Etc,

    Max
}

public interface IInteractable
{
    public PhotonView PV { get; }
    public void OnInteracted(IInteractor interactor);
    public GameObject gameObject { get; }
    public Transform transform { get; }
    public IInteractable This => this;
    public int HashCode => GetHashCode();
    public string Name { get; }
    public string Purpose { get; }
    public InteractType InteractType { get; }
    public Material[] Materials { get; }
}
public interface IInteractor
{
    // void Interact();
    public PhotonView PV { get; }
    public GameObject gameObject { get; }
    public Transform transform { get; }

    public bool DetectContinuous { get; }
    public void OnDetect(IInteractable it);
    public void OnRelease(IInteractable it);
}


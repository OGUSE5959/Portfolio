using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;

public class GenInteractable : MonoBehaviour
{
    [Tooltip("Item : Green, Door : Yellow")]
    [SerializeField] Color _gizmosColor = Color.black;
    [SerializeField] string _rawPath;

    void Generate()
    {
        var item = PhotonNetwork.Instantiate(_rawPath, transform.position, Quaternion.Euler(transform.forward));
        // var it = item.GetComponent<IInteractable>();
        if (transform.parent != null)
            item.transform.SetParent(transform.parent, false);
        if (item.TryGetComponent<FieldWeapon>(out FieldWeapon weapon))
            item.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
    // Start is called before the first frame update
    private void Awake()
    {
        _rawPath = Utility.GetResourcesPath(_rawPath);

        // Debug.Log(_path);
        if (PhotonNetwork.IsMasterClient)
        {
            Generate();
            // InteractableManager.Instance.AddRegetCallbacks(Generate);
            // item.transform.SetParent(transform);
        }
        // Destroy(gameObject);
    }
}

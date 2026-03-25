using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    [SerializeField] Transform _cameraTarget;
    [Space]
    [SerializeField] float _rectMoveSpeed;
    [SerializeField] float _verticalMoveSpeed;
    Vector2 _pitchAndYaw;
    [Space]
    [SerializeField] GameObject _settingsWnd;
    bool _setting = false;

    void Setting(bool setting)
    {
        _setting = setting;
        if (_setting)
        {
            Cursor.lockState = CursorLockMode.None;
            _settingsWnd.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            _settingsWnd.SetActive(false);
        }
    }

    private void Awake()
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetTarget(_cameraTarget);
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        if (CrossHair.Instance != null)
            CrossHair.Instance.SetAlpha(0f);
    }
    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (!_setting)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            _pitchAndYaw.x += -mouseY * 180f * deltaTime;
            _pitchAndYaw.y += mouseX * 180f * deltaTime;

            _cameraTarget.rotation = Quaternion.Euler(_pitchAndYaw.x, _pitchAndYaw.y, 0f);
            transform.rotation = Quaternion.Euler(0, _pitchAndYaw.y, 0f);
        }

        float right = Input.GetAxisRaw("Horizontal");
        float foword = Input.GetAxisRaw("Vertical");
        Vector3 rectDir = transform.right * right + transform.forward * foword;
        float up = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        float down = Input.GetKey(KeyCode.LeftControl) ? -1f : 0f;
        Vector3 vertDir = transform.up * up + transform.up * down;

        transform.position += (rectDir.normalized * _rectMoveSpeed + vertDir * _verticalMoveSpeed) 
            * (Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f) * deltaTime;

        if (Input.GetMouseButtonDown(1))
            Setting(!_setting);
    }
}

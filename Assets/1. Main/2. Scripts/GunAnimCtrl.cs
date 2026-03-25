using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GunArmAnimCtrl : AnimationController
{
    public enum Motion
    {
        Idle,
        Draw,
        Reload,
        ReloadNoAmmo,
        Fire,
        Run,

        Max
    }
    [SerializeField] Motion _currentMotion;
    public Motion GetMotion { get { return _currentMotion; } }
    Dictionary<Motion, int> _hashTable = new Dictionary<Motion, int>();

    StringBuilder _sb = new StringBuilder();

    public void Play(Motion motion, bool isBlend = true)
    {
        if (_hashTable.TryGetValue(motion, out int hash))
        {
            Play(hash, isBlend);
            _currentMotion = motion;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < (int)Motion.Max; i++)
        {
            Motion motion = (Motion)i;
            _sb.Clear();
            _sb.Append(motion);

            int hash = Animator.StringToHash(_sb.ToString());
            _hashTable.Add(motion, hash);
        }
    }
}

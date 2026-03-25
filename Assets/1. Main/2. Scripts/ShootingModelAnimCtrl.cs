using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingModelAnimCtrl : AnimationController
{
    public enum Motion
    {
        Stand,
        Crouch,
        Prone,

        Max
    }
    Dictionary<Motion, int> _hashTable = new Dictionary<Motion, int>();
    Motion _currentMotion;

    public Motion GetMotion => _currentMotion;

    public void Play(Motion motion, bool isBlend = true)
    {
        if (motion == _currentMotion) return;
        _currentMotion = motion;
        int hash = _hashTable[motion];
        Play(hash, isBlend);
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        for(int i = 0; i < (int)Motion.Max; i++)
        {
            Motion motion = (Motion)i;
            int hash = Animator.StringToHash(motion.ToString());
            _hashTable.Add(motion, hash);
        }
    }
}

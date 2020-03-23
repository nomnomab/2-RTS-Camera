using System;
using System.Collections;
using Attributes;
using UnityEngine;

public class Robot: CustomBehaviour {
    [SerializeField] private Animator _animator;
    [SerializeField] private float _waitTime = 5f;
    [Require] private SimplePath _path;
    private static readonly int WalkAnim = Animator.StringToHash("Walk_Anim");

    private void Start() {
        _path.enabled = false;
        StartCoroutine(WaitForIdle());
    }

    private IEnumerator WaitForIdle() {
        yield return new WaitForSeconds(_waitTime);
        _animator.SetBool(WalkAnim, true);
        _path.enabled = true;
    }
}
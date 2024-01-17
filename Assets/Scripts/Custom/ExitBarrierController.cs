using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ExitBarrierController : MonoBehaviour
{
    // [SerializeField]Transform movingBarrier;
    [SerializeField] Vector3 rotation;
    [SerializeField] float sec;

    public static ExitBarrierController Instance;

    void Awake()
    {
        MakeSingleton();
    }

    private void MakeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    internal void OpenExitBarrier()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalRotate(rotation, sec));
        sequence.Append(transform.DOLocalRotate(Vector3.zero, sec));
        sequence.Play();
    }
}

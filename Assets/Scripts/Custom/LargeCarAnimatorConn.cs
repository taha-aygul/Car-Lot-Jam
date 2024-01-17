using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeCarAnimatorConn : MonoBehaviour
{
    // Animation event
    public void Idle() {
        transform.parent.GetComponent<CarController>().IdleState();
    }
}

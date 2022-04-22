using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireConnection : MonoBehaviour
{
    public Vector3 offset;

    public Vector3 GetAttachLocation()
    {
        return transform.root.position + transform.position + offset;
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(GetAttachLocation(), 0.5f);
    }
}

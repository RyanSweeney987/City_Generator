using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoomRotation : MonoBehaviour
{
    public float rotationSpeed = 1.0f;

    private float currentRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    private void FixedUpdate()
    {
        
    }

    private void OnDrawGizmos()
    {
        if(transform.childCount > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.GetChild(0).transform.position);
        }
    }
}

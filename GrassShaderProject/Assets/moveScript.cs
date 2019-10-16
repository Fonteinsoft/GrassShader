using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveScript : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;
    [Header("Time to go from start to end")]
    public float time = 10f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(start, end, Mathf.Abs(Mathf.Sin(Time.time / time)));
    }
}

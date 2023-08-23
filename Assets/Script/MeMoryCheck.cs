using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MeMoryCheck : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
            this.transform.position = new Vector3(this.transform.position.x+0.85f,this.transform.position.y,this.transform.position.z);
        if (Input.GetKeyDown(KeyCode.B))
            this.transform.position = new Vector3(this.transform.position.x - 0.85f, this.transform.position.y, this.transform.position.z);
    }
}

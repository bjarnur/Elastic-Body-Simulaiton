using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour {

    public float speed;

    private Rigidbody rb;
    private int count;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        count = 0;
        //setCounterText();
        //winText.text = "";
    }
	
	// Update is called once per frame
	void Update () {
        //Vector3 movement = new Vector3(0, -9, 0);
        //rb.AddForce(movement);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;        
        
        for(int i = 0; i < vertices.Length; i++)
        {           
            if (vertices[i].x > 0)
            {
                Debug.Log(vertices[i]);
                vertices[i].x++;
            }                
        }
        mesh.vertices = vertices;
    }
}

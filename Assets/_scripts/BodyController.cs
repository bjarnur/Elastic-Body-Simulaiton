using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour {

    public Vector3 topLeft;
    public Vector3 bottomRight;
    public float spread;
    public GameObject prefab;    
    
    // Use this for initialization
    void Start () {
                
        for(float x = topLeft.x; x < bottomRight.x; x += spread)
        {
            for(float y = topLeft.y; y > bottomRight.y; y -= spread)
            {
                Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        /*
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
        mesh.vertices = vertices;*/
    }
}

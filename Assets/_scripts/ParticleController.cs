using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {

    public float gravitationalForce = -0.1f;
    public float bouncyFactor = 2f;

    private Rigidbody rb;
    private Vector3 speed = new Vector3(0, 0, 0);    

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {

        //rb = GetComponent<Rigidbody>();
        SphereCollider sc = GetComponent<SphereCollider>();
        Vector3 gravity = new Vector3(0, gravitationalForce, 0);
        Vector3 pos = transform.position;
        float radius = sc.radius;

        //Apply gravitational force
        speed += gravity * Time.deltaTime;
        transform.position += speed;

        if(pos.y < 0)
        {
            transform.position = new Vector3(pos.x, -pos.y / bouncyFactor, pos.z);
            speed = -(speed / bouncyFactor);
        }
    }
}

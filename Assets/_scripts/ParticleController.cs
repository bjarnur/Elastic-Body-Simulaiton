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
	public void ApplyGravity () {

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
            speed = (new Vector3(speed.x, -speed.y, speed.z)/ bouncyFactor);
        }
    }

    public void CheckCollisions()
    {
        Vector3 pos = transform.position;
        Debug.Log("Hello " + pos);
    }

    public Vector3 getVelocity()
    {
        return speed;
    }

    public void setVelocity(Vector3 v)
    {
        speed = v;
    }

    public float getRadius()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        return sc.radius;
    }

    public Vector3 getCenter()
    {
        return transform.position;
    }

    public float getBouncyFactor()
    {
        return bouncyFactor;
    }
}

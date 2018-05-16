using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour {

    public Vector3 topLeft;
    public Vector3 bottomRight;
    public float spread;
    public float weight_coefficient = 10;
    public float avg_weight = 1;
    public GameObject particle;
    public float coefficientOfRepulsion = 1.0f;

    private List<GameObject> particles = new List<GameObject>();
    
    // Use this for initialization
    void Start () {

        
        for(float x = topLeft.x; x < bottomRight.x; x += spread)
        {
            for(float y = topLeft.y; y > bottomRight.y; y -= spread)
            {
                float x_use = x;
                if (y % 2 == 0) x_use += (spread / 2);
                GameObject particleInstance = Instantiate(particle, new Vector3(x_use, y, 0), Quaternion.identity);
                if (y % 2 == 0)
                {
                    Renderer renderer = particleInstance.GetComponent<Renderer>();
                    Material m = renderer.material;
                    m.color = new Color(0, 1, 0);
                }
                particles.Add(particleInstance);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        CheckParticleCollisions();
        CheckPlaneCollision();
        foreach (GameObject particleInstance in particles)
        {
            ParticleController pc = particleInstance.GetComponent<ParticleController>();
            pc.ApplyGravity();
        }        
    }

    void CheckParticleCollisions()
    {
        Vector3[,] N = new Vector3[particles.Count, particles.Count];
        bool[,] collide = new bool[particles.Count, particles.Count];
        float[,] W = new float[particles.Count, particles.Count];
        float[] H = new float[particles.Count];

        for(int i = 0; i < particles.Count; i++)
        {
            GameObject pInstance1 = particles[i];
            ParticleController particle1 = pInstance1.GetComponent<ParticleController>();
            Vector3 center = particle1.getCenter();
            float radius = particle1.getRadius();
            float W_i = 0.0f;

            for (int j = 0; j < particles.Count; j++)
            {
                GameObject pInstance2 = particles[j];
                if (Object.ReferenceEquals(pInstance1, pInstance2)) continue;
                
                ParticleController particle2 = pInstance2.GetComponent<ParticleController>();
                Vector3 centerToCheck = particle2.getCenter();
                float distance = Vector3.Distance(center, centerToCheck);
                if (distance < radius * 2)
                {
                    //Compute normalized relative posision
                    N[i, j] = (center - centerToCheck).normalized;
                    
                    //Compute 1 - distance/diameter
                    W[i, j] = Mathf.Abs(1 - (distance / (radius * 2)));
                    W_i += W[i, j];
                    
                    //Just for convenience
                    collide[i, j] = true;
                }                
            }
            H[i] = Mathf.Max(0, weight_coefficient * (W_i - avg_weight));
        }

        for(int i = 0; i < particles.Count; i++)
        {
            GameObject pInstance1 = particles[i];
            ParticleController particle1 = pInstance1.GetComponent<ParticleController>();
            for (int j = 0; j < particles.Count; j++)
            {
                if (!collide[i, j]) continue;
                GameObject pInstance2 = particles[i];
                ParticleController particle2 = pInstance2.GetComponent<ParticleController>();

                float bounce = particle1.getBouncyFactor();
                /*
                Debug.Log("initial speed:" + particle1.getVelocity());
                Debug.Log("delta t: " + Time.deltaTime);
                Debug.Log("bounce: " + bounce);
                Debug.Log("H[j] " + H[j]);
                Debug.Log("H[i] " + H[i]);
                Debug.Log("W[i, j] " + W[i, j]);
                Debug.Log("N[i, j] " + N[i, j]);
                */
                Vector3 v   = particle1.getVelocity()
                            + Time.deltaTime
                            * coefficientOfRepulsion
                            * (H[i] + H[j])
                            * W[i, j]
                            * N[i, j];
                particle1.setVelocity(v);
                //Debug.Log("v: " + v);
            }
        }
        /*
        for (int i = 0; i < particles.Count; i++)
        {

        }*/
    }

    void CheckPlaneCollision()
    {
        Vector3[] N = new Vector3[particles.Count];
        bool[] collide = new bool[particles.Count];
        float[] W = new float[particles.Count];
        float[] H = new float[particles.Count];

        for (int i = 0; i < particles.Count; i++)
        {
            GameObject pInstance1 = particles[i];
            ParticleController particle1 = pInstance1.GetComponent<ParticleController>();
            Vector3 center = particle1.getCenter();
            float radius = particle1.getRadius();
           
            //TODO: Get ground object
               
            float planeCenter = -1;
            float planeThickt = 0.5f;
            Vector3 planeCtrPoint = new Vector3( particle1.getCenter().x, planeCenter, 0);
            float distance = Vector3.Distance(center, planeCtrPoint);
            if (distance < radius + planeThickt)
            {
                //Compute normalized relative posision
                N[i] = (center - planeCtrPoint).normalized;

                //Compute 1 - distance/diameter
                W[i] = Mathf.Abs(1 - (distance / (radius + planeThickt)));

                //Just for convenience
                collide[i] = true;
            }
            H[i] = Mathf.Max(0, weight_coefficient * (W[i] - avg_weight));            
        }

        for (int i = 0; i < particles.Count; i++)
        {
            GameObject pInstance1 = particles[i];
            ParticleController particle1 = pInstance1.GetComponent<ParticleController>();
            for (int j = 0; j < particles.Count; j++)
            {
                if (!collide[i]) continue;

                //TODO: Get ground object

                float bounce = particle1.getBouncyFactor();
                Vector3 v = particle1.getVelocity()
                            + Time.deltaTime
                            * coefficientOfRepulsion
                            * (H[i] + H[j])
                            * W[i]
                            * N[i];
                particle1.setVelocity(v);
                //Debug.Log("v: " + v);
            }
        }
        /*
        for (int i = 0; i < particles.Count; i++)
        {

        }*/
    }
}

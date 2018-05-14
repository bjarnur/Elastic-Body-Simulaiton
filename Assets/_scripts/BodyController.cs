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
                particles.Add(particleInstance);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        CheckParticleCollisions();
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

            for(int j = 0; j < particles.Count; j++)
            {
                GameObject pInstance2 = particles[j];
                if (Object.ReferenceEquals(pInstance1, pInstance2)) continue;

                float W_i = 0.0f;
                ParticleController particle2 = pInstance2.GetComponent<ParticleController>();
                Vector3 centerToCheck = particle2.getCenter();
                float distance = Vector3.Distance(center, centerToCheck);

                if (distance < radius * 2)
                {
                    //Compute normalized relative posision
                    N[i, j] = (centerToCheck - center).normalized;
                    
                    //Compute 1 - distance/diameter
                    W[i, j] = 1 - (distance / (radius * 2));
                    W_i += W[i, j];

                    //Just for convenience
                    collide[i, j] = true;
                }                
                H[i] = Mathf.Max(0, weight_coefficient * (W_i - avg_weight));                
            }
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
                Vector3 v   = particle1.getVelocity()
                            + Time.deltaTime
                            * bounce
                            * (H[i] + H[j])
                            * W[i, j]
                            * N[i, j];
                particle1.setVelocity(v);
                Debug.Log(v);
            }
        }
        /*
        for (int i = 0; i < particles.Count; i++)
        {

        }*/
    }
}

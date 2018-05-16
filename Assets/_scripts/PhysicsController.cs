using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController {

    List<GameObject> particles = new List<GameObject>();
    List<GameObject> elasticBodies = new List<GameObject>();
    List<GameObject> hardBodies = new List<GameObject>();

    public float gravitationalForce = -0.1f;
    public float avg_weight = 0.01f; //Needs to be very low to get a "firm bounce" 
    public float energyLeakUponBounce = 0.99f; //Pretty fun to experiment with, can simulate different materials 
    public float coefficientOfRepulsion = 10.0f; //TODO: Maybe want to be able to set separately per bodies?

    public void DoUpdate ()
    {        
        ApplyGravity();
        CheckCollision();
        UpdatePositions();
    }

    void ApplyGravity()
    {
        foreach(GameObject elasticBody in elasticBodies)
        {
            ElasticBodyController ebc = elasticBody.GetComponent<ElasticBodyController>();
            ApplyGravity(ebc.getParticles());
        }

        ApplyGravity(particles);

        //TODO Implement gravity for hard bodies
    }

    void ApplyGravity(List<GameObject> effectedParticles)
    {
        foreach (GameObject particle in effectedParticles)
        {
            ParticleController pc = particle.GetComponent<ParticleController>();

            Vector3 gravity = new Vector3(0, gravitationalForce, 0);
            Vector3 particleVelocity = pc.getVelocity();
            Vector3 particleCenter = pc.getCenter();

            particleVelocity += gravity * Time.deltaTime;
            particleCenter += particleVelocity;
            pc.setVelocity(particleVelocity);
            //pc.setCenter(particleCenter);
        }
    }

    void CheckCollision()
    {
        List<GameObject> allParticles = getAllParticles();
        CheckCollision(allParticles);
    }

    /**
     Should be called for every single particle in the system, including
     free particles (not part of Elastic Bodies), all particles from 
     Elastic Bodies, and all Hard Bodies (where each hard body is 
     interpreted as a single particle). */
    void CheckCollision(List<GameObject> allParticles)
    {
        Vector3[,] N = new Vector3[allParticles.Count, allParticles.Count];
        bool[,] collide = new bool[allParticles.Count, allParticles.Count];
        float[,] W = new float[allParticles.Count, allParticles.Count];
        float[] H = new float[allParticles.Count];

        for (int i = 0; i < allParticles.Count; i++)
        {
            GameObject pInstance1 = allParticles[i];
            AbstractParticleController particle1 = pInstance1.GetComponent<AbstractParticleController>();
            Vector3 center = particle1.getCenter();
            float W_i = 0.0f;

            for (int j = 0; j < allParticles.Count; j++)
            {
                GameObject pInstance2 = allParticles[j];
                if (Object.ReferenceEquals(pInstance1, pInstance2)) continue;

                AbstractParticleController particle2 = pInstance2.GetComponent<AbstractParticleController>();
                float radius = particle1.getRadius() + particle2.getRadius();
                float distance = particle1.getDistance(particle2);                

                if (distance < radius)
                {
                    Debug.Log("collision " + i + " - " + j);
                    //Compute normalized relative posision
                    N[i, j] = particle1.getNormalizedRelativePos(particle2);

                    //Compute 1 - distance/diameter
                    W[i, j] = Mathf.Abs(1 - (distance / (radius)));
                    W_i += W[i, j];

                    //Just for convenience
                    collide[i, j] = true;
                }
            }
            H[i] = Mathf.Max(0, particle1.getWeightCoefficient() * (W_i - avg_weight));
        }

        //Update each particle in the system
        for (int i = 0; i < allParticles.Count; i++)
        {
            GameObject pInstance1 = allParticles[i];
            AbstractParticleController particle1 = pInstance1.GetComponent<AbstractParticleController>();
            for (int j = 0; j < allParticles.Count; j++)
            {
                if (!collide[i, j]) continue;

                float bounce = particle1.getBouncyFactor();
                Vector3 v = particle1.getVelocity()
                            + Time.deltaTime
                            * coefficientOfRepulsion
                            * (H[i] + H[j])
                            * W[i, j]
                            * N[i, j];
                /*
                Debug.Log("particle id:  " + i);
                Debug.Log(H[i]);
                Debug.Log(H[j]);
                Debug.Log("w " + W[i, j]);
                Debug.Log("n: " + N[i, j]);
                Debug.Log("v: " + v);
                Debug.Log(particle1.getVelocity());*/
                particle1.setVelocity(v * energyLeakUponBounce);
                //Debug.Log(particle1.getVelocity());
            }
        }
    }

    void ApplyElasticForce()
    {

    }

    void PreventPenetration()
    {

    }

    void UpdatePositions()
    {
        List<GameObject> allParticles = getAllParticles();
        foreach(GameObject particle in allParticles)
        {
            AbstractParticleController ctrl = particle.GetComponent<AbstractParticleController>();
            ctrl.setCenter(ctrl.getCenter() + ctrl.getVelocity());
        }
    }

    List<GameObject> getAllParticles()
    {
        List<GameObject> allParticles = new List<GameObject>();
        allParticles.AddRange(hardBodies);
        allParticles.AddRange(particles);
        foreach (GameObject elasticBody in elasticBodies)
        {
            ElasticBodyController bc = elasticBody.GetComponent<ElasticBodyController>();
            allParticles.AddRange(bc.getParticles());
        }
        return allParticles;
    }

    public void setParticles(List<GameObject> particles)
    {
        this.particles = particles;
    }

    public void setElasticBodies(List<GameObject> elasticBodies)
    {
        this.elasticBodies = elasticBodies;
    }

    public void setHardBodies(List<GameObject> hardBodies)
    {
        this.hardBodies = hardBodies;
    }
}

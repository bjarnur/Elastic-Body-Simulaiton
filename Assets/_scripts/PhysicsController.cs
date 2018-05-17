﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController {

    List<GameObject> particles = new List<GameObject>();
    List<GameObject> elasticBodies = new List<GameObject>();
    List<GameObject> hardBodies = new List<GameObject>();

    public float gravitationalForce = -0.1f;
    public float avg_weight = 0.0005f; //Needs to be very low to get a "firm bounce" 
    public float energyLeakUponBounce = 0.9f; //Pretty fun to experiment with, can simulate different materials 
    public float coefficientOfRepulsion = 100.0f; //TODO: Maybe want to be able to set separately per bodies?

    public void DoUpdate ()
    {
        //PreventPenetration();        
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
        }
    }

    void CheckCollision()
    {        
        List<GameObject> allParticles = getAllParticles();
        Debug.Log(allParticles.Count);
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

                /*
                Debug.Log("Particle 1 center: " + particle1.getCenter());
                Debug.Log("Particle 2 center: " + particle2.getCenter());
                Debug.Log("Distance " + distance);
                Debug.Log("Radius " + radius);
                */

                if (distance <= radius)
                {
                    //Compute normalized relative posision
                    N[i, j] = particle1.getNormalizedRelativePos(particle2);

                    //Compute 1 - distance/diameter
                    W[i, j] = Mathf.Max(0, 1 - (distance / (radius)));
                    W_i += W[i, j];

                    //Just for convenience
                    collide[i, j] = true;
                }
            }
            H[i] = Mathf.Max(0, particle1.getWeightCoefficient() * (W_i - avg_weight));
            /*
            Debug.Log("wi: " + W_i);
            Debug.Log("H: " + H[i]);
            */
        }

        //Update each particle in the system
        for (int i = 0; i < allParticles.Count; i++)
        {
            GameObject pInstance1 = allParticles[i];
            AbstractParticleController particle1 = pInstance1.GetComponent<AbstractParticleController>();
            for (int j = 0; j < allParticles.Count; j++)
            {
                if (!collide[i, j]) continue;

                GameObject pInstance2 = allParticles[i];
                AbstractParticleController particle2 = pInstance2.GetComponent<AbstractParticleController>();

                float temp_h = H[i];
                if (particle2 is PlaneController)
                    temp_h *= 10000;

                float bounce = particle1.getBouncyFactor();
                Vector3 v = particle1.getVelocity()
                            + Time.deltaTime
                            * coefficientOfRepulsion
                            * (temp_h + H[j])
                            * W[i, j]
                            * N[i, j];
                /*
                Debug.Log("diff = " + Time.deltaTime
                            * coefficientOfRepulsion
                            * (H[i] + H[j])
                            * W[i, j]
                            * N[i, j]);
                Debug.Log("particle id:  " + i);
                Debug.Log("h this" + H[i]);
                Debug.Log("h other " + H[j]);
                Debug.Log("w " + W[i, j]);
                Debug.Log("n: " + N[i, j]);
                Debug.Log("v: " + v);
                Debug.Log("current v:  " + particle1.getVelocity());*/
                particle1.setVelocity(v /* energyLeakUponBounce * particle2.getDampeningEffect()*/);
            }
        }
    }

    void ApplyElasticForce()
    {

    }

    void PreventPenetration()
    {
        List<GameObject> allParticles = getAllParticles();
        foreach (GameObject particle in allParticles)
        {
            AbstractParticleController ctrl = particle.GetComponent<AbstractParticleController>();
            if (ctrl is ParticleController)
            {
                ParticleController particleCtrl = (ParticleController)ctrl;
                PreventPenetration(particleCtrl);
            }
        }
    }

    void PreventPenetration(ParticleController particleCtrl)
    {
        Vector3 positionStart = particleCtrl.getCenter();
        Vector3 positionEnd = positionStart + particleCtrl.getVelocity();
        //Debug.Log("velocity: " + particleCtrl.getVelocity());
        Vector3 ultimatePosition = new Vector3();
        bool intersectHappened = false;
        foreach (GameObject hardBody in hardBodies)
        {
            bool intersect;
            Vector3 planeNorm;
            Vector3 intersectionPoint; 
            PlaneController planeCtrl = hardBody.GetComponent<PlaneController>();
            intersect = planeCtrl.checkParticleCollision(out intersectionPoint, 
                                                           out planeNorm, 
                                                           particleCtrl);
            //Debug.Log(intersectionPoint);
            //Debug.Log(particleCtrl.getCenter());
            if(intersect)
            {
                //Debug.Log("are intersecting");
                if (intersectHappened)
                {
                    Vector3 tempPosition = intersectionPoint; //+ (planeNorm * planeCtrl.getRadius());
                    float currentDistance = Vector3.Distance(positionStart, ultimatePosition);
                    float newDistance = Vector3.Distance(positionStart, tempPosition);
                    if (newDistance < currentDistance)
                        ultimatePosition = tempPosition;
                } else
                {
                    ultimatePosition = intersectionPoint;// + (planeNorm * planeCtrl.getRadius());
                    intersectHappened = true;
                }
            }
        }

        if (intersectHappened)
        {            
            particleCtrl.setCenter(ultimatePosition);
            //Debug.Log("overriding location!");
            //Debug.Log("velocity: " + particleCtrl.getVelocity());
        }
        else
        {
            particleCtrl.setCenter(positionEnd);
        }
    }

    void UpdatePositions()
    {
        List<GameObject> allParticles = getAllParticles();
        foreach(GameObject particle in allParticles)
        {
            AbstractParticleController ctrl = particle.GetComponent<AbstractParticleController>();
            if (ctrl is ParticleController)
                PreventPenetration((ParticleController)ctrl);
            else
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

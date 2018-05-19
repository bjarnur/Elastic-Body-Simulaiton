using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController {

    List<GameObject> particles = new List<GameObject>();
    List<GameObject> elasticBodies = new List<GameObject>();
    List<GameObject> hardBodies = new List<GameObject>();

    public float gravitationalForce = -0.05f;
    public float avg_weight = 0f; //Needs to be very low to get a "firm bounce" 
    public float energyLeakUponBounce = 0.95f; //Pretty fun to experiment with, can simulate different materials 
    public float energyLeakUponGroundContact = 0.8f;
    public float coefficientOfRepulsion = 10.0f; //TODO: Maybe want to be able to set separately per bodies?

    public void DoUpdate ()
    {
        //PreventPenetration();        
        ApplyGravity();
        CheckCollision();
        ApplyElasticForce();
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

            particleVelocity += gravity * Time.deltaTime;
            pc.setVelocity(particleVelocity);
        }
    }

    void CheckCollision()
    {        
        List<ParticleController> allParticles = getAllElasticParticles();
        //Debug.Log(allParticles.Count);
        CheckCollision(allParticles);
    }

    /**
     Should be called for every single particle in the system, including
     free particles (not part of Elastic Bodies), all particles from 
     Elastic Bodies, and all Hard Bodies (where each hard body is 
     interpreted as a single particle). */
    void CheckCollision(List<ParticleController> allParticles)
    {
        Vector3[,] N = new Vector3[allParticles.Count, allParticles.Count];
        bool[,] collide = new bool[allParticles.Count, allParticles.Count];
        float[,] W = new float[allParticles.Count, allParticles.Count];
        float[] H = new float[allParticles.Count];

        for (int i = 0; i < allParticles.Count; i++)
        {
            ParticleController particle1 = allParticles[i];
            Vector3 center = particle1.getCenter();
            float W_i = 0.0f;

            for (int j = 0; j < allParticles.Count; j++)
            {
                ParticleController particle2 = allParticles[j];
                if (Object.ReferenceEquals(particle1, particle2)) continue;

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
                    W[i, j] = Mathf.Max(0, (1 - (distance / (radius))));
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
            ParticleController particle1 = allParticles[i];
            for (int j = 0; j < allParticles.Count; j++)
            {
                if (!collide[i, j]) continue;

                ParticleController particle2 = allParticles[i];
                float bounce = particle1.getBouncyFactor();
                Vector3 v = particle1.getVelocity()
                            + Time.deltaTime
                            * coefficientOfRepulsion
                            * (H[i] + H[j])
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
                //Debug.Log("setting velocity: " + v);
                particle1.setVelocity(v * energyLeakUponBounce /* particle2.getDampeningEffect()*/);
            }
        }
    }

    float springConstant = 0.5f;

    void ApplyElasticForce()
    {
        List<ParticleController> particles = getAllElasticParticles();
        foreach(ParticleController particle in particles)
        {
            List<float> distancesToNeighbors = particle.getNeighborDistances();
            List<ParticleController> neighbors = particle.getNeighbors();
            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector3 diff    = Time.deltaTime 
                                * springConstant 
                                * (particle.getDistance(neighbors[i]) - distancesToNeighbors[i])
                                * particle.getNormalizedRelativePos(neighbors[i]);
                particle.setVelocity(particle.getVelocity() + diff);
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
            
            particleCtrl.setVelocity(new Vector3(particleCtrl.getVelocity().x,
                                                 -particleCtrl.getVelocity().y,
                                                 particleCtrl.getVelocity().z) * energyLeakUponGroundContact);
            particleCtrl.setCenter(ultimatePosition/* + particleCtrl.getVelocity()*/);
            //Debug.Log("overriding location!");
            //Debug.Log("velocity: " + particleCtrl.getVelocity().x + ", " + particleCtrl.getVelocity().y);
        }
        else
        {
            particleCtrl.setCenter(positionEnd);
        }
    }

    void UpdatePositions()
    {
        List<AbstractBodyController> allBodies = getAllMovableBodies();
        foreach(AbstractBodyController body in allBodies)
        {
            if (body is ParticleController)
                PreventPenetration((ParticleController)body);
            else
                body.setCenter(body.getCenter() + body.getVelocity());
        }
    }

    List<AbstractBodyController> getAllMovableBodies()
    {
        List<GameObject> allParticles = new List<GameObject>();
        List<AbstractBodyController> allParticleControllers = new List<AbstractBodyController>();

        allParticles.AddRange(hardBodies);
        allParticles.AddRange(particles);
        foreach (GameObject elasticBody in elasticBodies)
        {
            ElasticBodyController bc = elasticBody.GetComponent<ElasticBodyController>();
            allParticles.AddRange(bc.getParticles());
        }

        foreach (GameObject particle in allParticles)
        {
            AbstractBodyController ctrl = particle.GetComponent<AbstractBodyController>();
            allParticleControllers.Add(ctrl);
        }
        return allParticleControllers;
    }

    List<ParticleController> getAllElasticParticles()
    {
        List<GameObject> allParticles = new List<GameObject>();
        List<ParticleController> allParticleControllers = new List<ParticleController>();

        allParticles.AddRange(particles);
        foreach (GameObject elasticBody in elasticBodies)
        {
            ElasticBodyController bc = elasticBody.GetComponent<ElasticBodyController>();
            allParticles.AddRange(bc.getParticles());
        }

        foreach(GameObject particle in allParticles)
        {
            ParticleController ctrl = particle.GetComponent<ParticleController>();
            allParticleControllers.Add(ctrl);
        }
        return allParticleControllers;
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

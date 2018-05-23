using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController {

    List<GameObject> particles = new List<GameObject>();
    List<GameObject> elasticBodies = new List<GameObject>();
    List<GameObject> hardBodies = new List<GameObject>();

    public float gravitationalForce = -5f;
    public float avg_weight = 0.0005f; //Needs to be very low to get a "firm bounce" 
    public float energyLeakUponBounce = 0.95f; //Pretty fun to experiment with, can simulate different materials 
    public float energyLeakUponGroundContact = 0.8f;
    public float coefficientOfRepulsion = -10.0f; //TODO: Maybe want to be able to set separately per bodies?
    float springConstant = 10f;

    public void DoUpdate ()
    {
        //PreventPenetration();        
        ApplyGravity();
        CheckCollision();
        ApplyElasticForce();
        UpdatePositions();
    }

    /* Just experimenting
    void ApplyTensileForces()
    {
        List<ParticleController> allParticles = getAllElasticParticles();
        Vector3[,] N = new Vector3[allParticles.Count, allParticles.Count];
        Vector3[] S = new Vector3[allParticles.Count];
        float[,] W = new float[allParticles.Count, allParticles.Count];
        float[] W_list = new float[allParticles.Count];

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

                N[i, j] = particle1.getNormalizedRelativePos(particle2);
                W[i, j] = Mathf.Max(0, (1 - (distance / (radius))));
                S[i] += ((1 - W[i, j]) * (W[i, j]) * (N[i, j]));

                W_list[i] += W[i, j];
            }
        }

        for (int i = 0; i < allParticles.Count; i++)
        {
            ParticleController particle1 = allParticles[i];
            Vector3 center = particle1.getCenter();

            for (int j = 0; j < allParticles.Count; j++)
            {
                ParticleController particle2 = allParticles[j];
                if (Object.ReferenceEquals(particle1, particle2)) continue;

                float a = 0.2f;
                float b = 0.2f;

                float A = a * (W_list[i] + W_list[j] - (2 * avg_weight));
                float B = b * Vector3.Dot(S[j] - S[i], N[i, j]);
                particle1.setVelocity(particle1.getVelocity() - (Time.deltaTime *
                                                                ((A + B) * N[i, j])));
            }
        }
    }*/
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

    void HandleCollision (ParticleController thisParticle, ParticleController otherParticle) {
        
    }

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
                                * (neighbors[i].getDistance(particle) - distancesToNeighbors[i])
                                * particle.getNormalizedRelativePos(neighbors[i]);

                Vector3 finalPos = particle.getCenter() + diff;
                if(Vector3.Distance(particle.getCenter(), finalPos) < Vector3.Distance(particle.getCenter(), neighbors[i].getCenter()))
                {
                    particle.setVelocity(particle.getVelocity() + diff);
                }                

                //if (Vector3.Magnitude(diff) > 100) Debug.Log(Time.deltaTime);
            }
        }
    }

    Vector3 GetFinalPosition(ParticleController particleCtrl)
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
            //particleCtrl.setCenter(ultimatePosition/* + particleCtrl.getVelocity()*/);
            return ultimatePosition;
            //Debug.Log("overriding location!");
            //Debug.Log("velocity: " + particleCtrl.getVelocity().x + ", " + particleCtrl.getVelocity().y);
        }
        else
        {
            //particleCtrl.setCenter(positionEnd);
            return positionEnd;
        }

    }

    Vector3[] getAABB(Vector3 startPos, Vector3 endPos, float radius)
    {
        Vector3[] AABB = new Vector3[3];
        Vector3 topLeft = new Vector3(Mathf.Min(endPos.x, startPos.x) - radius,
                                             Mathf.Max(endPos.y, startPos.y) + radius,
                                             0);
        Vector3 bottomRight = new Vector3(Mathf.Max(endPos.x, startPos.x) + radius,
                                          Mathf.Min(endPos.y, startPos.y) - radius,
                                     0);
        AABB[0] = topLeft;
        AABB[1] = bottomRight;
        AABB[2] = endPos;
        return AABB;
    }

    void UpdatePositions()
    {
        Dictionary<ParticleController, Vector3[]> ParticleAABB = new Dictionary<ParticleController, Vector3[]>();
        List<AbstractBodyController> allBodies = getAllMovableBodies();
        List<ParticleController> elasticBodies = getAllElasticParticles();       
        foreach(AbstractBodyController body in allBodies)
        {
            //All particles slowly lose energy with time
            body.setVelocity(body.getVelocity() * 0.99f);

            if (body is ParticleController)
            {
                ParticleController particle = (ParticleController)body;
                Vector3 finalPosition = GetFinalPosition(particle);
                Vector3[] AABB = getAABB(particle.getCenter(), finalPosition, particle.getRadius());
                ParticleAABB.Add((ParticleController) body, AABB);
            }
            else
            {
                body.setCenter(body.getCenter() + body.getVelocity());
            }
        }

        //Broad stage collision detection
        foreach(ParticleController thisParticle in elasticBodies)
        {
            foreach (ParticleController otherParticle in elasticBodies)
            {
                if (Object.ReferenceEquals(thisParticle, otherParticle)) continue;
                //bool intersect = IntervalColliionCheck(thisParticle, otherParticle, 0, Time.deltaTime);
                float hit;
                bool intersect = IntervalCollisionCheck(thisParticle, otherParticle, 0, 1.0f, out hit);
                if(intersect)
                {
                    thisParticle.setCenter(thisParticle.getCenter() + thisParticle.getVelocity() * hit);
                    otherParticle.setCenter(otherParticle.getCenter() + otherParticle.getVelocity() * hit);
                    thisParticle.setRemainingTime(1.0f - hit);
                    otherParticle.setRemainingTime(1.0f - hit);
                    HandleCollision(thisParticle, otherParticle);
                }
            }
        }
        foreach (ParticleController thisParticle in elasticBodies)
        {
            thisParticle.setCenter(ParticleAABB[thisParticle][2]);
        }
    }

    bool IntervalColliionCheck(ParticleController thisParticle, ParticleController otherParticle, float start, float end)
    {
        float timeDiff = end - start;
        //Debug.Log("Start  " + start);
        //Debug.Log("End " + end);
        //Debug.Log("Diff  " + timeDiff);
        Vector3 thisEndPos = thisParticle.getCenter() + (thisParticle.getVelocity() * timeDiff);
        Vector3 otherEndPos = otherParticle.getCenter() + (otherParticle.getVelocity() * timeDiff);
        Vector3[] thisAABB = getAABB(thisParticle.getCenter(), thisEndPos, thisParticle.getRadius());
        Vector3[] otherAABB = getAABB(otherParticle.getCenter(), otherEndPos, otherParticle.getRadius());

        //if (start == 0)
        //  GUI.Box(new Rect(thisAABB[1].x, thisAABB[0].x, thisAABB[1].y, thisAABB[0].x), "foo");

        Debug.Log("current " + thisParticle.getCenter() + " other " + otherParticle.getCenter());
        Debug.Log("top left 1 " + thisAABB[0] + " bottom right 1" + thisAABB[1]);
        Debug.Log("top left 2 " + otherAABB[0] + " bottom right 2" + otherAABB[1]);

        bool xOverlap = (thisAABB[0].x < otherAABB[1].x) && (thisAABB[1].x > otherAABB[0].x);
        bool yOverlap = (thisAABB[0].y > otherAABB[1].y) && (thisAABB[1].y < otherAABB[0].y);
        if (xOverlap && yOverlap)
        {
            Debug.Log("Maybe a collision");
            float midTime = 0.5f * timeDiff;
            if(Vector3.Distance(thisEndPos, otherEndPos) < thisParticle.getRadius() + otherParticle.getRadius())
            {
                //Debug.Log("Curent locations " + thisParticle.getCenter() + " and " + otherParticle.getCenter());
                //Debug.Log("Velocities " + thisParticle.getVelocity() + " and " + otherParticle.getVelocity());
                //Debug.Log("Distance " + Vector3.Distance(thisEndPos, otherEndPos));
                return true;
            }

            if (    Vector3.Distance(thisParticle.getCenter(), thisEndPos) < 1 &&
                    Vector3.Distance(otherParticle.getCenter(), otherEndPos) < 1) return false;

            bool firstCheck = IntervalColliionCheck(thisParticle, otherParticle, start, start + midTime);
            bool secondCheck = IntervalColliionCheck(thisParticle, otherParticle, start + midTime, end);

            return firstCheck || secondCheck;
        }
        return false;
    }

    bool IntervalCollisionCheck(ParticleController thisParticle, ParticleController otherParticle, float start, float end, out float hit) 
    {
        hit = 0.0f;
        //Debug.Log("start " + start);
        //Debug.Log("end " + end);

        float thisParticleMaximumMove = ParticleMaximumMove(thisParticle, start, end);
        float otherParticleMaximumMove = ParticleMaximumMove(otherParticle, start, end);
        float maxMoveDistance = thisParticleMaximumMove + otherParticleMaximumMove;

        float minimumDistanceAtStart = MinimumDistanceAtTime(thisParticle, otherParticle, start);
        if (minimumDistanceAtStart > maxMoveDistance)
            return false;
        float minimumDistanceAtEnd = MinimumDistanceAtTime(thisParticle, otherParticle, end);
        if (minimumDistanceAtEnd > maxMoveDistance)
            return false;

        if (end - start <  0.01) {
            hit = start;
            Debug.Log(end);
            return true;
        }

        float mid = (start + end) * 0.5f;
        if (IntervalCollisionCheck(thisParticle, otherParticle, start, mid, out hit))
            return true;
        else
            return IntervalCollisionCheck(thisParticle, otherParticle, mid, end, out hit);
    }

    float ParticleMaximumMove(ParticleController particle, float start, float end)
    {
        return Vector3.Distance(particle.getCenter() + particle.getVelocity() * start, particle.getCenter() + particle.getVelocity() * (end - start));
    }

    float MinimumDistanceAtTime(ParticleController firstParticle, ParticleController secondParticle, float time) 
    {
        Vector3 firstEndPos = firstParticle.getCenter() + firstParticle.getVelocity() * time;
        Vector3 secondEndPos = secondParticle.getCenter() + secondParticle.getVelocity() * time;

        return Vector3.Distance(firstEndPos, secondEndPos) - firstParticle.getRadius() - secondParticle.getRadius();
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

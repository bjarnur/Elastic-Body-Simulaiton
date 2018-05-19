using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : AbstractBodyController
{
    public float radius = 1f;

    List<float> distancesToNeighbors = new List<float>();
    List<ParticleController> neighbors = new List<ParticleController>();    

    void Start ()
    {
        // Use this for any initialization we might need
        isMovable = true;
    }

    public float getDistance(AbstractBodyController other)
    {
        if(other is ParticleController)
        {
            return Vector3.Distance(this.getCenter(), other.getCenter());
        }
        else //As of now only plane
        {
            Vector3 planeCtrPoint = new Vector3(this.getCenter().x,
                                                other.getCenter().y,
                                                0);
            return Vector3.Distance(this.getCenter(), planeCtrPoint);
        }
    }

    /**      
     Used to set the direction of the force applied to particles */
    public Vector3 getNormalizedRelativePos(AbstractBodyController other)
    {
        if(other is ParticleController)
        {
            return (this.getCenter() - other.getCenter()).normalized;
        }
        else //As of now only plane
        {
            Vector3 planeCtrPoint = new Vector3(this.getCenter().x,
                                                other.getCenter().y,
                                                0);
            return (this.getCenter() - planeCtrPoint).normalized;
        }
    }

    public float getRadius()
    {
        return radius;
    }

    public void addNeighbor(ParticleController particle, float distance)
    {
        this.neighbors.Add(particle);
        this.distancesToNeighbors.Add(distance);
    }

    public List<ParticleController> getNeighbors()
    {
        return neighbors;
    }

    public List<float> getNeighborDistances()
    {
        return distancesToNeighbors;
    }
}

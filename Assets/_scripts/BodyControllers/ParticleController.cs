using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : AbstractParticleController
{
    public float radius = 1f;
    
    void Start ()
    {
        // Use this for any initialization we might need
    }

    public override float getDistance(AbstractParticleController other)
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
    public override Vector3 getNormalizedRelativePos(AbstractParticleController other)
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
    
    public override float getRadius()
    {
        return radius;
    }

    public override Vector3 getCenter()
    {
        return transform.position;
    }

    public override void setVelocity(Vector3 velocity)
    {
        this.velocity = velocity; ;
    }
}

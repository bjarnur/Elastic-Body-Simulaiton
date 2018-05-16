using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : AbstractParticleController
{
    public float radius = 1f;

    // Use this for initialization
    void Start ()
    {
        this.weightCoefficient = 2;
    }

    public override float getDistance(AbstractParticleController other)
    {
        if(other is ParticleController)
        {
            return Vector3.Distance(this.getCenter(), other.getCenter());
        }
        else //HardBody
        {
            HardBodyController hbc = (HardBodyController)other;
            Vector3 planeCtrPoint = new Vector3(this.getCenter().x,
                                                //other.getCenter().y,
                                                hbc.yPos,
                                                0);
            //Debug.Log(planeCtrPoint);
            //Debug.Log(this.getCenter());
            //Debug.Log(Vector3.Distance(this.getCenter(), planeCtrPoint));
            return Vector3.Distance(this.getCenter(), planeCtrPoint);
        }
    }

    public override Vector3 getNormalizedRelativePos(AbstractParticleController other)
    {
        if(other is ParticleController)
        {
            return (this.getCenter() - other.getCenter()).normalized;
        }
        else
        {
            HardBodyController hbc = (HardBodyController)other;
            Vector3 planeCtrPoint = new Vector3(this.getCenter().x,
                                                hbc.yPos,
                                                0);
            return (this.getCenter() - planeCtrPoint).normalized;
        }
    }
    
    public override float getRadius()
    {
        return radius;
    }

    public override void setVelocity(Vector3 velocity)
    {
        //Debug.Log("setting velocity: " + velocity);
        this.velocity = velocity; ;
    }
}

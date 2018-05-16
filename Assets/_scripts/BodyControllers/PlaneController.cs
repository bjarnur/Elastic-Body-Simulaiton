using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : AbstractParticleController {

    public float xPos;
    public float yPos;
    public float zPos;
    public float height;
    public float witdh;
    public bool isMovable;
    public GameObject prefab;

    public override float getDistance(AbstractParticleController other)
    {
        if (other is ParticleController)
        {
            Vector3 planeCtrPoint = new Vector3(other.getCenter().x,
                                                this.getCenter().y,
                                                0);
            return Vector3.Distance(planeCtrPoint, other.getCenter());
        }
        else return 0.0f;//TODO: Cover any other plausible cases
    }

    public override Vector3 getNormalizedRelativePos(AbstractParticleController other)
    {
        if (other is ParticleController)
        {
            Vector3 planeCtrPoint = new Vector3(other.getCenter().x,
                                            this.getCenter().y,
                                            0);
            return (planeCtrPoint - other.getCenter()).normalized;
        }
        else return new Vector3(); //TODO: Cover any other plausible cases
    }


    // Use this for initialization
    void Start()
    {
        prefab = Instantiate(prefab, new Vector3(xPos, yPos, zPos), Quaternion.identity);
        dampeningEffect = 0.9f;
    }
    
    public override float getRadius()
    {
        return height / 2; 
    }

    public override Vector3 getCenter()
    {
        //TODO: some issues with using transpose.position, could need to sort that out
        return new Vector3(xPos, yPos, zPos);
    }

    public override void setVelocity(Vector3 velocity)
    {
        if(isMovable)
        {
            this.velocity = velocity;
        }
    }
}

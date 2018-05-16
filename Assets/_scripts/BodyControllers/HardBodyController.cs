using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardBodyController : AbstractParticleController {

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
            //TODO: This implementation assumes this is plane!!
            Vector3 planeCtrPoint = new Vector3(other.getCenter().x,
                                                this.yPos,
                                                0);
            //Debug.Log(Vector3.Distance(other.getCenter(), planeCtrPoint));
            return Vector3.Distance(planeCtrPoint, other.getCenter());
        }
        else return 0.0f;// TODO
    }

    public override Vector3 getNormalizedRelativePos(AbstractParticleController other)
    {
        if (other is ParticleController)
        {
            Vector3 planeCtrPoint = new Vector3(other.getCenter().x,
                                            //this.getCenter().y,
                                            this.yPos,
                                            0);
            return (planeCtrPoint - other.getCenter()).normalized;
        }
        else return new Vector3(); //TODO
    }


    // Use this for initialization
    void Start()
    {
        prefab = Instantiate(prefab, new Vector3(xPos, yPos, zPos), Quaternion.identity);
        this.weightCoefficient = 0;
        Debug.Log("AAAA " + yPos);
        Debug.Log("SDFA" + this.getCenter());
    }
    
    public override float getRadius()
    {
        return height / 2; //TODO assuming plane!!
    }

    public override void setVelocity(Vector3 velocity)
    {
        if(isMovable)
        {
            this.velocity = velocity;
        }
    }
}

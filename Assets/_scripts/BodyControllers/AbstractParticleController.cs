using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractParticleController : MonoBehaviour
{
    public float bouncyFactor = 1f;
    public float weightCoefficient = 1f;
    public float dampeningEffect = 1f;
    protected Vector3 velocity = new Vector3(0, 0, 0);

    public abstract float getDistance(AbstractParticleController other);
    
    public abstract Vector3 getNormalizedRelativePos(AbstractParticleController other);
    
    public abstract float getRadius();

    public abstract Vector3 getCenter();

    public abstract void setVelocity(Vector3 v);
   
    /*------------------------------------*/
    /*        Getters and Setters         */
    /*------------------------------------*/

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setCenter(Vector3 c)
    {
        transform.position = c;
    }

    public float getBouncyFactor()
    {
        return bouncyFactor;
    }

    public float getWeightCoefficient()
    {
        return weightCoefficient;
    }

    public float getDampeningEffect()
    {
        return dampeningEffect;
    }
}

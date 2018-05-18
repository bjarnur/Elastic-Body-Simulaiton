using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**    
 Provides behavior that is mutual to all bodies in the world 
*/
public abstract class AbstractBodyController : MonoBehaviour
{
    public bool isMovable = false;
    public float bouncyFactor = 1f;    
    public float dampeningEffect = 1f;
    public float weightCoefficient = 1f;

    protected Vector3 velocity = new Vector3(0, 0, 0);


    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setVelocity(Vector3 velocity)
    {
        if (isMovable)
        {
            this.velocity = velocity;
        }
    }

    public Vector3 getCenter()
    {
        return transform.position;
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

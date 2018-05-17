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

    public Vector3 getPointClosestToPlane(ParticleController ctrl, Vector3 planeNorm, Vector3 pointOnPlane)
    {
        Plane plane = new Plane(planeNorm, pointOnPlane);
        Vector3 closestPointOnPlane = plane.ClosestPointOnPlane(ctrl.getCenter());
        //Debug.Log("Closes point on plane " + closestPointOnPlane);
        Vector3 directionToPoint = ctrl.getCenter() - closestPointOnPlane;
        return ctrl.getCenter() + (directionToPoint.normalized * ctrl.getRadius());
    }

    public bool checkParticleCollision(out Vector3 intersectPoint, out Vector3 planeNorm, ParticleController ctrl)
    {   
        Vector3 point1 = new Vector3(xPos, yPos + (height / 2), zPos);
        Vector3 point2 = new Vector3(xPos + 10, yPos + (height / 2), zPos);
        Vector3 point3 = new Vector3(xPos, yPos + (height / 2), zPos + 10);

        planeNorm = Utilities.GetPlaneNormal(point1, point2, point3);
        Vector3 lineOrigin = getPointClosestToPlane(ctrl, planeNorm, point1);
        Vector3 lineEnd = lineOrigin + ctrl.getVelocity();


        Vector3 lineDirection = (lineOrigin - lineEnd).normalized;

        bool parallel = Utilities.LinePlaneIntersection(out intersectPoint, 
                                                        lineOrigin, 
                                                        lineDirection, 
                                                        planeNorm, point1);

        if(parallel)
        {
            //Debug.Log("Particle center " + ctrl.getCenter());
            //Debug.Log("Intersect point " + intersectPoint);
            intersectPoint = intersectPoint + planeNorm * ctrl.getRadius();
            //float distLine = Vector3.Distance(lineOrigin, lineEnd);
            float distLine = Vector3.Magnitude(ctrl.getVelocity());
            float distToIntersect = Vector3.Distance(ctrl.getCenter(), intersectPoint);
            //Debug.Log("movign to center " + intersectPoint);
            //Debug.Log("distance to intersect: " + distToIntersect);
            //Debug.Log("distance of line: " + distLine);
            if (distToIntersect < distLine)
            {
                return true;
            }
        }

        return false;
    }

    // Use this for initialization
    void Start()
    {
        prefab = Instantiate(prefab, new Vector3(xPos, yPos, zPos), Quaternion.identity);
        //Debug.DrawLine(new Vector3(xPos - 20, yPos, 0), new Vector3(xPos + 20, yPos, 0), Color.white, 2);
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

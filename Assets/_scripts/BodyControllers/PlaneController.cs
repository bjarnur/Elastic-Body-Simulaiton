using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlaneController : AbstractBodyController {

    public float xPos;
    public float yPos;
    public float zPos;
    public float height;
    public float witdh;
    public GameObject prefab;

    public Vector3 getPointClosestToPlane(ParticleController ctrl, Vector3 planeNorm, Vector3 pointOnPlane)
    {
        Plane plane = new Plane(planeNorm, pointOnPlane);
        Vector3 closestPointOnPlane = plane.ClosestPointOnPlane(ctrl.getCenter());        
        Vector3 directionToPoint =  closestPointOnPlane - ctrl.getCenter();

        //Debug.Log("Closes point on plane " + closestPointOnPlane);
        return ctrl.getCenter() + (directionToPoint.normalized * ctrl.getRadius());
    }

    public bool checkParticleCollision(out Vector3 intersectPoint, out Vector3 planeNorm, ParticleController ctrl)
    {   
        Vector3 point1 = new Vector3(xPos, yPos + (height / 2), zPos);
        Vector3 point2 = new Vector3(xPos + 10, yPos + (height / 2), zPos);
        Vector3 point3 = new Vector3(xPos, yPos + (height / 2), zPos + 10);

        planeNorm = Utilities.GetPlaneNormal(point1, point2, point3);
        if (Vector3.Dot(planeNorm, ctrl.getVelocity()) > 0)
        {
            intersectPoint = new Vector3();
            return false;
        }

        Vector3 lineOrigin = getPointClosestToPlane(ctrl, planeNorm, point1);
        Vector3 lineEnd = lineOrigin + (ctrl.getVelocity() * Time.deltaTime);
        Vector3 lineDirection = (lineOrigin - lineEnd).normalized;
        /*
        Debug.Log("line origin " + lineOrigin);
        Debug.Log("line direction " + lineDirection);
        Debug.Log("plane norm " + planeNorm);
        Debug.Log("ponit on plane " + point1    );
        */

        bool parallel = Utilities.LinePlaneIntersection(out intersectPoint, 
                                                        lineOrigin, 
                                                        lineDirection, 
                                                        planeNorm, point1);

        if(parallel)
        {
            intersectPoint = intersectPoint + planeNorm * ctrl.getRadius();            
            float distLine = Vector3.Magnitude(ctrl.getVelocity() * Time.deltaTime);
            float distToIntersect = Vector3.Distance(ctrl.getCenter(), intersectPoint);

            //Debug.Log("Particle center " + ctrl.getCenter());
            //Debug.Log("Intersect point " + intersectPoint);
            //float distLine = Vector3.Distance(lineOrigin, lineEnd);
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
        dampeningEffect = 0.9f;
    }
}

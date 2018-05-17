using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    /**
        Author: Bit Barrel Media
        Source: http://wiki.unity3d.com/index.php/3d_Math_functions
         
        Get the intersection between a line and a plane. 
        of the line and plane are not parallel, the function outputs true, otherwise false. 

    */
    public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
    {

        float length;
        float dotNumerator;
        float dotDenominator;
        Vector3 vector;
        intersection = Vector3.zero;

        //calculate the distance between the linePoint and the line-plane intersection point
        dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
        dotDenominator = Vector3.Dot(lineVec, planeNormal);

        //line and plane are not parallel
        if (dotDenominator != 0.0f)
        {
            length = dotNumerator / dotDenominator;

            //create a vector from the linePoint to the intersection point
            vector = SetVectorLength(lineVec, length);

            //get the coordinates of the line-plane intersection point
            intersection = linePoint + vector;

            return true;
        }

        //output not valid
        else
        {
            return false;
        }
    }

    //create a vector of direction "vector" with length "size"
    public static Vector3 SetVectorLength(Vector3 vector, float size)
    {

        //normalize the vector
        Vector3 vectorNormalized = Vector3.Normalize(vector);

        //scale the vector
        return vectorNormalized *= size;
    }

    public static Vector3 GetPlaneNormal(Vector3 origin, Vector3 point1, Vector3 point2)
    {
        Vector3 vec1 = (point1 - origin).normalized;
        Vector3 vec2 = (point2 - origin).normalized;
        return Vector3.Cross(vec1, vec2).normalized;
    }

}

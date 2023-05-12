using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonFormula
{
    public static Vector3 PointFromPointToLine(Vector3 p1, Vector3 p2, Vector3 p)
    {
        float k = -((p1.x - p.x) * (p2.x - p1.x) + (p1.y - p.y) * (p2.y - p1.y) + (p1.z - p.z) * (p2.z - p1.z))
            / (Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2) + Mathf.Pow(p2.z - p1.z, 2));
        float pnx = k * (p2.x - p1.x) + p1.x;
        float pny = k * (p2.y - p1.y) + p1.y;
        float pnz = k * (p2.z - p1.z) + p1.z;
        Vector3 pn = new Vector3(pnx, pny, pnz);
        return pn;
    }

    public static Vector3 UnitVector(Vector3 from, Vector3 to) 
    {
        Vector3 uv = (to - from) / Vector3.Distance(from, to);
        return uv;
    }
}

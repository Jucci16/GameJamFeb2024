using System;
using UnityEngine;

public static class TransformUtils
{
    public static float GetYRotFromVec(Vector2 v1, Vector2 v2)
    {
        float _r = Mathf.Atan2(v1.x - v2.x, v1.y - v2.y);
        float _d = (_r / Mathf.PI) * 180;
        if(_d < 0) _d = 360 - Math.Abs(_d);
     
        return _d;
    }
}

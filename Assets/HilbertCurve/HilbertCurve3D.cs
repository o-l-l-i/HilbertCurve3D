using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


// 3D Hilbert curve implementation by Olli S.
// This is not a full-scale Hilbert system that would scale up to different dimensions.

public class HilbertCurve3D : MonoBehaviour
{

    Material mat;

    // Order (powers of 2)
    const int M = 16;
    const int M3 = M * M * M;

    Vector3[] points;

    int drawCounter;


    void Start()
    {
        points = GeneratePositions();
        mat = new Material(Shader.Find("Unlit/LineColor"));
    }


    Vector3[] GeneratePositions()
    {
        Vector3[] points = new Vector3[M3];

        for (int i = 0; i < M3; i++)
        {
            points[i] = HilbertPoint3D(i);
        }

        return points;
    }


    int GrayCode(int x)
    {
        return x ^ (x >> 1);
    }


    int Last3bits(int x)
    {
        return (x & 7);
    }


    public int GetBit(int deci, int bitAt)
    {
        int constant = 1 << (bitAt);
        return (deci & constant) > 0 ? 1 : 0;
    }


    Vector3 HilbertPoint3D(int hindex)
    {
        int gc = GrayCode(Last3bits(hindex));

        int x = GetBit(gc, 0);
        int y = GetBit(gc, 1);
        int z = GetBit(gc, 2);

        hindex = (hindex >> 3);

        Vector3 pos = new Vector3(x, y, z);


        for (int n = 4; n <= M; n *= 2)
        {
            int n2 = n / 2;

            Vector3 tPos = pos;

            switch (Last3bits(hindex))
            {
                case 0:
                    pos.x =  tPos.y;
                    pos.y =  tPos.z;
                    pos.z =  tPos.x;
                break;
                case 1:
                    pos.x =  tPos.z;
                    pos.y =  tPos.x + n2;
                    pos.z =  tPos.y;
                break;
                case 2:
                    pos.x =  tPos.z + n2;
                    pos.y =  tPos.x + n2;
                    pos.z =  tPos.y;
                break;
                case 3:
                    pos.x = -tPos.x + n - 1;
                    pos.y = -tPos.y + n2 - 1;
                    pos.z =  tPos.z;
                break;
                case 4:
                    pos.x = -tPos.x + n - 1;
                    pos.y = -tPos.y + n2 - 1;
                    pos.z =  tPos.z + n2;
                break;
                case 5:
                    pos.x = -tPos.z + n - 1;
                    pos.y =  tPos.x + n2;
                    pos.z = -tPos.y + n - 1;
                break;
                case 6:
                    pos.x = -tPos.z + n2 - 1;
                    pos.y =  tPos.x + n2;
                    pos.z = -tPos.y + n - 1;
                break;
                case 7:
                    pos.x =  tPos.y;
                    pos.y = -tPos.z + n2 - 1;
                    pos.z = -tPos.x + n - 1;
                break;
            }

            hindex = (hindex >> 3);
        }

        pos = pos / 2.0f + new Vector3(0.25f, 0.25f, 0.25f);

        float md = (float)M / 4f;
        pos -= new Vector3(md, md, md);

        return pos;
    }


    # region render
    void OnRenderObject()
    {
        if (drawCounter >= points.Length-1)
            drawCounter = 0;

        Camera cam = Camera.current;

        if (cam == null)
            return;

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadIdentity();
        GL.MultMatrix(cam.worldToCameraMatrix);

        GL.Begin(GL.LINES);

        Vector3 currentPoint = Vector3.zero;
        Vector3 previousPoint = Vector3.zero;

        float angle = Time.time * 20f;
        Vector3 pivot = Vector3.zero;

        for(int i = 0; i < drawCounter; i++)
        {
            currentPoint = points[i];
            currentPoint = RotatePointAroundPivot(currentPoint, pivot, new Vector3(angle, angle, 0f));

            if (i == 0)
                previousPoint = currentPoint;

            Color col = Color.HSVToRGB((float)i / (M3), 1, 1);

            GL.Color(col);

            GL.Vertex(previousPoint);
            GL.Vertex(currentPoint);

            previousPoint = currentPoint;
        }

        GL.End();
        GL.PopMatrix();

        drawCounter++;
    }


    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }
    #endregion

}
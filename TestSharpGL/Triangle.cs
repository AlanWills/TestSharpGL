using SharpGL;
using System;

namespace TestSharpGL
{
    public class Vector3
    {
        public double X, Y, Z;

        public Vector3(double x, double y, double z) { X = x; Y = y; Z = z; }
    }

    public class Triangle
    {
        public Vector3 N, V1, V2, V3;

        public Triangle(byte[] pointData)
        {
            float x, y, z;

            x = BitConverter.ToSingle(pointData, 0);
            y = BitConverter.ToSingle(pointData, 4);
            z = BitConverter.ToSingle(pointData, 8);

            N = new Vector3(x, y, z);

            x = BitConverter.ToSingle(pointData, 12);
            y = BitConverter.ToSingle(pointData, 16);
            z = BitConverter.ToSingle(pointData, 20);

            V1 = new Vector3(x, y, z);

            x = BitConverter.ToSingle(pointData, 24);
            y = BitConverter.ToSingle(pointData, 28);
            z = BitConverter.ToSingle(pointData, 32);

            V2 = new Vector3(x, y, z);

            x = BitConverter.ToSingle(pointData, 36);
            y = BitConverter.ToSingle(pointData, 40);
            z = BitConverter.ToSingle(pointData, 44);

            V3 = new Vector3(x, y, z);
        }

        public void Draw(OpenGL gl)
        {
            gl.Vertex(V1.X, V1.Y, V1.Z);
            gl.Vertex(V2.X, V2.Y, V2.Z);
            gl.Vertex(V3.X, V3.Y, V3.Z);
        }
    }
}

using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TestSharpGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool rotating = false;
        Point mousePosDown;
        Vector3 lightPosition, cameraPosition;
        int numberOfTriangles;
        float cameraRotation = 0, modelRotation = 0;
        OpenGL gl;
        uint theProgram, vertexAttributeObject, vertexBufferObject;
        string strVertexShader = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Light.vert");
        string strFragmentShader = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Light.frag");

        public MainWindow()
        {
            InitializeComponent();

            MouseWheel += MainWindow_MouseWheel;
            MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseRightButtonUp += MainWindow_MouseRightButtonUp;
            lightPosition = new Vector3(0, 0, -1);
            cameraPosition = new Vector3(0, 0, 0);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            cameraPosition.Z += 1 * Math.Sign(e.Delta);
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rotating = true;
            mousePosDown = e.MouseDevice.GetPosition(this);
        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (rotating)
            {
                cameraRotation += (float)(e.MouseDevice.GetPosition(this).X - mousePosDown.X) * 0.001f;
                mousePosDown = e.MouseDevice.GetPosition(this);
            }
        }

        private void MainWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            rotating = false;
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //  Enable the OpenGL depth testing functionality.
            gl = args.OpenGL;
            gl.Enable(OpenGL.GL_DEPTH_TEST);

            float[] vertexPositions;

#if false
            using (FileStream model = new FileStream(@"C:\Users\alan\Downloads\Helix_pentagon.STL", FileMode.Open))
            {
                model.Seek(80, SeekOrigin.Begin);

                byte[] numTrianglesInBytes = new byte[4];
                model.Read(numTrianglesInBytes, 0, 4);

                numberOfTriangles = BitConverter.ToInt32(numTrianglesInBytes, 0);
                vertexPositions = new float[3 * 6 * numberOfTriangles];

                byte[] pointData = new byte[50];

                for (int i = 0; i < numberOfTriangles; ++i)
                {
                    model.Read(pointData, 0, 50);

                    // Normal
                    float x, y, z, nx, ny, nz;
                    nx = BitConverter.ToSingle(pointData, 0);
                    ny = BitConverter.ToSingle(pointData, 4);
                    nz = BitConverter.ToSingle(pointData, 8);

                    for (int point = 1; point < 4; ++point)
                    {
                        x = BitConverter.ToSingle(pointData, point * 12);
                        y = BitConverter.ToSingle(pointData, point * 12 + 4);
                        z = BitConverter.ToSingle(pointData, point * 12 + 8);

                        vertexPositions[3 * i + 6 * (point - 1)] = x;
                        vertexPositions[3 * i + 6 * (point - 1) + 1] = y;
                        vertexPositions[3 * i + 6 * (point - 1) + 2] = z;
                        vertexPositions[3 * i + 6 * (point - 1) + 3] = nx;
                        vertexPositions[3 * i + 6 * (point - 1) + 4] = ny;
                        vertexPositions[3 * i + 6 * (point - 1) + 5] = nz;
                    }
                }
            }
#else
            vertexPositions = new float[] {
                -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
                 0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
                 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
                 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

                -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
                 0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
                 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
                 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

                -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
                -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
                -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

                 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

                -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

                -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
                 0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
                 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
                 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
                -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
                -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
            };

            numberOfTriangles = vertexPositions.Length / 6;
#endif

            InitializeVertexBuffer(vertexPositions);
            InitializeProgram();
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            modelRotation += 0.01f;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Reset the modelview matrix.
            gl.LoadIdentity();

            gl.UseProgram(theProgram);
            
            int projLoc = gl.GetUniformLocation(theProgram, "projection");
            int viewLoc = gl.GetUniformLocation(theProgram, "view");
            int modelLoc = gl.GetUniformLocation(theProgram, "model");
            int lightPosLoc = gl.GetUniformLocation(theProgram, "lightPos");
            gl.Uniform3(lightPosLoc, lightPosition.X, lightPosition.Y, lightPosition.Z);

            Matrix4x4 projectionMat = Matrix4x4.CreatePerspectiveFieldOfView(0.5f * (float)(Math.PI), (float)gl.RenderContextProvider.Width / gl.RenderContextProvider.Height, 0.1f, 100);
            projectionMat = Matrix4x4.Identity;
            float[] projectionFloats = projectionMat.ToFloatArray();

            // Orthographic
            float[] projectionFloats2 = new float[16]
            {
                0.1f / 400f, 0, 0, 0,
                0, 0.1f / 400f, 0, 0,
                0, 0, -100.01f / 99.99f, -20f / 99.99f,
                0, 0, -1, 0,
            };

            Matrix4x4 viewMat = Matrix4x4.CreateRotationY(cameraRotation);
            //viewMat = Matrix4x4.Identity;
            float[] viewFloats = viewMat.ToFloatArray();
            
            Matrix4x4 modelMat = Matrix4x4.CreateRotationY(modelRotation);
            //modelMat = Matrix4x4.Identity;
            float[] modelFloats = modelMat.ToFloatArray();

            gl.UniformMatrix4(projLoc, 1, true, projectionFloats);
            gl.UniformMatrix4(viewLoc, 1, true, viewFloats);
            gl.UniformMatrix4(modelLoc, 1, true, modelFloats);

            gl.BindVertexArray(vertexAttributeObject);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, numberOfTriangles * 3);
            gl.BindVertexArray(0);

            gl.UseProgram(0);

            gl.Flush();
        }

        private void OpenGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            gl.Viewport(0, 0, gl.RenderContextProvider.Width, gl.RenderContextProvider.Height);

            // Load and clear the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            // Perform a perspective transformation
            gl.Perspective(45.0f, (float)gl.RenderContextProvider.Width / gl.RenderContextProvider.Height, 0.1f, 100.0f);

            // Load the modelview.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private uint CreateProgram(List<uint> shaderList)
        {
            uint program = gl.CreateProgram();

            foreach (uint shader in shaderList)
            {
                gl.AttachShader(program, shader);
            }

            gl.LinkProgram(program);

            ProgramErrorInfo(program);

            foreach (uint shader in shaderList)
                gl.DetachShader(program, shader);

            return program;
        }

        private uint CreateShader(uint eShaderType, string strShaderFile)
        {
            uint shader = gl.CreateShader(eShaderType);
            string strFileData = File.ReadAllText(strShaderFile);

            gl.ShaderSource(shader, strFileData);

            gl.CompileShader(shader);

            ShaderErrorInfo(shader);

            return shader;
        }

        private void InitializeProgram()
        {
            List<uint> shaderList = new List<uint>();

            shaderList.Add(CreateShader(OpenGL.GL_VERTEX_SHADER, strVertexShader));
            shaderList.Add(CreateShader(OpenGL.GL_FRAGMENT_SHADER, strFragmentShader));

            theProgram = CreateProgram(shaderList);

            foreach (uint shader in shaderList)
            {
                gl.DeleteShader(shader);
            }
        }

        private void InitializeVertexBuffer(float[] vertexPositions)
        {
            uint[] vao = new uint[1];
            gl.GenVertexArrays(1, vao);
            vertexAttributeObject = vao[0];

            uint[] vbo = new uint[1];
            gl.GenBuffers(1, vbo);
            vertexBufferObject = vbo[0];

            GCHandle handle = GCHandle.Alloc(vertexPositions, GCHandleType.Pinned);
            IntPtr vertexPtr = handle.AddrOfPinnedObject();
            var size = Marshal.SizeOf(typeof(float)) * vertexPositions.Length;

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vertexBufferObject);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, vertexPtr, OpenGL.GL_STATIC_DRAW);

            gl.BindVertexArray(vertexAttributeObject);

            // Position attribute
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 6 * sizeof(float), IntPtr.Zero);
            gl.EnableVertexAttribArray(0);

            // Normal attribute
            IntPtr offset = new IntPtr(3 * sizeof(float));
            gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false, 6 * sizeof(float), offset);
            gl.EnableVertexAttribArray(1);

            // Color attribute
            //offset = new IntPtr(6 * sizeof(float));
            //gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), offset);
            //gl.EnableVertexAttribArray(2);

            gl.BindVertexArray(0);
            handle.Free();
        }
        
        private bool ShaderErrorInfo(uint shaderId)
        {
            StringBuilder builder = new StringBuilder(2048);
            gl.GetShaderInfoLog(shaderId, 2048, IntPtr.Zero, builder);
            string res = builder.ToString();

            if (!res.Equals(""))
            {
                Debug.Fail(res);

                return false;
            }

            return true;
        }

        private bool ProgramErrorInfo(uint programId)
        {
            StringBuilder builder = new StringBuilder(2048);
            gl.GetProgramInfoLog(programId, 2048, IntPtr.Zero, builder);
            string res = builder.ToString();

            if (!res.Equals(""))
            {
                Debug.Fail(res);
                return false;
            }

            return true;
        }
    }

    public static class Extensions
    {
        public static float[] ToFloatArray(this Matrix4x4 matrix)
        {
            float[] floats = new float[16];

            floats[0] = matrix.M11;
            floats[1] = matrix.M12;
            floats[2] = matrix.M13;
            floats[3] = matrix.M14;
            floats[4] = matrix.M21;
            floats[5] = matrix.M22;
            floats[6] = matrix.M23;
            floats[7] = matrix.M24;
            floats[8] = matrix.M31;
            floats[9] = matrix.M32;
            floats[10] = matrix.M33;
            floats[11] = matrix.M34;
            floats[12] = matrix.M41;
            floats[13] = matrix.M42;
            floats[14] = matrix.M43;
            floats[15] = matrix.M44;

            return floats;
        }
    }
}

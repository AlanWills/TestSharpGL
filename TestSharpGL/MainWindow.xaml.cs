using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        List<Triangle> triangles = new List<Triangle>();

        Vector3 lightPosition;
        float rotation = 0;
        OpenGL gl;
        uint theProgram, vertexAttributeObject, vertexBufferObject;
        string strVertexShader = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Light.vert");
        string strFragmentShader = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Light.frag");

        public MainWindow()
        {
            InitializeComponent();

            MouseWheel += MainWindow_MouseWheel;
            lightPosition = new Vector3(0, 0, -1);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //cameraPosition.Z += 0.1f * e.Delta;
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //  Enable the OpenGL depth testing functionality.
            gl = args.OpenGL;
            gl.Enable(OpenGL.GL_DEPTH_TEST);

            InitializeVertexBuffer();
            InitializeProgram();

            using (FileStream model = new FileStream(@"C:\Users\alan\Downloads\Helix_pentagon.STL", FileMode.Open))
            {
                model.Seek(80, SeekOrigin.Begin);

                byte[] numTrianglesInBytes = new byte[4];
                model.Read(numTrianglesInBytes, 0, 4);

                int number = BitConverter.ToInt32(numTrianglesInBytes, 0);

                triangles = new List<Triangle>(number);

                byte[] pointData = new byte[50];

                for (int i = 0; i < number; i++)
                {
                    model.Read(pointData, 0, 50);
                    
                    triangles.Add(new Triangle(pointData));
                }
            }
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            rotation += 0.01f;

#if false
            //Clear the color and depth buffers.
            gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.DrawText(5, 100, 1, 1, 1, "Courier New", 12, "Hello");

            // Reset the modelview matrix.
            gl.LoadIdentity();

            // Move the geometry into a fairly central position.
            gl.Translate(-90, -80, -100);

            // Start drawing triangles.
            gl.Begin(OpenGL.GL_TRIANGLES);

            gl.Color(1.0f, 1.0f, 1.0f);
            foreach (Triangle triangle in triangles)
            {
                triangle.Draw(gl);
            }
            
            gl.End();

            // Flush OpenGL.
            gl.Flush();

#else
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Reset the modelview matrix.
            gl.LoadIdentity();

            gl.UseProgram(theProgram);

            //glm::mat4 view;
            //view = glm::lookAt(cameraPos, cameraPos + cameraFront, cameraUp);
            //// Projection 
            //glm::mat4 projection;
            //projection = glm::perspective(fov, (GLfloat)WIDTH / (GLfloat)HEIGHT, 0.1f, 100.0f);
            //// Get the uniform locations
            //GLint modelLoc = glGetUniformLocation(ourShader.Program, "model");
            //GLint viewLoc = glGetUniformLocation(ourShader.Program, "view");
            //GLint projLoc = glGetUniformLocation(ourShader.Program, "projection");
            //// Pass the matrices to the shader
            //glUniformMatrix4fv(viewLoc, 1, GL_FALSE, glm::value_ptr(view));
            //glUniformMatrix4fv(projLoc, 1, GL_FALSE, glm::value_ptr(projection));

            int projLoc = gl.GetUniformLocation(theProgram, "projection");
            int viewLoc = gl.GetUniformLocation(theProgram, "view");
            int modelLoc = gl.GetUniformLocation(theProgram, "model");
            int lightPosLoc = gl.GetUniformLocation(theProgram, "lightPos");
            gl.Uniform3(lightPosLoc, (float)lightPosition.X, (float)lightPosition.Y, (float)lightPosition.Z);

            float[] projectionMat = new float[16]
            {
                -(float)gl.RenderContextProvider.Height / gl.RenderContextProvider.Width, 0, 0, 0,
                0, (float)(1 / Math.Tan(45.0f)), 0, 0,
                0, 0, (100.0f/99.99f), 0,
                0, 0, -(10/99.99f), 1
            };

            float[] viewMat = new float[16]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
            };

            //float[] viewMat = new float[16]
            //{
            //    1, 0, 0, 0,
            //    0, (float)Math.Cos(rotation), -(float)Math.Sin(rotation), 0,
            //    0, (float)Math.Sin(rotation), (float)Math.Cos(rotation), 0,
            //    0, 0, 0, 1,
            //};

            float[] modelMat = new float[16]
            {
                (float)Math.Cos(rotation), 0, -(float)Math.Sin(rotation), 0,
                0, 1, 0, 0,
                (float)Math.Sin(rotation), 0, (float)Math.Cos(rotation), 0,
                0, 0, 0, 1,
            };

            gl.UniformMatrix4(projLoc, 1, false, projectionMat);
            gl.UniformMatrix4(viewLoc, 1, false, viewMat);
            gl.UniformMatrix4(modelLoc, 1, false, modelMat);

            gl.BindVertexArray(vertexAttributeObject);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 36);
            gl.BindVertexArray(0);

            gl.UseProgram(0);

            gl.Flush();
#endif
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

        private void InitializeVertexBuffer()
        {
            float[] vertexPositions = {
                -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, 0.5f, 0.5f, 0.5f,

                -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, 0.5f, 0.5f, 0.5f,

                -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,

                 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, 0.5f, 0.5f, 0.5f,

                -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, 0.5f, 0.5f, 0.5f,

                -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
                -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, 0.5f, 0.5f, 0.5f,
            };
            
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
            gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), IntPtr.Zero);
            gl.EnableVertexAttribArray(0);

            // Normal attribute
            IntPtr offset = new IntPtr(3 * sizeof(float));
            gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), offset);
            gl.EnableVertexAttribArray(1);

            // Color attribute
            offset = new IntPtr(6 * sizeof(float));
            gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), offset);
            gl.EnableVertexAttribArray(2);


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
}

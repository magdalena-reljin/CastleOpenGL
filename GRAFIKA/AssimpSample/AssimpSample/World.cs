// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi
        private enum Textures { Grass = 0, Metal = 1, Mud = 2 };
        private uint[] m_textures = null;
        private string[] m_textureImages = { "..//..//Textures//grass.jpg", "..//..//Textures//metal.jpg", "..//..//Textures//mud.jpg" };
        private float faktorSkaliranjaStrele=1;
        private int m_textureCount = Enum.GetNames(typeof(Textures)).Length;
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private DispatcherTimer timer3;
        private DispatcherTimer timer4;
        private DispatcherTimer timer5;
        private DispatcherTimer timer6;
        private DispatcherTimer timer7;
        private DispatcherTimer timer8;
        private DispatcherTimer timer9;
        private int zoomOutCounter1 = 0;
        private int zoomOutCounter2 = 0;
        private float eyeZ = 80.0f;
        private float eyeY = 0.0f;
        private float eyeX = 0.0f;

        private float centerZ = 0.0f;
        private float centerY = 0.0f;
        private float centerX = 0.0f;

        private float upZ = 0.0f;
        private float upY = 1.0f;
        private float upX = 0.0f;
        private float doorRotationAngle = 0f;
        private float doorTranslationY = -700f;
        private float doorTranslationZ = 0f;
        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene_Castle;
        private AssimpScene m_scene_Arrow;
        private AssimpScene m_scene_Door;
        private AssimpScene m_scene_Wall;
        private float arrowTranslate = 100.0f;
        private int arrowCounter = 0;


        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;
        public OpenGL gl;
        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene_Castle; }
            set { m_scene_Castle = value; }
        }
        public AssimpScene SceneDoor
        {
            get { return m_scene_Door; }
            set { m_scene_Door = value; }
        }
        public AssimpScene SceneWall
        {
            get { return m_scene_Wall; }
            set { m_scene_Wall = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { if (value > 0 && value < 90) m_xRotation = value; else if (ActiveAnimation) m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }
        public bool ActiveAnimation { get; set; }
        public float FaktorSkaliranjaStrele
        {
            get { return faktorSkaliranjaStrele; }
            set { faktorSkaliranjaStrele = value; }
        }
        public float LeftWallTranslationInput { get; set; }
        public float RightWallRotationInput { get; set; }
        public float EyeX
        {
            get { return eyeX; }
            set { eyeX = value; }
        }

        /// <summary>
        ///     Y pozicija kamere.
        /// </summary>
        public float EyeY
        {
            get { return eyeY; }
            set { eyeY = value; }
        }

        /// <summary>
        ///     Z pozicija kamere.
        /// </summary>
        public float EyeZ
        {
            get { return eyeZ; }
            set {  eyeZ = value; }
        }

        public float CenterZ
        {
            get { return centerZ; }
            set { centerZ = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene_Castle = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_scene_Arrow = new AssimpScene("3D Models\\Arrow", "Arrow.obj", gl);
            this.m_scene_Door = new AssimpScene("3D Models\\ornate_tomb_door", "scene.obj", gl);
            this.m_scene_Wall = new AssimpScene("3D Models\\large_medieval_wall_-_modular", "scene.obj", gl);
            this.m_width = width;
            this.m_height = height;
            this.gl = gl;
            m_textures = new uint[m_textureCount];
            
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 1f, 1f);

            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            SetLighting();
            DefineAnimationTimers();
            SetTexture();

            m_scene_Castle.LoadScene();
            m_scene_Castle.Initialize();
            m_scene_Arrow.LoadScene();
            m_scene_Arrow.Initialize();
            m_scene_Door.LoadScene();
            m_scene_Door.Initialize();
            m_scene_Wall.LoadScene();
            m_scene_Wall.Initialize();
        }

        private void SetLighting()
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            LightingAboveTheCastle();
            TopLeftLighting();
        }
        private void LightingAboveTheCastle()
        {
            float[] ambient = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] difuse = new float[] { 0.7f, 0.7f, 0.7f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 45.0f);
            gl.Enable(OpenGL.GL_LIGHT1);
            float[] positions = { -50.0f, 1400.0f, -2000.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, positions);
            

        }
        private void TopLeftLighting()
        {
            float[] position = new float[] { -10.0f, 100.0f, 7000, 1.0f };
            float[] ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] difuse = new float[] { 0.91f, 0.91f, 0.07f, 1.0f }; 

            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, position);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); 
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1f, 0f, 0f);
            gl.Rotate(m_yRotation, 0f, 1f, 0f);
            gl.LookAt(eyeX, eyeY, eyeZ, centerX, centerY, centerZ, upX, upY, upZ);
           
            DrawCastle();
            DrawFrontWallLeft();
            DrawFrontWallRight();
            DrawCastleWallRight();
            DrawCastleWallLeft();
            DrawCastleBackWall();



            DrawDoor();
            DrawGround();
            DrawArrowsRight();
            DrawArrowsLeft();
          
            DrawRoad();
            DrawLeftWall();
            DrawRightWall();
            DrawText();
            gl.PopMatrix();
            gl.Flush();
        }

        private void DrawRightWall()
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)Textures.Metal]);
            gl.PushMatrix();
            gl.Translate(1900f, -800f, -1000f);
            gl.Rotate(RightWallRotationInput, 0f, 1f, 0f);
            gl.Scale(15.0f, 200.0f, 1800.0f);
            Cube rightWall = new Cube();
            rightWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        private void DrawLeftWall()
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)Textures.Metal]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.Scale(0.5f, 1f, 0.5f);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Translate(-1900f+LeftWallTranslationInput, -800f, -1000f);
            gl.Rotate(0, 0f, 1f, 0f);
            gl.Scale(15.0f, 200.0f, 1800.0f);
            Cube leftWall = new Cube();
            leftWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        private void DrawGround()
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)Textures.Grass]); 
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.Scale(3.0f, 2.0f, 2.0f);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PushMatrix();
            gl.Translate(0f, 1000f, 7000f);
            gl.Scale(15.0f, 20.0f, 10.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.TexCoord(0.0f, 10.0f);
            gl.Vertex(250f, -100f, -400f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(250f, -100f, -1200f);
            gl.TexCoord(10.0f, 0.0f);
            gl.Vertex(-250f, -100f, -1200f);
            gl.TexCoord(10.0f, 10.0f);
            gl.Vertex(-250f, -100f, -400f);
            gl.End();

            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        private void DrawRoad()
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)Textures.Mud]);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.Scale(100.0f, 100.0f, 100.0f);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Translate(0f, 900f, 7000f);
            gl.Scale(3.2f, 19.0f, 10.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.TexCoord(0.0f, 10.0f);
            gl.Vertex(120f, -100, -400);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(120f, -100, -700);
            gl.TexCoord(10.0f, 0.0f);
            gl.Vertex(-120f, -100, -700);
            gl.TexCoord(10.0f, 10.0f);
            gl.Vertex(-120f, -100, -400);
            gl.End();
            gl.PopMatrix();

            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawCastle()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.PushMatrix();
            gl.Translate(17.0f, -1000.0f, -1400.0f);
            gl.Scale(20f, 30f, 15f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Castle.Draw();
            gl.PopMatrix();

            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawFrontWallLeft()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(-710.0f, -1000.0f, 0.0f);
            gl.Scale(100f, 50f, 10f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Wall.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawFrontWallRight()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(700.0f, -1000.0f, -10.0f);
            gl.Rotate(180, 0f, 1f, 0f);
            gl.Scale(100f, 50f, 10f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Wall.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawCastleWallRight()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(1200.0f, -1000.0f, -1200.0f);
            gl.Rotate(90, 0f, 1f, 0f);
            gl.Scale(270f, 50f, 10f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Wall.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawCastleWallLeft()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(-1200.0f, -1000.0f, -1200.0f);
            gl.Rotate(-90, 0f, 1f, 0f);
            gl.Scale(270f, 50f, 10f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Wall.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        public void DrawCastleBackWall()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(0.0f, -1000.0f, -2400.0f);
            gl.Rotate(-180, 0f, 1f, 0f);
            gl.Scale(270f, 50f, 10f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Wall.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }


        public void DrawDoor()
        {

            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PushMatrix();
            gl.Translate(0.0f, doorTranslationY, doorTranslationZ);
            gl.Rotate(doorRotationAngle, 1f, 0f, 0f);
            gl.Scale(400.0f, 300.0f, 400.0f);
            gl.Color(0.658824f, 0.658824f, 0.658824f, 1f);
            m_scene_Door.Draw();
            gl.PopMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        private void DrawArrowsLeft()
        {
            int faktorPocetni = 100;
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Color(1, 1, 1);
            for (float i = 0; i < 12; i++)
            {
                gl.PushMatrix();
                
                gl.Translate(-2300f + 70*i, -350f, arrowTranslate);
                gl.Rotate(-90, 0.0f, 1.0f, 0.0f);
                gl.Scale(faktorPocetni, faktorPocetni, faktorPocetni);
                gl.Translate(0.0f, 0.0f, -12.0f);
                gl.Scale(FaktorSkaliranjaStrele, FaktorSkaliranjaStrele, FaktorSkaliranjaStrele);
                m_scene_Arrow.Draw();
                gl.PopMatrix();
            }
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        private void DrawArrowsRight()
        {
            int faktorPocetni = 100;
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Color(1, 1, 1);
            for (float i = 0; i < 12; i++)
            {
                gl.PushMatrix();

                gl.Translate(-880 + 70 * i, -350f, arrowTranslate);
                gl.Rotate(-90, 0.0f, 1.0f, 0.0f);
                gl.Scale(faktorPocetni, faktorPocetni, faktorPocetni);
                gl.Translate(0.0f, 0.0f, -12.0f);
                gl.Scale(FaktorSkaliranjaStrele, FaktorSkaliranjaStrele, FaktorSkaliranjaStrele);
                m_scene_Arrow.Draw();
                gl.PopMatrix();
            }
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        public void DrawText()
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.PushMatrix();
            gl.Viewport(m_width * 3 / 4, 0, m_width / 4, m_height / 2);
            gl.DrawText(20, 200, 0.0f, 1.0f, 1.0f, "Courier New", 14, " ");
            gl.DrawText(20, 240, 0.0f, 1.0f, 1.0f, "Arial", 14, "Predmet: Racunarska grafika");
            gl.DrawText(20, 238, 0.0f, 1.0f, 1.0f, "Arial", 14, "_______________________");
            gl.DrawText(20, 200, 0.0f, 1.0f, 1.0f, "Arial", 14, "Sk.god: 2021/22.");
            gl.DrawText(20, 198, 0.0f, 1.0f, 1.0f, "Arial", 14, "______________");
            gl.DrawText(20, 160, 0.0f, 1.0f, 1.0f, "Arial", 14, "Ime: Magdalena");
            gl.DrawText(20, 158, 0.0f, 1.0f, 1.0f, "Arial", 14, "_____________");
            gl.DrawText(20, 120, 0.0f, 1.0f, 1.0f, "Arial", 14, "Prezime: Reljin");
            gl.DrawText(20, 118, 0.0f, 1.0f, 1.0f, "Arial", 14, "____________");
            gl.DrawText(20, 80, 0.0f, 1.0f, 1.0f, "Arial", 14, "Sifra zad: 3.1");
            gl.DrawText(20, 78, 0.0f, 1.0f, 1.0f, "Arial", 14, "___________");
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
        }

        /// <summary>
        /// Teksture
        /// </summary>


        public void SetTexture()
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL); //način stapanja teksture sa materijalom GL_DECAL
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                Bitmap image = new Bitmap(m_textureImages[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR); //filteri za teksture
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR); //linearno filtriranje
                //wrapping GL_REPEAT po obema osama
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                // Posto je kreirana tekstura slika nam vise ne treba
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }
        #region Animacija
        /// <summary>
        /// Metode za animaciju
        /// </summary>

        public void ActivateAnimation()
        {
            ActiveAnimation = true;
            timer1.Start();
        }

        private void DefineAnimationTimers()
        {
            timer1 = new DispatcherTimer();
            timer2 = new DispatcherTimer();
            timer3 = new DispatcherTimer();
            timer4 = new DispatcherTimer();
            timer5 = new DispatcherTimer();
            timer6 = new DispatcherTimer();
            timer7 = new DispatcherTimer();
            timer8 = new DispatcherTimer();
            timer9 = new DispatcherTimer();

            timer1.Interval = TimeSpan.FromMilliseconds(20);
            timer1.Tick += new EventHandler(SetupForAnimation);


            timer2.Interval = TimeSpan.FromMilliseconds(30);
            timer2.Tick += new EventHandler(ZoomScene1);

            
            timer3.Interval = TimeSpan.FromMilliseconds(20);
            timer3.Tick += new EventHandler(OpenTheDoor);
            
            
            timer4.Interval = TimeSpan.FromMilliseconds(5);
            timer4.Tick += new EventHandler(ComeIn);
            
            timer5.Interval = TimeSpan.FromMilliseconds(5);
            timer5.Tick += new EventHandler(TurnCameraAround);
            
            timer6.Interval = TimeSpan.FromMilliseconds(5);
            timer6.Tick += new EventHandler(TurnCameraAroundAgain);

            timer7.Interval = TimeSpan.FromMilliseconds(5);
            timer7.Tick += new EventHandler(ZoomOut);

            timer8.Interval = TimeSpan.FromMilliseconds(5);
            timer8.Tick += new EventHandler(GoUp);

            timer9.Interval = TimeSpan.FromMilliseconds(2);
            timer9.Tick += new EventHandler(Arrows);

        }
        private void SetupForAnimation(object sender, EventArgs e)
        {
            m_sceneDistance -= 3000.0f;
            RotationX -= 20.0f;
            centerX = 0f;
            centerY = -100f;
            centerZ = -700f;
            upZ = -1800f;
            timer1.Stop();
            timer2.Start();
        }
        private void ZoomScene1(object sender, EventArgs e)
        {
            Console.WriteLine("usao u zooom    " +m_scene_Castle);
            Console.WriteLine(m_sceneDistance);
            if (m_sceneDistance > 2100)
            {
                m_sceneDistance -= 100.0f;
            }
            else{
                timer2.Stop();
                timer3.Start();
            }
        }
        private void OpenTheDoor(object sender, EventArgs e)
        {
            Console.WriteLine("usao u open   " + m_scene_Castle);
            Console.WriteLine(m_sceneDistance);
            if (doorRotationAngle > -90)
            {
                doorRotationAngle -= 10;
                doorTranslationZ -= 27;
                doorTranslationY -= 30;
            }
            else
            {
                timer3.Stop();
                timer4.Start();
            }
        }
        private void ComeIn(object sender, EventArgs e)
        {
            Console.WriteLine("koor   " + RotationX);
            Console.WriteLine(m_sceneDistance);
           
            if (RotationX < 30)
            {
                RotationX += 2;
                SceneDistance -= 130;
            }
            else
            {
                timer4.Stop();
                timer5.Start();
               
            }
        }
        private void TurnCameraAround(object sender, EventArgs e)
        {
            for (int i=0; i<3; i++)
            {
                RotationX -= 5.0f;
            }
               timer5.Stop();
               timer6.Start();
        }
        private void TurnCameraAroundAgain(object sender, EventArgs e)
        {
            RotationY = -180.0f;
            RotationX = 30.0f;
            m_sceneDistance += 2600.0f;
            timer6.Stop();
            timer7.Start();
        }
        private void ZoomOut(object sender, EventArgs e)
        {
            zoomOutCounter1++;
            if (zoomOutCounter1 < 3)
            {
                SceneDistance += 200.0f;
            }
            else
            {
                timer7.Stop();
                timer8.Start();
            }
        }
        private void GoUp(object sender, EventArgs e)
        {
            zoomOutCounter2++;
            if (zoomOutCounter2 < 15)
            {
                RotationX += 5.0f;
            }
            else
            {
                timer8.Stop();
                timer9.Start();
            }
         }
        private void Arrows(object sender, EventArgs e)
        {
            arrowCounter++;
            if (arrowCounter < 40)
                arrowTranslate += 30;
            else
            {
                timer9.Stop();
                ActiveAnimation = false;
            }
        }
        public void ResetBeforeAnimation()
        {
               zoomOutCounter1 = 0;
               zoomOutCounter2 = 0;
               eyeZ = 80.0f;
               eyeY = 0.0f;
               eyeX = 0.0f;
               centerZ = 0.0f;
               centerY = 0.0f;
               centerX = 0.0f;
               upZ = 0.0f;
               upY = 1.0f;
               upX = 0.0f;
               doorRotationAngle = 0f;
               doorTranslationY = -700f;
               doorTranslationZ = 0f;
               arrowTranslate = 100.0f;
               arrowCounter = 0;
               m_xRotation = 0.0f;
               m_yRotation = 0.0f;
               m_sceneDistance = 7000.0f;
        }
        #endregion Animacija

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);    
            gl.LoadIdentity();
            gl.Perspective(50f, (double)width / height, 1.0f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();              
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene_Castle.Dispose();
                m_scene_Arrow.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}

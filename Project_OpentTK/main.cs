﻿using OpenTK.Mathematics;
using OpenTKBase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SDLBase
{
    
    public static class OpenTKProgram
    {
        private static string useShader = "toon";
        public static void Main()
        {
            OpenTKApp app = new OpenTKApp(1280, 720, "Forest", true);

            app.Initialize();
            app.LockMouse(true);

            ExecuteApp_Forest(app);

            app.Shutdown();
        }

        static GameObject CreateGround(float size)
        {
            Mesh mesh = GeometryFactory.AddPlane(size, size, 128, true);
            mesh.ComputeNormalsAndTangentSpace();

            Texture grassTexture = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Linear, true);
            grassTexture.Load("Textures/grass_basecolor.png");
            Texture grassNormal = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Linear, true);
            grassNormal.Load("Textures/grass_normal.png");

            Material material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", Color4.White);
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", new Vector2(2.0f, 128.0f));
            material.Set("BaseColor", grassTexture);
            material.Set("NormalMap", grassNormal);

            GameObject go = new GameObject();
            go.transform.position = new Vector3(0, 0, 0);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = material;

            return go;
        }

        static float Range(this Random rnd, float a, float b)
        {
            return rnd.NextSingle() * (b - a) + a;
        }

        static void CreateRandomTree(Random rnd, float forestSize)
        {
            float s = forestSize * 0.5f;

            // Trunk
            float heightTrunk = rnd.Range(0.5f, 1.5f);
            float widthTrunk = rnd.Range(0.7f, 1.25f);

            Mesh mesh = GeometryFactory.AddCylinder(widthTrunk, heightTrunk, 8, true);

            Material material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", new Color4(rnd.Range(0.6f, 0.9f), rnd.Range(0.4f, 0.6f), rnd.Range(0.15f, 0.35f), 1.0f));
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);

            GameObject mainObject = new GameObject();
            mainObject.transform.position = new Vector3(rnd.Range(-s, s), 0, rnd.Range(-s, s));
            MeshFilter mf = mainObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = mainObject.AddComponent<MeshRenderer>();
            mr.material = material;

            // Leaves
            float radiusLeaves = rnd.Range(widthTrunk * 1.5f, widthTrunk * 3.0f);

            mesh = GeometryFactory.AddSphere(radiusLeaves, 16, false, true);

            material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", new Color4(rnd.Range(0.0f, 0.2f), rnd.Range(0.6f, 0.8f), rnd.Range(0.0f, 0.2f), 1.0f));
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);

            GameObject leaveObj = new GameObject();
            leaveObj.transform.position = mainObject.transform.position + Vector3.UnitY * (heightTrunk / 2 + radiusLeaves);
            leaveObj.transform.SetParent(mainObject.transform);
            mf = leaveObj.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            mr = leaveObj.AddComponent<MeshRenderer>();
            mr.material = material;
        }

        static void CreateLilGuy(Vector3 pos)
        {
            // Torso
            Mesh mesh = GeometryFactory.AddCylinder(0.8f, 1.0f, 8, true);

            Material material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", new Color4(0.0f, 1.0f, 1.0f, 1.0f));
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);

            GameObject mainObject = new GameObject();
            mainObject.transform.position = pos + new Vector3(0.0f, 1.0f, 0.0f);
            MeshFilter mf = mainObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = mainObject.AddComponent<MeshRenderer>();
            mr.material = material;

            // Legs
            mesh = GeometryFactory.AddCylinder(0.3f, 1.0f, 16, true);

            material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", new Color4(0.0f, 0.0f, 1.0f, 1.0f));
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);

            GameObject leftLegObj = new GameObject();
            leftLegObj.transform.position = mainObject.transform.position + new Vector3(-0.5f, -1.0f, 0.0f);
            leftLegObj.transform.SetParent(mainObject.transform);
            mf = leftLegObj.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            mr = leftLegObj.AddComponent<MeshRenderer>();
            mr.material = material;

            GameObject rightLegObj = new GameObject();
            rightLegObj.transform.position = mainObject.transform.position + new Vector3(0.5f, -1.0f, 0.0f);
            rightLegObj.transform.SetParent(mainObject.transform);
            mf = rightLegObj.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            mr = rightLegObj.AddComponent<MeshRenderer>();
            mr.material = material;

            // Head
            mesh = GeometryFactory.AddSphere(0.6f, 32, false, true);

            material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", new Color4(0.8f, 0.0f, 1.0f, 1.0f));
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", new Vector2(2.0f, 128.0f));

            GameObject headObj = new GameObject();
            headObj.transform.position = mainObject.transform.position + new Vector3(0.0f, 1.6f, 0.0f);
            headObj.transform.SetParent(mainObject.transform);
            mf = headObj.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            mr = headObj.AddComponent<MeshRenderer>();
            mr.material = material;

            // Head Outline
            // mesh = GeometryFactory.AddSphere(0.6f, 32, false, true);

            // material = new Material(Shader.Find($"Shaders/outline"));
            // material.Set("Color", new Color4(0.0f, 0.0f, 1.0f, 1.0f));
            // material.Set("ColorEmissive", Color4.Black);
            // material.Set("Specular", Vector2.UnitY);

            // GameObject headOutObj = new GameObject();
            // headOutObj.transform.position = mainObject.transform.position + new Vector3(0.0f, 1.6f, 0.0f);
            // headOutObj.transform.SetParent(mainObject.transform);
            // mf = headOutObj.AddComponent<MeshFilter>();
            // mf.mesh = mesh;
            // mr = headOutObj.AddComponent<MeshRenderer>();
            // mr.material = material;
        }

        static void SetupEnvironment()
        {
            var cubeMap = new Texture();
            cubeMap.LoadCube("Textures/cube_*.jpg");

            var env = OpenTKApp.APP.mainScene.environment;

            env.Set("Color", new Color4(0.2f, 0.2f, 0.2f, 1.0f));
            env.Set("ColorTop", new Color4(0.0f, 1.0f, 1.0f, 1.0f));
            env.Set("ColorMid", new Color4(1.0f, 1.0f, 1.0f, 1.0f));
            env.Set("ColorBottom", new Color4(0.0f, 0.25f, 0.0f, 1.0f));
            env.Set("FogDensity", 0.000001f);
            env.Set("FogColor", Color.DarkCyan);
            env.Set("CubeMap", cubeMap);
        }

        static GameObject SetupLights()
        {
            // Setup directional light turned 30 degrees down
            GameObject go = new GameObject();
            go.transform.position = new Vector3(0.0f, 20.0f, 20.0f);
            go.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitX, -MathF.PI * 0.16f);
            Light light = go.AddComponent<Light>();
            light.type = Light.Type.Spot;
            light.lightColor = Color.White;
            light.intensity = 2.0f;
            light.range = 200;
            light.cone = new Vector2(0.0f, MathF.PI / 2.0f);

            return go;
        }

        static (GameObject, Material) CreateSphere()
        {
            Mesh mesh = GeometryFactory.AddSphere(0.5f, 32, true);

            Material material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", Color4.White);
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);

            GameObject go = new GameObject();
            go.transform.position = new Vector3(0, 2, -5);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = material;

            return (go, material);
        }

        static void CreateTexturedPlane(string textureName, Vector3 pos)
        {
            string texturePath = Path.Combine("Textures",textureName);

            Mesh mesh = GeometryFactory.AddPlane(100,100,16,true);
            mesh.ComputeNormalsAndTangentSpace();

            Texture texture = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Linear, true);
            texture.Load($"{texturePath}_basecolor.png");
            Texture textureNormal = new Texture(OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureMinFilter.Linear, true);
            textureNormal.Load($"{texturePath}_normal.png");

            Material material = new Material(Shader.Find($"Shaders/{useShader}"));
            material.Set("Color", Color4.White);
            material.Set("ColorEmissive", Color4.Black);
            material.Set("Specular", Vector2.UnitY);
            material.Set("BaseColor", texture);
            material.Set("NormalMap", textureNormal);

            GameObject go = new GameObject();
            go.transform.position = pos;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = material;
        }

        static void CreateSkysphere(float radius)
        {
            Mesh mesh = GeometryFactory.AddSphere(radius, 64, true);

            Material material = new Material(Shader.Find("Shaders/skysphere_envmap"));

            GameObject go = new GameObject();
            go.transform.position = new Vector3(0, 0, 0);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = material;
        }

        static GameObject CreateForest(GameObject light)
        {
            float forestSize = 120.0f;

            // Create skysphere
            CreateSkysphere(forestSize * 4.0f);

            // Create ground
            var ret = CreateGround(forestSize);

            // Create a sphere in the middle of the forest
            var (reflectSphere, reflectMaterial) = CreateSphere();
            var (glowSphere, glowMaterial) = CreateSphere();
            glowSphere.transform.position = light.transform.position;
            glowMaterial.Set("Color", Color4.Black);
            glowMaterial.Set("ColorEmissive", Color4.Yellow);

            // Create trees
            Random rnd = new Random(1);
            for (int i = 0; i < 50; i++)
            {
                CreateRandomTree(rnd, forestSize);
            }

            // Create Lil Guy
            CreateLilGuy(new Vector3(-2f,0f,2f));

            return ret;
        }

        static void ExecuteApp_Forest(OpenTKApp app)
        {
            SetupEnvironment();

            var light = SetupLights();

            var ground = CreateForest(light);

            // Create camera
            GameObject cameraObject = new GameObject();
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0.0f, 2.0f, 0.0f);
            camera.ortographic = false;
            FirstPersonController fps = cameraObject.AddComponent<FirstPersonController>();

            // Create pipeline
            RPS renderPipeline = new RPS();

            app.Run(() =>
            {
                app.Render(renderPipeline);
            });
        }
    }
}

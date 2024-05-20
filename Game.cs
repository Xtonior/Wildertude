using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using XGE3D;
using XGE3D.Common;
using XGE3D.Core;
using XGE3D.Core.ComponentSystem.Components;
using XGE3D.Core.SceneSystem;
using XGE3D.Physics.Shapes;
using XGE3D.Tools;

namespace Wildertude
{
    internal class Game : GameScript
    {
        private bool _firstMove = true;
        private Vector2 _lastPos;
        private Camera camComp;
        private float deltaTime;

        private Entity ground;
        private Entity box;
        private Entity bullet;
        private Entity player;
        private PlayerMovement playerMovement;

        private float yRot;

        public override void Start()
        {
            Entity camera = new Entity("camera", new OpenTK.Mathematics.Vector3(0, 5, 2));
            camComp = new Camera(new OpenTK.Mathematics.Vector3(0, 0, 3), GameEngine.GetWindow().ClientSize.X / (float)GameEngine.GetWindow().ClientSize.Y);
            camera.AddComponent(camComp);
            SceneAssembler.AddObject(Program.scene, camera);
            GameEngine.renderEngine.SetRenderCamera(camComp);

            //Entity model = new Entity("thing", new Vector3(0, -5, 0));
            //XTLogger.Warn(model.transform.position.ToString());
            //Model bp = new Model(@"Resources\survival-guitar-backpack\bp.fbx", @"Resources\survival-guitar-backpack\diffuse.jpg");
            //MeshRenderer mesh = new MeshRenderer(bp, RenderEngine.CurrentRenderer.GetCurrentShader(), camComp);
            //model.AddComponent(mesh);

            //SceneAssembler.AddObject(Program.scene, model);

            Model md_ground = new Model(@"Resources\Models\test\1.fbx", @"Resources\default\default.png");
            ground = new Entity("ground", new Vector3(0f, -2f, 0f), new Vector3(15f, 0.5f, 100f));
            ground.Triplanar = 1f;
            MeshRenderer groundMesh = new MeshRenderer(md_ground, RenderEngine.CurrentRenderer.GetCurrentShader(), camComp);
            BodyRigid immovable = new BodyRigid(new BoxShape(7.5f, 0.25f, 50f).shape, 0.0f, true);
            ground.AddComponent(groundMesh);
            ground.AddComponent(immovable);

            box = new Entity("box", new Vector3(0f, 2f, 0f));
            Model md_box = new Model(@"Resources\Models\cube.fbx", @"Resources\default\default.png");
            MeshRenderer boxMesh = new MeshRenderer(md_box, RenderEngine.CurrentRenderer.GetCurrentShader(), camComp);
            BodyRigid boxRigid = new BodyRigid(new BoxShape(0.5f).shape, 1.0f, false);
            box.AddComponent(boxMesh);
            box.AddComponent(boxRigid);
            
            bullet = new Entity("bullet", camComp.Position);
            Model md_bullet = new Model(@"Resources\Models\sphere.fbx", @"Resources\container2.png");
            MeshRenderer bulletMesh = new MeshRenderer(md_bullet, RenderEngine.CurrentRenderer.GetCurrentShader(), camComp);
            BodyRigid bulletRigid = new BodyRigid(new SphereShape(0.5f).shape, 0.3f, false);
            bullet.AddComponent(bulletMesh);
            bullet.AddComponent(bulletRigid);

            player = new Entity("pm", new Vector3(2f, 0f, 0f), new Vector3(1f, 2f, 1f));
            playerMovement = new PlayerMovement();
            player.AddComponent(playerMovement);

            SceneAssembler.AddObject(Program.scene, ground);
            SceneAssembler.AddObject(Program.scene, box);
            SceneAssembler.AddObject(Program.scene, bullet);
            SceneAssembler.AddObject(Program.scene, player);
        }

        public override void Update(FrameEventArgs args)
        {
            deltaTime = GameEngine.GetWindow().deltaTime;
            CameraInput();
            camComp.Position = player.transform.Position + new Vector3(0f, 0.5f, 0f);
        }

        private void CameraInput()
        {
            var input = GameEngine.GetWindow().KeyboardState;
            var mouse = GameEngine.GetWindow().MouseState;

            //const float cameraSpeed = 2.5f;
            const float sensitivity = 0.01f;

            Vector2 moveInput = new Vector2(0, 0);

            if (input.IsKeyPressed(Keys.R))
            {
                box.GetComponent<BodyRigid>().SetLinearVelocity(Vector3.Zero);
                box.GetComponent<BodyRigid>().SetAngularVelocity(Vector3.Zero);
                box.GetComponent<BodyRigid>().SetPosition(new Vector3(0f, 2f, 0f));
            }

            if (input.IsKeyPressed(Keys.Q))
            {
                bullet.GetComponent<BodyRigid>().SetLinearVelocity(Vector3.Zero);
                bullet.GetComponent<BodyRigid>().SetAngularVelocity(Vector3.Zero);
                bullet.GetComponent<BodyRigid>().SetPosition(camComp.Position);
                bullet.GetComponent<BodyRigid>().ApplyImpulse(camComp.Front * 10f);
            }

            if (input.IsKeyDown(Keys.W))
            {
                //playerMovement.Move(new Vector2(-1f, 0f), camComp.Front, camComp.Right);
                moveInput += new Vector2(0f, 1f);
                //camComp.Position -= camComp.Front * cameraSpeed * (float)deltaTime; // Backwards
                //playerController.Move(new Vector2(0f, -1f), cameraComponent.Front, cameraComponent.Right);
            }
            if (input.IsKeyDown(Keys.A))
            {
                //playerMovement.Move(new Vector2(0f, -1f), camComp.Front, camComp.Right);
                moveInput += new Vector2(1f, 0f);
                //camComp.Position -= camComp.Right * cameraSpeed * (float)deltaTime; // Left
                //playerController.Move(new Vector2(-1f, 0f), cameraComponent.Front, cameraComponent.Right);
            }
            if (input.IsKeyDown(Keys.S))
            {
                //playerMovement.Move(new Vector2(1f, 0f), camComp.Front, camComp.Right);
                moveInput += new Vector2(0f, -1f);
                //camComp.Position += camComp.Front * cameraSpeed * (float)deltaTime; // Forward
                //playerController.Move(new Vector2(0f, 1f), cameraComponent.Front, cameraComponent.Right);
            }
            if (input.IsKeyDown(Keys.D))
            {
                //playerMovement.Move(new Vector2(0f, 1f), camComp.Front, camComp.Right);
                moveInput += new Vector2(-1f, 0f);
                //camComp.Position += camComp.Right * cameraSpeed * (float)deltaTime; // Right
                //playerController.Move(new Vector2(1f, 0f), cameraComponent.Front, cameraComponent.Right);
            }
            if (input.IsKeyPressed(Keys.Space))
            {
                playerMovement.Jump();
                //camComp.Position += Vector3.UnitY * cameraSpeed * (float)deltaTime; // Up

            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                //camComp.Position -= Vector3.UnitY * cameraSpeed * (float)deltaTime; // Down
            }

            Vector2.Clamp(moveInput, -Vector2.One, Vector2.One);
            playerMovement.Move(moveInput);

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                yRot += deltaX * sensitivity;
                player.transform.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, yRot));
                camComp.Yaw = MathHelper.RadiansToDegrees(player.transform.Rotation.ToEulerAngles().X);

                camComp.Pitch -= MathHelper.RadiansToDegrees(deltaY * sensitivity);
            }
        }
    }
}

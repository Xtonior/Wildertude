using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using XGE3D.Core.ComponentSystem.Components;
using XGE3D.Core.ComponentSystem;
using OpenTK.Mathematics;
using XGE3D.Physics.Shapes;
using XGE3D.Core;
using XGE3D.Tools;
using XGE3D.BulletSharpPhysics;
using System.Reflection.Metadata;
using BulletSharp.SoftBody;
using System.Security.Cryptography;

namespace Wildertude
{
    internal class PlayerMovement : Component
    {
        private BodyRigid bodyRigid = new BodyRigid(new BoxShape(0.5f, 1f, 0.5f).shape, 1.0f, false);
        private MeshRenderer mesh;

        private float maxSpeed = 200000f;
        private float walkSpeed = 1200f;
        private float groundAccel = 2500f;
        private float airAccel = 0f;
        private float stopSpeed = 0.001f;

        private float friction= 100f;

        private float jumpForce = 4f;

        private Vector3 moveDir;

        private bool isGrounded = false;
        private bool canJump = true;

        private float dt;

        public override void Init()
        {
            base.Init();

            //Model md = new Model(@"Resources\Models\cube.fbx", @"Resources\default\default.png");
            //mesh = new MeshRenderer(md, RenderEngine.CurrentRenderer.GetCurrentShader(), RenderEngine.MainCamera);
            
            bodyRigid.AngularInertia = new Vector3(0f, 0f, 0f);
            bodyRigid.Friction = 0f;
            ParentEntity.AddComponent(bodyRigid);
            //ParentEntity.AddComponent(mesh);
        }

        public void Move(Vector2 input)
        {
            moveDir = ParentEntity.transform.Forward * input.X + ParentEntity.transform.Right * input.Y;
            moveDir = Vector3.Clamp(moveDir, -Vector3.One, Vector3.One);

            moveDir.Y = 0f;

            Vector3 vel = bodyRigid.LinearVelocity;

            if (isGrounded)
            {
                //vel = (CalcFriction(vel).X, vel.Y, CalcFriction(vel).Z);
                vel += MoveGround(moveDir, vel, walkSpeed);
                bodyRigid.SetLinearVelocity(vel);
                XTLogger.Warn(moveDir.ToString());
            }
            else
            {
                bodyRigid.AddLinearVelocity(MoveAir(moveDir, vel, walkSpeed));
            }
        }

        public void Jump()
        {
            if (!isGrounded) 
                return;

            if (!canJump) 
                return;

            bodyRigid.ApplyImpulse(Vector3.UnitY * jumpForce);
            canJump = false;
        }

        public override void Update(float deltaTime)
        {
            dt = deltaTime;
            bodyRigid = ParentEntity.GetComponent<BodyRigid>();

            isGrounded = Physics.RayCast(bodyRigid.Position + Vector3.UnitY, bodyRigid.Position + -Vector3.UnitY * 1f);
            if (isGrounded && !canJump && bodyRigid.LinearVelocity.Y <= 0.1f) canJump = true;
        }

        public override XmlSchema? GetSchema()
        {
            throw new NotImplementedException();
        }

        public override void WriteXml(XmlWriter writer)
        {
            
        }

        public override void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        private float CalcFriction(Vector3 prevVelocity)
        {
            Vector3 v = prevVelocity;
            v.Y = 0f;

            // Calculate speed
            float speed = v.Length;

            // If too slow, return
            if (speed < 0.1f)
            {
                return 0f; 
            }

            // Bleed off some speed, but if we have less than the bleed
            // threshhold, bleed the theshold amount.
            float control = (speed < stopSpeed) ? stopSpeed : speed;

            // Add the amount to t'he drop amount.
            float drop = control * friction * dt;
            return drop;

            // scale the velocity
            /*float newspeed = speed - drop;

            if (newspeed < 0)
                newspeed = 0;

            // Determine proportion of old speed we are using.
            if (speed != 0f)
                newspeed /= speed;

            return v * newspeed;*/
        }

        private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float targetSpeed)
        {
            Vector3 v = prevVelocity;
            v.Y = 0f;

            float currentspeed = Vector3.Dot(v, accelDir);

            // Reduce wishspeed by the amount of veer.
            float addspeed = (accelDir.Length - currentspeed) * targetSpeed;

            // If not going to add any speed, done.
            if (addspeed <= 0)
                return Vector3.Zero;

            // Determine amount of accleration.
            float accelspeed = accelerate * dt * accelDir.Length - CalcFriction(v);

            // Cap at addspeed
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            return accelspeed * accelDir;
        }

        private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity, float targetSpeed)
        {
            // ground_accelerate and max_velocity_ground are server-defined movement variables
            return Accelerate(accelDir, prevVelocity, groundAccel, targetSpeed);
        }

        private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity, float targetSpeed)
        {
            // air_accelerate and max_velocity_air are server-defined movement variables
            return Accelerate(accelDir, prevVelocity, airAccel, targetSpeed);
        }
    }
}

namespace MultiplayerEngine
{
    public class LaserComponent : Component
    {
        public Vector3 DirectionVector { get; set; }

        // properties for laser look
        public Color Color { get; set; }
        public float Thickness { get; set; } = 1.0f;
        public float Length { get; set; } = 1;
        public float StopLength { get; set; } = 10;
        public ParticleSystem linePS, startPS, stopPS;
        //OldParticleSystem particleSystem;

        Model laserModel;
        LaserPool pool;
        int stage = 0;

        public void Setup(LaserPool laserPool)
        {
            laserModel = GameWorld.Instance.Content.Load<Model>("Laser");
            pool = laserPool;
            //particleSystem = new OldParticleSystem();
            //particleSystem.ParticleModel = GameWorld.Instance.Content.Load<Model>("Particle");
        }

        public void FireLaser(Vector3 dir, float stopLength)
        {
            StopLength = stopLength;
            //DirectionVector = new Vector3(
            //(float)(Math.Cos(MathHelper.ToRadians(transform.Rotation.Z + 180))),
            //(float)(Math.Cos(MathHelper.ToRadians(transform.Rotation.Y - 90)) * Math.Sin(MathHelper.ToRadians(transform.Rotation.Z + 180))),
            //(float)(Math.Sin(MathHelper.ToRadians(transform.Rotation.Y + 90)) * Math.Sin(MathHelper.ToRadians(transform.Rotation.Z + 180)))
            //);

            //creates a matrix with the given rotations and get a direction vector with buildin matrix function
            Matrix rotation =
                Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.Y)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.X)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z + 90));
            Matrix scale = Matrix.CreateScale(Thickness, Length, Thickness);
            Matrix world = SceneManager.active_scene.worldMatrix * scale * rotation * Matrix.CreateTranslation(transform.Position3D);
            DirectionVector = world.Forward;

            // Trin 1: Sæt en tilfældig farve til laseren
            Random rand = new Random();
            Color = new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));

            // Generer partikler langs laserruten i en vifte
            //int numParticles = 10;
            //float maxAngle = 30;  // Maksimal vinkel i grader for viften
            //for (int i = 0; i < numParticles; i++)
            //{
            //    float t = (float)i / (numParticles - 1);
            //    Vector3 particlePos = Vector3.Lerp(transform.Position3D, transform.Position3D + DirectionVector * StopLength, t);
            //    Vector3 baseVelocity = new Vector3(0, Globals.Rnd.Next(0, 1), 0);  // Basis hastighed for updrift

            //    particleSystem.GenerateParticles(particlePos, baseVelocity, 1, maxAngle);
            //}
            ((ParticleSystem.LineSpawn)linePS.spawnInfo).direction = DirectionVector;
            ((ParticleSystem.LineSpawn)linePS.spawnInfo).length = Length;
        }
    

        public void Draw3D()
        {
            

            if(laserModel == null)
            {
                return;
            }
            // Trin 2: Beregn rotationen, så cylinderen peger i retning af skudet
            //Vector3 direction = Vector3.Normalize(DirectionVector);
            //float pitch = (float)Math.Asin(direction.Y);
            //float yaw = (float)Math.Atan2(direction.X, direction.Z);
            //Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);
            Matrix rotation =
                Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.Y - 90)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.X)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z + 90));
            // Trin 3: Skaler cylinderen baseret på dens tykkelse og længde
            Matrix scale = Matrix.CreateScale(Thickness, Length, Thickness);

            // Trin 4: Beregn den samlede transformationsmatrix
            Matrix world = SceneManager.active_scene.worldMatrix * scale * rotation * Matrix.CreateTranslation(transform.Position3D);
            // Trin 5: Tegn cylinderen
            foreach(ModelMesh mesh in laserModel.Meshes)
            {
                foreach(BasicEffect meshEffect in mesh.Effects)
                {
                    meshEffect.View = SceneManager.active_scene.viewMatrix;
                    meshEffect.World = world;
                    meshEffect.Projection = SceneManager.active_scene.projectionMatrix;
                    meshEffect.DiffuseColor = Color.ToVector3(); // Anvend farven
                }
                mesh.Draw();
            }

            //particleSystem.Draw(SceneManager.active_scene.viewMatrix, SceneManager.active_scene.projectionMatrix);
        }

        // sættes i en update
        public void RetractLaser(float retractSpeed, float deltaTime)
        {
            // Trin 1: Beregn hvor meget laseren skal trækkes sammen
            float retractAmount = retractSpeed * deltaTime;

            // Trin 2: Opdater laseren længde og position
            if(stage == 0)
            {
                Length += retractAmount;  // Træk laseren sammen
                if(Length > StopLength)
                {
                    stage = 1;
                }
            }
            else if(stage == 1)
            {
                Length -= retractAmount;  // Træk laseren sammen
                transform.Position3D += Vector3.Normalize(DirectionVector) * retractAmount;  // Flyt laseren tilbage
                // Trin 3: Sørg for, at Length ikke bliver negativ
                if(Length < 0)
                {
                    pool.ReturnLaser(gameObject);
                    Length = 1;
                }
            }
        }

        //eksempel
        public void Update()
        {
            // Træk laseren sammen
            RetractLaser(50.0f, Globals.DeltaTime);


            //particle update
            if(linePS != null && linePS.spawnInfo.GetType() == typeof(ParticleSystem.LineSpawn))
                ((ParticleSystem.LineSpawn)linePS.spawnInfo).length = Length;
            //if (particleSystem != null)
            //    particleSystem.Update(Globals.DeltaTime);
        }

    }
}

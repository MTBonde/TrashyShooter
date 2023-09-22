using System.Numerics;

namespace GameServer
{
    /// <summary>
    /// the core for a collider used for collision detection in the world
    /// Niels
    /// </summary>
    public abstract class Collider : ICloneable
    {
        // Colliderens position i verden
        public Vector3 position;

        //public Collider()
        //{
        //    // Tilføjer denne collider til den globale liste over colliders
        //    CollisionManager.colliders.Add(this);
        //}

        //public Collider(bool addToColliders)
        //{
        //    // Tilføjer denne collider til den globale liste over colliders
        //    if (addToColliders)
        //        CollisionManager.colliders.Add(this);
        //}

        // Abstrakte metoder for kollisionskontrol
        public abstract CollisionInfo CheckPointCollision(Vector3 startPoint, Vector3 endPoint);
        public abstract CollisionInfo CheckCylinderCollision(Vector3 startPoint, Vector3 endPoint, float radius, float height);
        public abstract object Clone();
    }

    public class CollisionInfo
    {
        public Collider? collider;
        public Vector3 collisionPoint;
        public byte? playerID;
    }

    public class BoxCollider : Collider
    {
        public byte? playerID;
        public Vector3 size, offset;
        Vector3 cord1, cord2;

        public BoxCollider()
        {
            // Tilføjer denne collider til den globale liste over colliders
            CollisionManager.colliders.Add(this);
        }

        public BoxCollider(bool addToColliders)
        {
            // Tilføjer denne collider til den globale liste over colliders
            if (addToColliders)
                CollisionManager.colliders.Add(this);
        }

        public override CollisionInfo CheckPointCollision(Vector3 startPoint, Vector3 endPoint)
        {
            CollisionInfo col = new CollisionInfo();
            col.collisionPoint = endPoint;
            cord1 = -(size / 2) + offset + position;
            cord2 = (size / 2) + offset + position;
            if (endPoint.X > cord1.X && endPoint.X < cord2.X)
                if (endPoint.Y > cord1.Y && endPoint.Y < cord2.Y)
                {
                    col.playerID = playerID;
                    col.collider = this;
                    if (startPoint.X > cord1.X && startPoint.X < cord2.X)
                    {
                        if (startPoint.Y < cord1.Y)
                            col.collisionPoint = new Vector3(endPoint.X, cord1.Y, endPoint.Z);
                        else
                            col.collisionPoint = new Vector3(endPoint.X, cord2.Y, endPoint.Z);
                    }
                    else
                    {
                        if (startPoint.X < cord1.X)
                            col.collisionPoint = new Vector3(cord1.X, endPoint.Y, endPoint.Z);
                        else
                            col.collisionPoint = new Vector3(cord2.X, endPoint.Y, endPoint.Z);
                    }
                }
            return col;
        }

        public override CollisionInfo CheckCylinderCollision(Vector3 startPoint, Vector3 endPoint, float radius, float height)
        {
            CollisionInfo col = new CollisionInfo();
            col.collisionPoint = endPoint;
            cord1 = -(size / 2) + offset + position;
            cord2 = (size / 2) + offset + position;
            if(endPoint.Z > cord1.Z &&  endPoint.Z < cord2.Z + height)
                if (endPoint.X > cord1.X - radius && endPoint.X < cord2.X + radius)
                    if (endPoint.Y > cord1.Y - radius && endPoint.Y < cord2.Y + radius)
                    {
                        col.playerID = playerID;
                        col.collider = this;
                        if (startPoint.X > cord1.X - radius && startPoint.X < cord2.X + radius)
                        {
                            if (startPoint.Y < position.Y - radius)
                                col.collisionPoint = new Vector3(endPoint.X, cord1.Y - radius, endPoint.Z);
                            else
                                col.collisionPoint = new Vector3(endPoint.X, cord2.Y + radius, endPoint.Z);
                        }
                        else
                        {
                            if (startPoint.X < position.X - radius)
                                col.collisionPoint = new Vector3(cord1.X - radius, endPoint.Y, endPoint.Z);
                            else
                                col.collisionPoint = new Vector3(cord2.X + radius, endPoint.Y, endPoint.Z);
                        }
                    }
            return col;
        }

        public override object Clone()
        {
            return new BoxCollider(false) { cord1 = Vector3.Zero, cord2 = Vector3.Zero, offset = this.offset, size = this.size, playerID = this.playerID, position = this.position };
        }
    }
}

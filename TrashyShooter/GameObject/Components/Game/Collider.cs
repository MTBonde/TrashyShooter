using Microsoft.Xna.Framework;

namespace MultiplayerEngine
{
    /// <summary>
    /// the core for a collider used for collision detection in the world
    /// Niels
    /// </summary>
    public abstract class Collider : Component
    {



        public Collider()
        {
            CollisionManager.colliders.Add(this);
        }

        public abstract CollisionInfo CheckPointCollision(Vector3 startPoint, Vector3 endPoint);
        public abstract CollisionInfo CheckCylinderCollision(Vector3 startPoint, Vector3 endPoint, GameObject go, float radius, float height);
    }

    public class CollisionInfo
    {
        public Collider collider;
        public Vector3 collisionPoint;
    }

    public class BoxCollider : Collider
    {

        public Vector3 size, offset;
        Vector3 cord1, cord2;

        public override CollisionInfo CheckPointCollision(Vector3 startPoint, Vector3 endPoint)
        {
            CollisionInfo col = new CollisionInfo();
            col.collider = this;
            col.collisionPoint = endPoint;
            cord1 = -(size / 2) + offset + transform.Position3D;
            cord2 = (size / 2) + offset + transform.Position3D;
            if (endPoint.X > cord1.X && endPoint.X < cord2.X)
                if (endPoint.Y > cord1.Y && endPoint.Y < cord2.Y)
                {
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

        public override CollisionInfo CheckCylinderCollision(Vector3 startPoint, Vector3 endPoint, GameObject go, float radius, float height)
        {
            CollisionInfo col = new CollisionInfo();
            col.collisionPoint = endPoint;
            cord1 = -(size / 2) + offset + transform.Position3D;
            cord2 = (size / 2) + offset + transform.Position3D;
            if(endPoint.Z > cord1.Z &&  endPoint.Z < cord2.Z + height)
                if (endPoint.X > cord1.X - radius && endPoint.X < cord2.X + radius)
                    if (endPoint.Y > cord1.Y - radius && endPoint.Y < cord2.Y + radius)
                    {
                        col.collider = this;
                        if (startPoint.X > cord1.X - radius && startPoint.X < cord2.X + radius)
                        {
                            if (startPoint.Y < transform.Position.Y - radius)
                                col.collisionPoint = new Vector3(endPoint.X, cord1.Y - radius, endPoint.Z);
                            else
                                col.collisionPoint = new Vector3(endPoint.X, cord2.Y + radius, endPoint.Z);
                        }
                        else
                        {
                            if (startPoint.X < transform.Position.X - radius)
                                col.collisionPoint = new Vector3(cord1.X - radius, endPoint.Y, endPoint.Z);
                            else
                                col.collisionPoint = new Vector3(cord2.X + radius, endPoint.Y, endPoint.Z);
                        }
                        if(go != null)
                            gameObject.OnCollision(go);
                    }
            return col;
        }
    }

    //public class MazeCollider : Collider
    //{

    //    MazeCell[,] _cells;
    //    public float wallThickness = 0.2f;

    //    public void SetMaze(MazeCell[,] maze)
    //    {
    //        _cells = maze;
    //    }

    //    public override CollisionInfo CheckCylinderCollision(Vector3 startPoint, Vector3 endPoint, GameObject go, float radius, float height)
    //    {
    //        CollisionInfo col = new CollisionInfo();
    //        col.collider = this;
    //        int currentX = (int)(startPoint.X - transform.Position.X);
    //        int currentY = (int)(startPoint.Y - transform.Position.Y);
    //        Vector3 newEndPoint = endPoint;
    //        if (transform.Position3D.Z + 2 > startPoint.Z && transform.Position3D.Z < startPoint.Z)
    //            if (0 <= currentX && _cells.GetLength(0) > currentX)
    //                if (0 <= currentY && _cells.GetLength(1) > currentY)
    //                {
    //                    if (_cells[currentX, currentY].Walls[1])
    //                    {
    //                        newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX + 1, currentY + 0.5f, 1), startPoint, newEndPoint, radius);
    //                    }
    //                    else if (currentX < _cells.GetLength(0) - 1)
    //                    {
    //                        if (_cells[currentX + 1, currentY].Walls[0])
    //                            newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX + 1.5f, currentY + 1, 1), startPoint, newEndPoint, radius);
    //                        if (currentY > 0)
    //                            if (_cells[currentX + 1, currentY - 1].Walls[0])
    //                                newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX + 1.5f, currentY, 1), startPoint, newEndPoint, radius);
    //                    }
    //                    if (currentX > 0)
    //                    {
    //                        if (_cells[currentX - 1, currentY].Walls[1])
    //                        {
    //                            newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX, currentY + 0.5f, 1), startPoint, newEndPoint, radius);
    //                        }
    //                        else if (currentX > 0)
    //                        {
    //                            if (_cells[currentX - 1, currentY].Walls[0])
    //                                newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX - 0.5f, currentY + 1, 1), startPoint, newEndPoint, radius);
    //                            if (currentY > 0)
    //                                if (_cells[currentX - 1, currentY - 1].Walls[0])
    //                                    newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX - 0.5f, currentY, 1), startPoint, newEndPoint, radius);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX, currentY + 0.5f, 1), startPoint, newEndPoint, radius);
    //                    }


    //                    if (_cells[currentX, currentY].Walls[0])
    //                    {
    //                        newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX + 0.5f, currentY + 1, 1), startPoint, newEndPoint, radius);
    //                    }
    //                    else if (currentY < _cells.GetLength(1) - 1)
    //                    {
    //                        if (_cells[currentX, currentY + 1].Walls[1])
    //                            newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX + 1f, currentY + 1.5f, 1), startPoint, newEndPoint, radius);
    //                        if (currentX > 0)
    //                            if (_cells[currentX - 1, currentY + 1].Walls[1])
    //                                newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX, currentY + 1.5f, 1), startPoint, newEndPoint, radius);
    //                    }
    //                    if (currentY > 0)
    //                    {
    //                        if (_cells[currentX, currentY - 1].Walls[0])
    //                        {
    //                            newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX + 0.5f, currentY, 1), startPoint, newEndPoint, radius);
    //                        }
    //                        else if (currentY > 0)
    //                        {
    //                            if (_cells[currentX, currentY - 1].Walls[1])
    //                                newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX + 1f, currentY - 0.5f, 1), startPoint, newEndPoint, radius);
    //                            if (currentX > 0)
    //                                if (_cells[currentX - 1, currentY - 1].Walls[1])
    //                                    newEndPoint = BoxSolver(new Vector3(wallThickness, 1 + wallThickness, 2), transform.Position3D + new Vector3(currentX, currentY - 0.5f, 1), startPoint, newEndPoint, radius);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (currentX > 0)
    //                            newEndPoint = BoxSolver(new Vector3(1 + wallThickness, wallThickness, 2), transform.Position3D + new Vector3(currentX + 0.5f, currentY, 1), startPoint, newEndPoint, radius);
    //                    }
    //                }
    //        col.collisionPoint = newEndPoint;
    //        return col;
    //    }

    //    Vector3 BoxSolver(Vector3 size, Vector3 pos, Vector3 startPoint, Vector3 endPoint, float radius)
    //    {
    //        Vector3 cord1 = -(size / 2) + pos;
    //        Vector3 cord2 = (size / 2) + pos;
    //        if (endPoint.X > cord1.X - radius && endPoint.X < cord2.X + radius)
    //            if (endPoint.Y > cord1.Y - radius && endPoint.Y < cord2.Y + radius)
    //            {
    //                if (startPoint.X > cord1.X - radius && startPoint.X < cord2.X + radius)
    //                {
    //                    if (startPoint.Y < pos.Y - radius)
    //                        return new Vector3(endPoint.X, cord1.Y - radius, endPoint.Z);
    //                    else
    //                        return new Vector3(endPoint.X, cord2.Y + radius, endPoint.Z);
    //                }
    //                else if (startPoint.Y > cord1.Y - radius && startPoint.Y < cord2.Y + radius)
    //                {
    //                    if (startPoint.X < pos.X - radius)
    //                        return new Vector3(cord1.X - radius, endPoint.Y, endPoint.Z);
    //                    else
    //                        return new Vector3(cord2.X + radius, endPoint.Y, endPoint.Z);
    //                }
    //            }
    //        return endPoint;
    //    }

    //    public override CollisionInfo CheckPointCollision(Vector3 startPoint, Vector3 endPoint)
    //    {
    //        CollisionInfo col = new CollisionInfo();
    //        col.collider = this;
    //        col.collisionPoint = endPoint;
    //        return col;
    //    }
    //}
}

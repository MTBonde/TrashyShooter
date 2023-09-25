using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MultiplayerEngine
{
    /// <summary>
    /// used to check if you are colliding with anything
    /// Niels
    /// </summary>
    public static class CollisionManager
    {

        public static List<Collider> colliders = new List<Collider>();

        public static CollisionInfo CheckCircleCollision(Vector3 start, Vector3 end, GameObject go, float radius, float height)
        {
            CollisionInfo col = new CollisionInfo();
            col.collisionPoint = end;
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i].enabled && colliders[i].gameObject.enabled)
                {
                    CollisionInfo newCol = colliders[i].CheckCylinderCollision(start, col.collisionPoint, go, radius, height);
                    if (end != newCol.collisionPoint)
                        col = newCol;
                }
            }
            return col;
        }

        public static bool RayCast(Vector3 start, Vector3 end)
        {
            Vector3 dir = end - start;
            dir.Normalize();
            dir /= 10;
            Vector3 current = start + dir;
            while (Vector3.Distance(current, end) > 0.2f)
            {
                CollisionInfo colInfo = CheckCircleCollision(start, current, null, 0.1f, 0.1f);
                if (colInfo.collider == null || colInfo.collider.gameObject.name != "Enemy")
                    if (colInfo.collisionPoint != current)
                        return true;
                current += dir;
            }
            return false;
        }
    }
}

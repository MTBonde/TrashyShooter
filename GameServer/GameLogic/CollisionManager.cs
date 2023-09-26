using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    // Håndterer kollisionsdetektion i spillet
    public static class CollisionManager
    {
        /// <summary>
        /// Liste over alle colliders i spillet.
        /// </summary>
        public static List<Collider> colliders = new List<Collider>();

        // Metode til at kontrollere cirkelkollisioner
        public static CollisionInfo CheckCircleCollision(Vector3 start, Vector3 end, float radius, float height, Collider colliderToIgnore, List<Collider> collidersToUse = null)
        {
            if(collidersToUse == null) collidersToUse = colliders;
            // Opretter et nyt CollisionInfo-objekt til at lagre kollisionsinformation
            CollisionInfo col = new CollisionInfo();
            col.collisionPoint = end;

            // Løber igennem alle colliders i spillet
            for(int i = 0; i < collidersToUse.Count; i++)
            {
                // Ignorer den collider, der er angivet til at skulle ignoreres, det er egen collider
                if(collidersToUse[i] != colliderToIgnore)
                {
                    // Tjekker for kollision og returnerer nye kollisionsinformationer
                    CollisionInfo newCol = collidersToUse[i].CheckCylinderCollision(start, col.collisionPoint, radius, height);

                    // Opdaterer kollisionspunktet, hvis der er fundet en kollision
                    if(end != newCol.collisionPoint)
                        col = newCol;
                }
            }
            // Returnerer det opdaterede CollisionInfo-objekt
            return col;
        }

        // Metode til at udføre raycasting fra start til slutpunkt
        public static CollisionInfo RayCast(Vector3 start, Vector3 end, Collider colliderToIgnore, List<Collider> collidersToUse = null)
        {
            // Beregner retningen
            Vector3 dir = end - start;
            dir = Vector3.Normalize(dir);
            dir /= 10;

            // Starter ved begyndelsespunktet og bevæger sig fremad i den beregnede retning
            Vector3 current = start + dir;

            // Debugging: Udskriver start- og slutpunkterne
            //Console.WriteLine("Start point: " + start);
            //Console.WriteLine("End point: " + end);

            // Udfører strålekastningen, indtil vi når slutpunktet
            while(Vector3.Distance(current, end) > 0.2f)
            {
                // Tjekker for kollision ved det aktuelle punkt
                CollisionInfo colInfo = CheckCircleCollision(start, current, 0.01f, 0.01f, colliderToIgnore, collidersToUse);

                // Hvis der er en kollision, og det ikke er ved det aktuelle punkt, returneres sand
                if(colInfo.collider != null)
                    if(colInfo.collisionPoint != current)
                    {
                        // Debugging: Udskriver det punkt, hvor der blev fundet en kollision
                        //Console.WriteLine("Hit something at: " + current);
                        //Retuner den collider vi ramte
                        return colInfo;
                    }

                // Bevæger det aktuelle punkt fremad i den beregnede retning
                current += dir;
            }
            // Returnerer null collider hvis vi ikke ramte
            return new CollisionInfo { collider = null };
        }
    }

}

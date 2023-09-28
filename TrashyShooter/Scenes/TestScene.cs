using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    public class TestScene : Scene
    {



        public override void SetupScene()
        {
            GameWorld.Instance.IsMouseVisible = false;

            //creates worlds center point
            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            int floorCount = 5;
            int floorScale = 5;
            for (int x = 0; x < floorCount; x++)
            {
                for (int y = 0; y < floorCount; y++)
                {
                    GameObject floor = new GameObject();
                    floor.AddComponent<MeshRenderer>().SetModel("floor");
                    floor.GetComponent<MeshRenderer>().scale = 1.0f;
                    Debug.WriteLine(y * floorCount - (floorCount * floorScale / 2));
                    floor.transform.Position3D = new Vector3(x * floorScale - (floorCount * floorScale / 2), y * floorCount - (floorCount * floorScale / 2), 0);
                    floor.transform.Rotation = new Vector3(90, 0, 0);
                }
            }

            //GameObject roof = new GameObject();
            //roof.AddComponent<MeshRenderer>().SetModel("floor");
            //roof.transform.Position3D = new Vector3(0, 0, 2);
            //roof.transform.Rotation = new Vector3(90, 0, 0);

            foreach (Vector3 cubePos in WorldLayout.cubes)
            {
                GameObject cube = new GameObject();
                cube.AddComponent<MeshRenderer>().SetModel("Box");
                cube.transform.Position3D = cubePos;
                cube.transform.Rotation = new Vector3(0, 0, 0);
            }
        }
    }
}

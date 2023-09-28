using Microsoft.Xna.Framework.Input;
using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiplayerEngine
{
    public class MainMenu : Scene
    {

        InputField ipInput;
        InputField nameInput;
        TextRenderer errorText;

        public override void SetupScene()
        {
            AudioManager.StartBackgroundMusic();

            errorText = new GameObject().AddComponent<TextRenderer>();
            errorText.color = Color.Red;
            errorText.scale = 2;
            errorText.transform.Position = new Vector2(Globals.ScreenCenter.X, 200);
            errorText.SetText("");

            GameWorld.Instance.IsMouseVisible = true;
            ipInput = new GameObject().AddComponent<InputField>();
            ipInput.transform.Position = new Vector2(Globals.ScreenCenter.X, Globals.ScreenCenter.Y + 100);
            ipInput.enterSomethingText = "Enter IP";

            nameInput = new GameObject().AddComponent<InputField>();
            nameInput.transform.Position = new Vector2(Globals.ScreenCenter.X, Globals.ScreenCenter.Y + 25);
            nameInput.enterSomethingText = "Enter Name";

            UIButton playButton = new GameObject().AddComponent<UIButton>();
            playButton.OnClick += Play;
            playButton.transform.Position = new Vector2(Globals.ScreenCenter.X, Globals.ScreenCenter.Y + 200);
            TextRenderer playButtonText = playButton.gameObject.AddComponent<TextRenderer>();
            playButtonText.SetText("Play");
            playButtonText.scale = 5;

            UIButton ExitButton = new GameObject().AddComponent<UIButton>();
            ExitButton.OnClick += Exit;
            ExitButton.transform.Position = new Vector2(Globals.ScreenCenter.X, Globals.ScreenCenter.Y + 350);
            TextRenderer ExitButtonText = ExitButton.gameObject.AddComponent<TextRenderer>();
            ExitButtonText.SetText("Exit");
            ExitButtonText.scale = 5;

            //creates worlds center point
            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            GameObject floor = new GameObject();
            floor.AddComponent<MeshRenderer>().SetModel("floor");
            floor.transform.Position3D = new Vector3(0, 0, 0);
            floor.transform.Rotation = new Vector3(90, 0, 0);

            foreach (Vector3 cubePos in WorldLayout.cubes)
            {
                GameObject cube = new GameObject();
                cube.AddComponent<MeshRenderer>().SetModel("Cube");
                cube.transform.Position3D = cubePos;
                cube.transform.Rotation = new Vector3(0, 0, 0);
            }
        }

        public void Play()
        {
            if(nameInput.input == "" || nameInput.input == null)
            {
                errorText.SetText("No name entered");
                return;
            }
            //TODO: make no server on IP error / Enter real IP
            GameWorld.Instance.gameClient.JoinServer(ipInput.input);
            SceneManager.LoadScene(1);
            NetworkManager.SetupRest(ipInput.input);
            GameWorld.Instance.gameClient.SendDataToServer(new ClientHasJoined { playerName = nameInput.input });
        }

        public void Exit()
        {
            Application.Exit();
        }
    }
}

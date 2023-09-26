using Microsoft.Xna.Framework.Input;
using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    public class Player : Component
    {

        float moveScale = 1.0f;
        float mouseSensetivity = 0.1f;
        Vector2 oldMousePos;
        Sender sender;
        Receiver receiver;
        //TextRenderer textRenderer;
        List<(int seqNum, Vector3 pos)> unProcessedInputs = new List<(int, Vector3)>();
        List<(DateTime timestamp, Vector3 position, float rotZ)> positionBuffer = new List<(DateTime, Vector3, float)>();
        LaserPool bulletPool;
        public Hud hud;
        int nextSeqNum = 0;
        ScoreBoard scoreBoard;
        InputField chatField;
        TextRenderer chat;

        bool ready;
        bool chatting = false;

        public void Setup(bool Owner)
        {
            if (Owner)
            {
                sender = gameObject.AddComponent<Sender>();
                sender.snapShotAction += SnapshotUpdate;
                gameObject.transform.Position3D = new Vector3(0f, 0f, 1.6f);
                gameObject.transform.Rotation = new Vector3(0, 120, 90);
                gameObject.AddComponent<Camera>();
                gameObject.GetComponent<Camera>().Awake();
                hud = new GameObject().AddComponent<Hud>();
                hud.Awake();
                sender.HudUpdate += hud.UpdateDisplay;
                bulletPool = gameObject.AddComponent<LaserPool>();
                scoreBoard = gameObject.AddComponent<ScoreBoard>();
                //sender.ScoreUpdate += scoreBoard.UpdateScoreboard;

                scoreBoard.gameObject.Awake();
                //hopper på senders laserShotEvent
                sender.LaserAction += bulletPool.OnLaserShotReceived;

                chatField = new GameObject().AddComponent<InputField>();
                chatField.Awake();
                chatField.enterSomethingText = "Press Enter to chat";
                chatField.transform.Position = new Vector2(200, Globals.ScreenSize.Y - 50);
                chat = new GameObject().AddComponent<TextRenderer>();
                chat.transform.Position = new Vector2(200, Globals.ScreenSize.Y - 100);
                chat.color = Color.White;
                chat.TextPivot = TextRenderer.TextPivots.ButtomCenter;
                chat.text = "";
                sender.ChatUpdate += UpdateChat;
                gameObject.AddComponent<AudioListner>();
            }
            else
            {
                receiver = gameObject.AddComponent<Receiver>();
                receiver.snapShotAction += SnapshotUpdate;
                gameObject.AddComponent<MeshRenderer>().SetModel("ghost_rig");
            }
            //textRenderer = gameObject.AddComponent<TextRenderer>();
            //textRenderer.color = Color.White;
            ready = true;
        }

        public void SnapshotUpdate(NetworkMessage message)
        {
            if (message.MessageType == MessageType.PlayerSnapShot)
            {
                PlayerSnapShot snapshot = (PlayerSnapShot)message;
                HandleSnapShot(snapshot);
                //positionBuffer.Add((DateTime.UtcNow, new Vector3(snapshot.positionX,snapshot.positionY,snapshot.positionZ)));
            }
        }

        KeyboardState lastKeyboardState;
        MouseState lastMouseState;

        public void Update()
        {
            if (ready)
            {
                if (receiver != null)
                {
                    DateTime now = DateTime.UtcNow;
                    DateTime renderTimestamp = now - TimeSpan.FromMilliseconds(1000.0f / GameWorld.Instance.gameClient.ServerUpdateRate);

                    // Drop older positions.
                    while (positionBuffer.Count >= 2 && positionBuffer[1].timestamp <= renderTimestamp)
                    {
                        positionBuffer.RemoveAt(0);
                    }
                    // Interpolate between the two surrounding authoritative positions.
                    if (positionBuffer.Count >= 2
                        && positionBuffer[0].timestamp <= renderTimestamp
                        && renderTimestamp <= positionBuffer[1].timestamp)
                    {
                        Vector3 position0 = positionBuffer[0].position;
                        Vector3 position1 = positionBuffer[1].position;
                        float rotation0 = positionBuffer[0].rotZ;
                        float rotation1 = positionBuffer[1].rotZ;
                        DateTime timestamp0 = positionBuffer[0].timestamp;
                        DateTime timestamp1 = positionBuffer[1].timestamp;
                        //Debug.WriteLine((position0 + (position1 - position0) * (float)(renderTimestamp - timestamp0).TotalMilliseconds / (float)(timestamp1 - timestamp0).TotalMilliseconds));
                        transform.Rotation = new Vector3(0, 0, (rotation0 + (rotation1 - rotation0) * (float)(renderTimestamp - timestamp0).TotalMilliseconds / (float)(timestamp1 - timestamp0).TotalMilliseconds));
                        transform.Position3D = (position0 + (position1 - position0) * (float)(renderTimestamp - timestamp0).TotalMilliseconds / (float)(timestamp1 - timestamp0).TotalMilliseconds);
                    }
                }
                else
                {
                    //rotates player based on mouse movement and resets mouse position to center of screen
                    KeyboardState keyState = Keyboard.GetState();
                    MouseState mouseState = Mouse.GetState();

                    if (chatting)
                    {
                        if (lastKeyboardState.IsKeyDown(Keys.Enter) != keyState.IsKeyDown(Keys.Enter))
                        {
                            if (lastKeyboardState.IsKeyDown(Keys.Enter))
                            {
                                ChatMessage chatMessage = new ChatMessage();
                                chatMessage.Message = chatField.input;
                                chatField.input = "";
                                gameObject.GetComponent<Sender>().SendData(chatMessage);
                                chatting = false;
                                chatField.isWriting = false;
                                GameWorld.Instance.IsMouseVisible = false;
                                //close chat
                            }
                        }
                    }
                    else
                    {
                        Vector2 currentMouse = Mouse.GetState().Position.ToVector2();
                        Vector2 centerOfScreen = new Vector2(GameWorld.Instance.GraphicsDevice.Viewport.Width / 2, GameWorld.Instance.GraphicsDevice.Viewport.Height / 2);
                        Vector2 changeThisFrame = new Vector2(currentMouse.X, currentMouse.Y) - oldMousePos;
                        transform.Rotation -= new Vector3(0, changeThisFrame.Y, changeThisFrame.X) * mouseSensetivity;
                        transform.Rotation = new Vector3(transform.Rotation.X, Math.Clamp(transform.Rotation.Y, 10, 170), transform.Rotation.Z);
                        Mouse.SetPosition((int)centerOfScreen.X, (int)centerOfScreen.Y);
                        oldMousePos = Mouse.GetState().Position.ToVector2();
                        float elapsed = Globals.DeltaTime;
                        Vector3 movement = transform.Position3D;
                        //gets foward vector based on players rotation
                        Vector3 facing = new Vector3(
                        -MathF.Sin((float)(Math.PI / 180) * (transform.Rotation.Z + 90)),
                        MathF.Cos((float)(Math.PI / 180) * (transform.Rotation.Z + 90)),
                        0f);
                        //get the right vector based on players rotation
                        Vector3 sideVector = new Vector3(
                        -MathF.Sin((float)(Math.PI / 180) * (transform.Rotation.Z)),
                        MathF.Cos((float)(Math.PI / 180) * (transform.Rotation.Z)),
                        0f);
                        //moves player based on keyboard inputs
                        if (keyState.IsKeyDown(Keys.W))
                            movement += facing * moveScale * elapsed;
                        if (keyState.IsKeyDown(Keys.S))
                            movement -= facing * moveScale * elapsed;
                        if (keyState.IsKeyDown(Keys.D))
                            movement += sideVector * moveScale * elapsed;
                        if (keyState.IsKeyDown(Keys.A))
                            movement -= sideVector * moveScale * elapsed;
                        transform.Position3D = movement;
                        PlayerUpdate update = new PlayerUpdate();
                        update.up = keyState.IsKeyDown(Keys.W);
                        update.down = keyState.IsKeyDown(Keys.S);
                        update.left = keyState.IsKeyDown(Keys.D);
                        update.right = keyState.IsKeyDown(Keys.A);
                        if (lastKeyboardState.IsKeyDown(Keys.Space) != keyState.IsKeyDown(Keys.Space))
                            update.jump = keyState.IsKeyDown(Keys.Space);
                        if (lastMouseState.LeftButton.HasFlag(ButtonState.Pressed) != mouseState.LeftButton.HasFlag(ButtonState.Pressed))
                        {
                            update.shoot = mouseState.LeftButton.HasFlag(ButtonState.Pressed);
                            update.PriorityMessage = mouseState.LeftButton.HasFlag(ButtonState.Pressed);
                        }
                        else
                            update.shoot = false;
                        if (lastKeyboardState.IsKeyDown(Keys.R) != keyState.IsKeyDown(Keys.R))
                            update.reload = keyState.IsKeyDown(Keys.R);
                        else
                            update.reload = false;
                        update.rotZ = transform.Rotation.Z;
                        update.rotY = transform.Rotation.Y;
                        update.SnapSeqId = nextSeqNum;
                        unProcessedInputs.Add((nextSeqNum, transform.Position3D));
                        nextSeqNum++;
                        sender.SendData(update);

                        if (lastKeyboardState.IsKeyDown(Keys.Tab) != keyState.IsKeyDown(Keys.Tab))
                        {
                            if (lastKeyboardState.IsKeyDown(Keys.Tab))
                                scoreBoard.HideScoreboard();
                            else
                                scoreBoard.ShowScoreboard();
                        }
                        if (lastKeyboardState.IsKeyDown(Keys.Enter) != keyState.IsKeyDown(Keys.Enter))
                        {
                            if (lastKeyboardState.IsKeyDown(Keys.Enter))
                            {
                                chatting = true;
                                chatField.isWriting = true;
                                GameWorld.Instance.IsMouseVisible = true;
                                //open chat
                            }
                        }
                        if (update.shoot) Shoot(new Vector2(update.rotY, update.rotZ));
                        CameraManager.lightDirection = facing;
                    }
                    lastKeyboardState = keyState;
                    lastMouseState = mouseState;
                }
            }
        }
        private void Shoot(Vector2 lookDirection)
        {
            GameObject bullet = bulletPool.GetLaser();

            bullet.transform.Position3D = transform.Position3D - new Vector3(0, 0, 0.2f);
            bullet.transform.Rotation = new Vector3(0, lookDirection.X, lookDirection.Y);
            bullet.GetComponent<LaserComponent>().FireLaser(transform.Rotation, 10);
        }

        public void HandleSnapShot(PlayerSnapShot snap)
        {
            if (receiver != null) // interpolation
            {
                positionBuffer.Add((DateTime.UtcNow, new Vector3(snap.positionX, snap.positionY, snap.positionZ - 1.6f), snap.rotZ));
            }
            else // recon!
            {
                List<(int seqNum, Vector3 pos)> inputsToRemove = new List<(int seqNum, Vector3 pos)>();
                for (int i = 0; i < unProcessedInputs.Count; i++)
                {
                    if (snap.SnapSeqId > unProcessedInputs[i].seqNum)
                        inputsToRemove.Add(unProcessedInputs[i]);
                    else if (snap.SnapSeqId == unProcessedInputs[i].seqNum)
                    {
                        transform.Position3D += (new Vector3(snap.positionX, snap.positionY, snap.positionZ) - unProcessedInputs[i].pos);
                        inputsToRemove.Add(unProcessedInputs[i]);
                    }
                }
                foreach ((int seqNum, Vector3 pos) input in inputsToRemove)
                {
                    unProcessedInputs.Remove(input);
                }
            }
        }

        string chatMessages = "";
        public void UpdateChat(NetworkMessage message)
        {
            if (message.MessageType == MessageType.ChatMessage)
            {
                chatMessages = chatMessages + "\n" + ((ChatMessage)message).UserName + ": " + ((ChatMessage)message).Message;
                chat.SetText(chatMessages.ToString());
            }
        }
    }
}

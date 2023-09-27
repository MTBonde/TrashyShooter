using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    public class ScoreBoard : Component
    {

        public TextRenderer[] scores = new TextRenderer[10];
        public SpriteRenderer backgroundSprite;

        public void Awake()
        {
            for (int i = 0; i < scores.Length; i++)
            {
                GameObject scoreObject = new GameObject();
                scoreObject.transform.Position = new Vector2(Globals.ScreenCenter.X, 200 + (i + 1) * 50);
                scores[i] = scoreObject.AddComponent<TextRenderer>();
                scores[i].color = Color.Black;
                scores[i].scale = 5;
                scores[i].TextPivot = TextRenderer.TextPivots.TopCenter;
                scores[i].SetText("Space for more players");
            }
            GameObject Background = new GameObject();
            backgroundSprite = Background.AddComponent<SpriteRenderer>();
            backgroundSprite.transform.Position = new Vector2(Globals.ScreenCenter.X, 500);
            backgroundSprite.scale = 1;
            backgroundSprite.SetSprite("ScoreboardBackground");
            HideScoreboard();
        }

        //public void UpdateScoreboard(NetworkMessage update)
        //{
        //    (string name, int score)[] playerScores = ((ScoreboardUpdate)update).scores;
        //    int length;
        //    if (playerScores.Length > scores.Length)
        //        length = scores.Length;
        //    else
        //        length = playerScores.Length;
        //    for (int i = 0; i < length; i++)
        //    {
        //        scores[i].SetText("Player: " + playerScores[i].name + " score is " + playerScores[i].score);
        //    }
        //}

        public void ShowScoreboard()
        {
            backgroundSprite.gameObject.enabled = true;
            for (int i = 0; i < scores.Length; i++)
            {
                scores[i].gameObject.enabled = true;
            }
        }

        public void HideScoreboard()
        {
            backgroundSprite.gameObject.enabled = false;
            for (int i = 0; i < scores.Length; i++)
            {
                scores[i].gameObject.enabled = false;
            }
        }
    }
}

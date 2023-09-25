using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    public class Hud : Component
    {

        TextRenderer healthText;
        TextRenderer ammoText;

        public void Awake()
        {
            healthText = new GameObject().AddComponent<TextRenderer>();
            healthText.TextPivot = TextRenderer.TextPivots.TopLeft;
            healthText.color = Color.White;

            ammoText = new GameObject().AddComponent<TextRenderer>();
            ammoText.TextPivot = TextRenderer.TextPivots.TopLeft;
            ammoText.color = Color.Red;
            ammoText.transform.Position = new Vector2(0, 50);
        }

        public void UpdateDisplay(NetworkMessage message)
        {
            PlayerInfoUpdate update = (PlayerInfoUpdate)message;
            healthText.SetText("Health: " + update.health.ToString());
            ammoText.SetText("Ammo: " + update.ammo.ToString());
        }
    }
}

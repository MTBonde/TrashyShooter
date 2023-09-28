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
        int health;
        public int ammo;
        TextRenderer ammoText;
        AudioSouce takeDammageSound;
        SpriteRenderer healthbar;

        public void Awake()
        {
            healthText = new GameObject().AddComponent<TextRenderer>();
            healthText.TextPivot = TextRenderer.TextPivots.MidRight;
            healthText.SetTextSize(3);
            healthText.color = Color.White;
            healthText.offset = new Vector2 (-500, 0);
            healthText.SetText("100/100");
            SpriteRenderer healthbarPart = healthText.gameObject.AddComponent<SpriteRenderer>();
            healthbarPart.SetSprite("HealthbarInner");
            healthbarPart.layer = 0.11f;
            healthbarPart.color = Color.Red;
            healthbarPart.scale = 0.25f;
            healthbarPart = healthText.gameObject.AddComponent<SpriteRenderer>();
            healthbarPart.SetSprite("HealthbarOuter");
            healthbarPart.layer = 0.13f;
            healthbarPart.scale = 0.25f;
            healthbar = new GameObject().AddComponent<SpriteRenderer>();
            healthbar.layer = 0.12f;
            healthbar.SetSprite("HealthbarInner");
            healthbar.color = Color.Green;
            healthbar.scale = 0.25f;
            healthbar.transform.Position = new Vector2(Globals.ScreenSize.X - 190, Globals.ScreenSize.Y - 80);
            healthText.transform.Position = new Vector2(Globals.ScreenSize.X - 190, Globals.ScreenSize.Y - 80);

            ammoText = new GameObject().AddComponent<TextRenderer>();
            ammoText.TextPivot = TextRenderer.TextPivots.MidRight;
            ammoText.SetTextSize(3);
            ammoText.color = Color.White;
            ammoText.transform.Position = new Vector2(Globals.ScreenSize.X - 125, Globals.ScreenSize.Y - 170);
            ammoText.offset = new Vector2(-300, 0);
            ammoText.SetText("30/30");
            SpriteRenderer ammoBar = ammoText.gameObject.AddComponent<SpriteRenderer>();
            ammoBar.SetSprite("Ammobar");
            ammoBar.layer = 0.13f;
            ammoBar.scale = 0.25f;

            SpriteRenderer crosshair = new GameObject().AddComponent<SpriteRenderer>();
            crosshair.transform.Position = Globals.ScreenCenter;
            crosshair.SetSprite("Crosshair");
            crosshair.scale = 0.1f;

            takeDammageSound = gameObject.AddComponent<AudioSouce>();
            takeDammageSound.SetSoundEffect("TakeDammage");


        }

        public void UpdateDisplay(NetworkMessage message)
        {
            PlayerInfoUpdate update = (PlayerInfoUpdate)message;
            healthText.SetText(update.health.ToString() + "/100");
            ammoText.SetText(update.ammo.ToString() + "/30");
            healthbar.scale = 0.25f * (update.health / 100);
            if (update.health < health)
                takeDammageSound.Play();
            health = update.health;
            ammo = update.ammo;
        }
    }
}

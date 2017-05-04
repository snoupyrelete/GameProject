using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject.Model
{
	public class Player
	{
		// Animation representing the player
		private Texture2D playerTexture;
		public Texture2D PlayerTexture
		{
			get { return playerTexture; }
			set { playerTexture = value; }
		}

		// Position of the Player relative to the upper left side of the screen
		public Vector2 Position;

		// State of the player
		private bool active;
		public bool Active 
		{
			get { return active; }
			set { active = value; } 
		}

		// Amount of hit points that player has
		private int health;
		public int Health 
		{
			get { return health; }
			set { health = value;}
		}

		// Get the width of the player ship
		private int width;
		public int Width
		{
			get { return PlayerTexture.Width; }
		}

		// Get the height of the player ship
		private int height;
		public int Height
		{
			get { return PlayerTexture.Height; }
		}

		public Player()
		{
		}

		public void Initialize(Texture2D texture, Vector2 position)
		{
			PlayerTexture = texture;

			// Set the starting position of the player around the middle of the screen and to the back
			Position = position;

			// Set the player to be active
			Active = true;

			// Set the player health
			Health = 100;
		}

		public void Update()
		{
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(PlayerTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}
}

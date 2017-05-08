using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GameProject.Model;
using GameProject.View;



namespace GameProject
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class ShooterGame : Game
	{
		#region Declaration Section
		// Enemies
		Texture2D enemyTexture;
		List<Enemy> enemies;

		// The rate at which the enemies appear
		TimeSpan enemySpawnTime;
		TimeSpan previousSpawnTime;

		// A random number generator
		Random random;

		Texture2D projectileTexture;
		Texture2D projectileTexture2;
		List<Projectile> projectiles;

		// The rate of fire of the player laser
		TimeSpan fireTime;
		TimeSpan previousFireTime;

		Texture2D explosionTexture;
		List<Animation> explosions;

		//Number that holds the player score
		int score;

		int weapon;

		// The font used to display UI elements
		SpriteFont font;

		// Image used to display the static background
		private Texture2D mainBackground;

		// Parallaxing Layers
		private ParallaxingBackground bgLayer1;
		private ParallaxingBackground bgLayer2;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Player player;

		// Keyboard states used to determine key presses
		private KeyboardState currentKeyboardState;
		private KeyboardState previousKeyboardState;

		// Gamepad states used to determine button presses
		private GamePadState currentGamePadState;
		private GamePadState previousGamePadState;

		// A movement speed for the player
		private float playerMoveSpeed;
		#endregion

		public ShooterGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			player = new Player();

			projectiles = new List<Projectile>();

			// Set the laser to fire every quarter second
			fireTime = TimeSpan.FromSeconds(.15f);

			explosions = new List<Animation>();

			//Set player's score to zero
			score = 0;

			weapon = 1;

			// Initialize the enemies list
			enemies = new List<Enemy>();

			// Set the time keepers to zero
			previousSpawnTime = TimeSpan.Zero;

			// Used to determine how fast enemy respawns
			enemySpawnTime = TimeSpan.FromSeconds(1.0f);

			// Initialize our random number generator
			random = new Random();

			bgLayer1 = new ParallaxingBackground();
			bgLayer2 = new ParallaxingBackground();

			// Set a constant player move speed
			playerMoveSpeed = 8.0f;


			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load the player resources
			Animation playerAnimation = new Animation();
			Texture2D playerTexture = Content.Load<Texture2D>("Animation/shipAnimation");
			playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

			Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
			+ GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
			player.Initialize(playerAnimation, playerPosition);

			bgLayer1.Initialize(Content, "Texture/bgLayer1", GraphicsDevice.Viewport.Width, -1);
			bgLayer2.Initialize(Content, "Texture/bgLayer2", GraphicsDevice.Viewport.Width, -2);

			enemyTexture = Content.Load<Texture2D>("Animation/mineAnimation");

			projectileTexture = Content.Load<Texture2D>("Texture/katana");
			projectileTexture2 = Content.Load<Texture2D>("Texture/missile");


			explosionTexture = Content.Load<Texture2D>("Animation/explosion");

			font = Content.Load<SpriteFont>("Font/gameFont");

			mainBackground = Content.Load<Texture2D>("Texture/mainbackground");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif

			// TODO: Add your update logic here

			// Update the parallaxing background
			bgLayer1.Update();
			bgLayer2.Update();

			UpdateEnemies(gameTime);

			UpdateCollision();

			UpdateProjectiles();

			UpdateExplosions(gameTime);

			// Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
			previousGamePadState = currentGamePadState;
			previousKeyboardState = currentKeyboardState;

			// Read the current state of the keyboard and gamepad and store it
			currentKeyboardState = Keyboard.GetState();
			currentGamePadState = GamePad.GetState(PlayerIndex.One);


			//Update the player
			UpdatePlayer(gameTime);

			base.Update(gameTime);
		}

		private void UpdatePlayer(GameTime gameTime)
		{
			player.Update(gameTime);
			// Get Thumbstick Controls
			player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
			player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

			// Use the Keyboard / Dpad
			if (currentKeyboardState.IsKeyDown(Keys.A) ||
			currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				player.Position.X -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.D) ||
			currentGamePadState.DPad.Right == ButtonState.Pressed)
			{
				player.Position.X += playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.W) ||
			currentGamePadState.DPad.Up == ButtonState.Pressed)
			{
				player.Position.Y -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.S) ||
			currentGamePadState.DPad.Down == ButtonState.Pressed)
			{
				player.Position.Y += playerMoveSpeed;
			}

			if (currentKeyboardState.IsKeyDown(Keys.Z))
			{
				this.weapon = 1;
			}
			if (currentKeyboardState.IsKeyDown(Keys.X))
			{
				this.weapon = 2;
			}

			// Make sure that the player does not go out of bounds
			player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
			player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

			if (gameTime.TotalGameTime - previousFireTime > fireTime)
			{
				// Reset our current time
				previousFireTime = gameTime.TotalGameTime;

				// Add the projectile, but add it to the front and center of the player
				AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
			}

			// reset score if player health goes to zero
			if (player.Health <= 0)
			{
				player.Health = 100;
				score = 0;
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			//TODO: Add your drawing code here
			spriteBatch.Begin();

			spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

			// Draw the moving background
			bgLayer1.Draw(spriteBatch);
			bgLayer2.Draw(spriteBatch);

			// Draw the Enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].Draw(spriteBatch);
			}

			// Draw the Projectiles
			for (int i = 0; i < projectiles.Count; i++)
			{
				projectiles[i].Draw(spriteBatch);
			}

			// Draw the explosions
			for (int i = 0; i < explosions.Count; i++)
			{
				explosions[i].Draw(spriteBatch);
			}

			// Draw the score
			spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
			// Draw the player health
			spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
			//Draw selected weapon
			if (this.weapon == 1)
			{
				spriteBatch.DrawString(font, "weapon: katana", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 60), Color.White);
			}
			else {
				spriteBatch.DrawString(font, "weapon: missile", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 60), Color.White);
			}

			player.Draw(spriteBatch);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void AddEnemy()
		{
			// Create the animation object
			Animation enemyAnimation = new Animation();

			// Initialize the animation with the correct animation information
			enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

			// Randomly generate the position of the enemy
			Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

			// Create an enemy
			Enemy enemy = new Enemy();

			// Initialize the enemy
			enemy.Initialize(enemyAnimation, position);

			// Add the enemy to the active enemies list
			enemies.Add(enemy);
		}

		private void UpdateEnemies(GameTime gameTime)
		{
			// Spawn a new enemy enemy every 1.5 seconds
			if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
			{
				previousSpawnTime = gameTime.TotalGameTime;

				// Add an Enemy
				AddEnemy();
			}

			// Update the Enemies
			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				enemies[i].Update(gameTime);

				if (enemies[i].Active == false)
				{
					// If not active and health <= 0
					if (enemies[i].Health <= 0)
					{
						// Add an explosion
						AddExplosion(enemies[i].Position);
						score += enemies[i].Value;
					}
					enemies.RemoveAt(i);
				}

			}
		}

		private void UpdateCollision()
		{
			// Use the Rectangle's built-in intersect function to 
			// determine if two objects are overlapping
			Rectangle rectangle1;
			Rectangle rectangle2;

			// Only create the rectangle once for the player
			rectangle1 = new Rectangle((int)player.Position.X,
			(int)player.Position.Y,
			player.Width,
			player.Height);

			// Do the collision between the player and the enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				rectangle2 = new Rectangle((int)enemies[i].Position.X,
				(int)enemies[i].Position.Y,
				enemies[i].Width,
				enemies[i].Height);

				// Determine if the two objects collided with each
				// other
				if (rectangle1.Intersects(rectangle2))
				{
					// Subtract the health from the player based on
					// the enemy damage
					player.Health -= enemies[i].Damage;

					// Since the enemy collided with the player
					// destroy it
					enemies[i].Health = 0;

					// If the player health is less than zero we died
					if (player.Health <= 0)
						player.Active = false;
				}
			}
			// Projectile vs Enemy Collision
			for (int i = 0; i < projectiles.Count; i++)
			{
				for (int j = 0; j < enemies.Count; j++)
				{
					// Create the rectangles we need to determine if we collided with each other
					rectangle1 = new Rectangle((int)projectiles[i].Position.X -
					projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
					projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

					rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
					(int)enemies[j].Position.Y - enemies[j].Height / 2,
					enemies[j].Width, enemies[j].Height);

					// Determine if the two objects collided with each other
					if (rectangle1.Intersects(rectangle2))
					{
						enemies[j].Health -= projectiles[i].Damage;
						projectiles[i].Active = false;
					}
				}
			}
		}

		private void AddProjectile(Vector2 position)
		{
			
			if (this.weapon == 1)
			{
				Projectile projectile = new Projectile();
				projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position, weapon);
				projectiles.Add(projectile);
				fireTime = TimeSpan.FromSeconds(.15f);
			}
			else if (this.weapon == 2)
			{
				Projectile projectile = new Projectile();
				projectile.Initialize(GraphicsDevice.Viewport, projectileTexture2, position, weapon);
				projectiles.Add(projectile);
				fireTime = TimeSpan.FromSeconds(1f);
			}
		}

		private void UpdateProjectiles()
		{
			// Update the Projectiles
			for (int i = projectiles.Count - 1; i >= 0; i--)
			{
				projectiles[i].Update();

				if (projectiles[i].Active == false)
				{
					projectiles.RemoveAt(i);
				}

			}
		}

		private void AddExplosion(Vector2 position)
		{
			Animation explosion = new Animation();
			explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
			explosions.Add(explosion);
		}

		private void UpdateExplosions(GameTime gameTime)
		{
			for (int i = explosions.Count - 1; i >= 0; i--)
			{
				explosions[i].Update(gameTime);
				if (explosions[i].Active == false)
				{
					explosions.RemoveAt(i);
				}
			}
		}
	}
}

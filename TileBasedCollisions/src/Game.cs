using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

public class TileBasedGame : Game
{
	private GraphicsDeviceManager m_graphics;
	private SpriteBatch m_spriteBatch;
	private RenderTarget2D m_renderTarget;
	private bool m_bDebugDraw = true;

	private Level m_level;
	private Player m_player;

	// Singleton
	public static TileBasedGame Instance { get; private set; }
	public SpriteFont SpriteFont { get; private set; }
	public Texture2D TilesTex { get; private set; }

	public TileBasedGame ()
	{
		Window.Title = "Tile Based Collisions";
		m_graphics = new GraphicsDeviceManager (this);
		Content.RootDirectory = "Content";

		// Set the singleton instance
		Instance = this;
	}
	
	protected override void Initialize ()
	{
		// Create our render target
		m_renderTarget = new RenderTarget2D (GraphicsDevice, 256, 144);
		// Set the resolution
		m_graphics.PreferredBackBufferWidth = 1280;
		m_graphics.PreferredBackBufferHeight = 720;
		m_graphics.ApplyChanges ();

		base.Initialize ();

		// Create a new SpriteBatch, which can be used to draw textures.
		m_spriteBatch = new SpriteBatch (GraphicsDevice);
	}
	
	protected override void LoadContent ()
	{
		// Load the sprite font
		SpriteFont = Content.Load<SpriteFont> ("SpriteFont");
		// Load the tiles texture
		TilesTex = Content.Load<Texture2D> ("Tiles");
		// Create the collision map
		string[] collisionMap = new string[9]
		{
			"0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
			"0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
			"0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
			"0,0,0,0,0,0,0,0,3,6,7,0,0,0,0,0",
			"1,1,1,1,8,8,8,1,1,1,1,1,8,8,8,1",
			"0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
			"0,0,0,0,0,0,0,0,0,0,4,5,3,0,0,0",
			"0,0,4,5,1,3,0,0,0,2,1,1,1,6,7,0",
			"1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1"
		};
		int levelWidth = 16;
		// Create the level
		m_level = new Level (levelWidth, collisionMap);
		// Create the player
		m_player = new Player (m_level, new Point (Level.TILE_SIZE, Level.TILE_SIZE));
	}
	
	protected override void Update (GameTime gameTime)
	{
		if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState ().IsKeyDown (Keys.Escape))
			Exit ();

		// Get the delta time
		float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
		// Update the input state
		Input.Update ();
		// Toggle debug drawing
		if (Input.IsKeyPressed (Keys.F3)) { m_bDebugDraw = !m_bDebugDraw; }
		// Update the player
		m_player.Update (deltaTime);

		base.Update (gameTime);
	}
	
	protected override void Draw (GameTime gameTime)
	{
		// Draw on the render target
		GraphicsDevice.SetRenderTarget (m_renderTarget);
		GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };
		GraphicsDevice.Clear (new Color (.1f, .1f, .1f));

		base.Draw (gameTime);

		// Begin drawing stuff on the screen
		m_spriteBatch.Begin (SpriteSortMode.Deferred, null, SamplerState.PointClamp);
		// Draw the collision map
		m_level.DrawCollisionMap (m_spriteBatch);
		// Draw the player
		m_player.Draw (m_spriteBatch);
		// Draw debug info
		if (m_bDebugDraw) { DebugDraw.DrawRequests (m_spriteBatch); }
		m_spriteBatch.End ();

		// Stop drawing on the render target
		GraphicsDevice.SetRenderTarget (null);

		
		m_spriteBatch.Begin (SpriteSortMode.Deferred, null, SamplerState.PointClamp);
		// Draw the render target on the screen
		m_spriteBatch.Draw (m_renderTarget, new Rectangle (0, 0, 1280, 720), Color.White);
		// Draw debug text info directly on the screen so it's clearer
		if (m_bDebugDraw) { DebugDraw.DrawTextRequests (m_spriteBatch); }
		m_spriteBatch.End ();

		DebugDraw.ClearRequests ();
		DebugDraw.ClearTextRequests ();
	}

	protected override void UnloadContent ()
	{
		DebugDraw.ClearRequests ();
		DebugDraw.ClearTextRequests ();
	}

	protected override void Dispose (bool bDisposing)
	{
		if (bDisposing)
		{
			// Dispose the render target
			m_renderTarget.Dispose ();
		}

		base.Dispose (bDisposing);
	}
}

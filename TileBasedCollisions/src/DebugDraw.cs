using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class DebugDraw
{
	private static List<DrawRequest> s_drawRequests = new List<DrawRequest> ();
	private static List<TextDrawRequest> s_textRequests = new List<TextDrawRequest> ();

	public static void DrawRectFilled (Rectangle rect, Color color)
	{
		Texture2D tex = new Texture2D (TileBasedGame.Instance.GraphicsDevice, 1, 1);
		tex.SetData (new Color[1] { Color.White });
		s_drawRequests.Add (new DrawRequest (tex, new Rectangle (0, 0, 1, 1), rect, color, false));
	}

	public static void DrawText (string text, Vector2 position, Color color, Vector2 origin, float scale = 1f)
	{
		s_textRequests.Add (new TextDrawRequest (text, position, color, origin, scale));
	}

	public static void DrawRequests (SpriteBatch spriteBatch)
	{
		// Draw each request
		foreach (DrawRequest request in s_drawRequests)
		{
			spriteBatch.Draw (request.texture, request.destRect, request.sourceRect, request.color);
		}
	}

	public static void DrawTextRequests (SpriteBatch spriteBatch)
	{
		// Draw each text request
		foreach (TextDrawRequest request in s_textRequests)
		{
			spriteBatch.DrawString (TileBasedGame.Instance.SpriteFont, request.text, request.position, request.color, 0f, request.origin, request.scale, SpriteEffects.None, 0f);
		}
	}

	public static void ClearRequests ()
	{
		// Dispose all textures
		foreach (DrawRequest request in s_drawRequests)
		{
			request.Dispose ();
		}

		s_drawRequests.Clear ();
	}

	public static void ClearTextRequests ()
	{
		s_textRequests.Clear ();
	}
}

public struct DrawRequest : IDisposable
{
	public Texture2D texture;
	public Rectangle sourceRect;
	public Rectangle destRect;
	public Color color;

	public DrawRequest (Texture2D texture, Rectangle sourceRect, Rectangle destRect, Color color)
	{
		this.texture = texture;
		this.sourceRect = sourceRect;
		this.destRect = destRect;
		this.color = color;
	}

	public void Dispose () {
		texture.Dispose ();
	}
}

public struct TextDrawRequest
{
	public string text;
	public Vector2 position;
	public Color color;
	public Vector2 origin;
	public float scale;

	public TextDrawRequest (string text, Vector2 position, Color color, Vector2 origin, float scale)
	{
		this.text = text;
		this.position = position;
		this.color = color;
		this.origin = origin;
		this.scale = scale;
	}
}

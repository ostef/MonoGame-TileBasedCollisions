using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Level
{
	// Collision map
	private int[,] m_collisionMap;

	public int[,] Collisions { get => m_collisionMap; }
	public int this [int column, int row] { get => m_collisionMap[column, row]; }
	public int Width { get => m_collisionMap.GetLength (0); }
	public int Height { get => m_collisionMap.GetLength (1); }

	public const int TILE_SIZE = 16;

	public Level (int columns, string[] rows)
	{
		m_collisionMap = new int[columns, rows.Length];

		// Parse the rows
		for (int y = 0; y < Height; y++)
		{
			string[] tiles = rows[y].Split (',');

			for (int x = 0; x < Width && x < tiles.Length; x++)
			{
				int.TryParse (tiles[x], out m_collisionMap[x, y]);
			}
		}
	}

	public void DrawCollisionMap (SpriteBatch spriteBatch)
	{
		// Draw each tile
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				// Don't draw void tiles
				if (m_collisionMap[x, y] == 0) { continue; }
				// Draw the correct tile
				SpriteEffects spriteEffects = IsTileXFlipped (m_collisionMap[x, y]) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				Rectangle sourceRect = GetTileSourceRect (m_collisionMap[x, y]);
				Rectangle destRect = new Rectangle (x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
				spriteBatch.Draw (TileBasedGame.Instance.TilesTex, destRect, sourceRect, Color.White, 0f, Vector2.Zero, spriteEffects, 0f);
			}
		}
	}

	public static Rectangle GetTileSourceRect (int tileId)
	{
		Rectangle rectangle = new Rectangle (0, 0, TILE_SIZE, TILE_SIZE);

		switch (tileId)
		{
			case 1:
				return rectangle;
			case 2:
				rectangle.X = 16;
				break;
			case 3:
				rectangle.X = 16;
				break;
			case 4:
				rectangle.X = 32;
				break;
			case 5:
				rectangle.X = 48;
				break;
			case 6:
				rectangle.X = 48;
				break;
			case 7:
				rectangle.X = 32;
				break;
			case 8:
				rectangle.X = 64;
				break;
			default:
				return rectangle;
		}

		return rectangle;
	}

	public static bool IsTileXFlipped (int tileId)
	{
		return tileId == 3 || tileId == 6 || tileId == 7;
	}

	public static bool IsSlopeTile (int tileId)
	{
		return tileId >= 2 && tileId <= 7;
	}

	public static Point GetSlopeYPositions (int tileId)
	{
		switch (tileId)
		{
			case 2:
				return new Point (TILE_SIZE, 0);
			case 3:
				return new Point (0, TILE_SIZE);
			case 4:
				return new Point (TILE_SIZE, TILE_SIZE / 2);
			case 5:
				return new Point (TILE_SIZE / 2, 0);
			case 6:
				return new Point (0, TILE_SIZE / 2);
			case 7:
				return new Point (TILE_SIZE / 2, TILE_SIZE);
			default:
				return Point.Zero;
		}
	}
}

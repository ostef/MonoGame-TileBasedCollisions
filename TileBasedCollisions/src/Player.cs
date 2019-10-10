using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Player
{
	private Level m_level;
	private int m_width = Level.TILE_SIZE;
	private int m_height = Level.TILE_SIZE;
	private float m_moveSpeed = 2f;
	private bool m_bGrounded;
	private bool m_bStandingOnPlatform;
	private bool m_bStandingOnSlope;

	private Point m_startPosition;
	private Point m_position;
	private Vector2 m_remainingVelocity;
	private Vector2 m_velocity;

	public int Width { get => m_width; }
	public int Height { get => m_height; }
	public Point Position { get => m_position; }

	public Player (Level level, Point position)
	{
		m_level = level;
		m_position = position;
		m_startPosition = m_position;
	}

	public void Update (float deltaTime)
	{
		// Reset button
		if (Input.IsKeyPressed (Keys.R)) { m_position = m_startPosition; }
		// Adjust the speed with the up and down arrow keys
		m_moveSpeed += ((Input.IsKeyPressed (Keys.Up) ? 1 : 0) - (Input.IsKeyPressed (Keys.Down) ? 1 : 0)) * .25f;
		m_moveSpeed = MathHelper.Clamp (m_moveSpeed, .25f, float.MaxValue);
		// Movement
		m_velocity.X = ((Input.IsKeyDown (Keys.D) ? 1f : 0f) - (Input.IsKeyDown (Keys.A) ? 1f : 0f)) * m_moveSpeed *  (deltaTime * 60f);
		
		// Apply gravity
		if (!m_bGrounded)
		{
			m_velocity.Y += .75f * (deltaTime * 60f);
		}
		else
		{
			m_velocity.Y = 0f;
		}

		// Jumping
		if (Input.IsKeyPressed (Keys.W) && m_bGrounded) { m_velocity.Y = -10f; }
		// Fall of platform
		if (Input.IsKeyPressed (Keys.S) && m_bStandingOnPlatform) { m_position.Y++; }
		// Handle sub pixel movement
		m_remainingVelocity += m_velocity;
		m_velocity.X = (int)m_remainingVelocity.X;
		m_velocity.Y = (int)m_remainingVelocity.Y;
		m_remainingVelocity -= m_velocity;
		// Horizontal collisions
		HorizontalCollisions ();
		// Apply X-axis movement
		m_position.X += m_velocity.ToPoint ().X;
		// Clamp the position to the level bounds
		m_position.X = MathHelper.Clamp (m_position.X, 0, m_level.Width * Level.TILE_SIZE - m_width);
		// Vertical collisions
		VerticalCollisions ();
		// Apply Y-axis movement
		m_position.Y += m_velocity.ToPoint ().Y;
		// Clamp the position to the level bounds
		m_position.Y = MathHelper.Clamp (m_position.Y, 0, m_level.Height * Level.TILE_SIZE - m_height);
		// Check for ground
		CheckForGround ();
		// Check for slope
		CheckForSlope ();

		// Draw debug info
		DebugDraw.DrawText ("Player speed: " + m_moveSpeed + " px per frame", new Vector2 (10f, 10f), Color.White, Vector2.Zero, 2f);
		DebugDraw.DrawText ("Is grounded: " + m_bGrounded, new Vector2 (10f, 50f), Color.White, Vector2.Zero, 2f);
		DebugDraw.DrawText ("Is standing on platform: " + m_bStandingOnPlatform, new Vector2 (10f, 90f), Color.White, Vector2.Zero, 2f);
		DebugDraw.DrawText ("Is standing on slope: " + m_bStandingOnSlope, new Vector2 (10f, 130f), Color.White, Vector2.Zero, 2f);
		DebugDraw.DrawText ("Velocity: " + m_velocity, new Vector2 (10f, 170f), Color.White, Vector2.Zero, 2f);
	}

	public void Draw (SpriteBatch spriteBatch)
	{
		// Draw the player
		Rectangle sourceRect = new Rectangle (80, 0, Level.TILE_SIZE, Level.TILE_SIZE);
		Rectangle destRect = new Rectangle (m_position, new Point (m_width, m_height));
		spriteBatch.Draw (TileBasedGame.Instance.TilesTex, destRect, sourceRect, Color.White);
	}

	private void HorizontalCollisions ()
	{
		if (m_velocity.X == 0f) { return; }

		// Get the info for collision checking
		int xDir = Math.Sign (m_velocity.X);
		int xVelAbs = (int)Math.Abs (m_velocity.X);
		Rectangle collisionRect = new Rectangle (0, (int)m_position.Y, xVelAbs, m_height);
		collisionRect.X = xDir == 1 ? (int)m_position.X + m_width : (int)m_position.X - xVelAbs;
		int tileSize = Level.TILE_SIZE;
		// Get the surrounding tiles
		int closeColumn = xDir == 1 ? (int)Math.Floor (((float)m_position.X + m_width) / tileSize) : (int)Math.Floor ((float)m_position.X / tileSize);
		int farColumn = xDir == 1 ? (int)Math.Floor (((float)m_position.X + m_width + xVelAbs) / tileSize) : (int)Math.Floor (((float)m_position.X - xVelAbs) / tileSize);
		int minRow = (int)Math.Floor ((float)m_position.Y / tileSize);
		int maxRow = (int)Math.Floor (((float)m_position.Y + m_height - 1f) / tileSize);
		// Clamp to prevent an out of bounds exception
		closeColumn = MathHelper.Clamp (closeColumn, 0, m_level.Width - 1);
		farColumn = MathHelper.Clamp (farColumn, 0, m_level.Width - 1);
		minRow = MathHelper.Clamp (minRow, 0, m_level.Height - 1);
		maxRow = MathHelper.Clamp (maxRow, 0, m_level.Height - 1);
		// Cache the tile infos
		int tileId = 0;
		Rectangle tileRect = new Rectangle (0, 0, tileSize, tileSize);

		bool bCollided = false;

		// Iterate through each tile from the closest to the farthest on X and from the lowest to the highest on Y
		for (int x = closeColumn; x != farColumn + xDir; x += xDir)
		{
			for (int y = maxRow; y > minRow - 1; y--)
			{
				// If we arrived here, that means we didn't collide last time
				bCollided = false;
				// Don't check for collisions on the bottom most tile when climbing slope
				if (m_bStandingOnSlope && y == maxRow) { continue; }
				// Get the tile id
				tileId = m_level[x, y];
				// Only collide with solid tiles
				if (tileId == 0 || tileId == 8) { continue; }
				// Get the tile rect X and Y
				tileRect.X = x * tileSize;
				tileRect.Y = y * tileSize;
				// Check if a collision occured
				bCollided = collisionRect.Intersects (tileRect);
				// Continue checking if we didn't collide
				if (!bCollided) { continue; }
				// Check if this is a slope ahead
				if (Level.IsSlopeTile (tileId))
				{
					// Only collide with the vertical side of a slope
					bool bOnVerticalSide = xDir == 1 ? Level.IsTileXFlipped (tileId) : !Level.IsTileXFlipped (tileId);
					if (!bOnVerticalSide) { continue; }
				}
				// Get the new velocity X
				float newXVel = xDir == 1 ? tileRect.X - collisionRect.X : tileRect.X + tileRect.Width - collisionRect.X - collisionRect.Width;
				// Update the velocity X
				m_velocity.X = newXVel;

				// Stop checking since this is the closest collision possible
				break;
			}

			// Stop checking for collisions on the X-axis if we collided
			if (bCollided) { break; }
		}
	}

	private void VerticalCollisions ()
	{
		if (m_velocity.Y == 0f) { return; }

		// Get the info for collision checking
		int yDir = Math.Sign (m_velocity.Y);
		int yVelAbs = (int)Math.Abs (m_velocity.Y);
		Rectangle collisionRect = new Rectangle (m_position.X, 0, m_width, yVelAbs);
		collisionRect.Y = yDir == 1 ? m_position.Y + m_height : m_position.Y - yVelAbs;
		int tileSize = Level.TILE_SIZE;
		// Get the surrounding tiles
		int minColumn = (int)Math.Floor ((float)(m_position.X) / tileSize);
		int maxColumn = (int)Math.Floor (((float)m_position.X + m_width - 1f) / tileSize);
		int closeRow = yDir == 1 ? (int)Math.Floor (((float)m_position.Y + m_height) / tileSize) : (int)Math.Floor ((float)m_position.Y / tileSize);
		int farRow = yDir == 1 ? (int)Math.Floor (((float)m_position.Y + m_height + yVelAbs) / tileSize) : (int)Math.Floor (((float)m_position.Y - yVelAbs) / tileSize);
		// Clamp to prevent an out of bounds exception
		minColumn = MathHelper.Clamp (minColumn, 0, m_level.Width - 1);
		maxColumn = MathHelper.Clamp (maxColumn, minColumn, m_level.Width - 1);
		closeRow = MathHelper.Clamp (closeRow, 0, m_level.Height - 1);
		farRow = MathHelper.Clamp (farRow, 0, m_level.Height - 1);
		// Cache the tile infos
		int tileId = 0;
		Rectangle tileRect = new Rectangle (0, 0, tileSize, tileSize);

		bool bCollided = false;

		// Iterate through each tile from the closest to the farthest on X and from the lowest to the highest on Y
		for (int y = closeRow; y != farRow + yDir; y += yDir)
		{
			for (int x = minColumn; x < maxColumn + 1; x++)
			{
				// If we arrived here, that means we didn't collide last time
				bCollided = false;
				// Get the tile id
				tileId = m_level[x, y];
				// Only collide with solid tiles
				if (tileId == 0) { continue; }
				// Get the tile rect X and Y
				tileRect.X = x * tileSize;
				tileRect.Y = y * tileSize;
				// Check if we should collide with the slope tile
				if (Level.IsSlopeTile (tileId) && (yDir == 1 || m_position.Y < tileRect.Y + tileRect.Height)) { continue; }
				// Check if a collision occured
				bCollided = collisionRect.Intersects (tileRect);
				// Continue checking if we didn't collide
				if (!bCollided) { continue; }

				// Handle platform collisions
				if (tileId == 8)
				{
					// Don't collide with the platform if going upwards
					if (yDir == -1) { continue; }
					// Don't collide with the platform if we are below it
					if (m_position.Y + m_height > tileRect.Y) { continue; }
				}

				// Get the new velocity Y
				float newYVel = yDir == 1 ? tileRect.Y - collisionRect.Y : tileRect.Y + tileRect.Height - collisionRect.Y - collisionRect.Height;
				// Update the velocity Y
				m_velocity.Y = newYVel;

				// Stop checking since this is the closest collision possible
				break;
			}

			// Stop checking for collisions on the X-axis if we collided
			if (bCollided) { break; }
		}
	}

	private void CheckForGround ()
	{
		// Reset the grounded flags
		m_bGrounded = false;
		m_bStandingOnPlatform = false;
		// Don't check for ground if going upwards
		if (m_velocity.Y < 0f) { return; }
		// Get the needed info for ground checking
		int tileSize = Level.TILE_SIZE;
		int minColumn = (int)Math.Floor ((float)m_position.X / tileSize);
		int maxColumn = (int)Math.Floor (((float)m_position.X + m_width - 1) / tileSize);
		int groundRow = (int)Math.Floor (((float)m_position.Y + m_height) / tileSize);
		// Clamp to prevent an out of bounds exception when accessing the collision map
		minColumn = MathHelper.Clamp (minColumn, 0, m_level.Width - 1);
		maxColumn = MathHelper.Clamp (maxColumn, minColumn, m_level.Width - 1);
		groundRow = MathHelper.Clamp (groundRow, 0, m_level.Height - 1);
		// Cache the tile infos
		int tileId = 0;
		Rectangle tileRect = new Rectangle (0, groundRow * tileSize, tileSize, tileSize);

		// Check for ground
		for (int x = minColumn; x < maxColumn + 1; x++)
		{
			// Update the tile info
			tileId = m_level[x, groundRow];
			tileRect.X = x * tileSize;
			// This tile is not not solid nor a platform, check next
			if (tileId != 1 && tileId != 8) { continue; }
			// Check if the distance is sufficient to say we're grounded
			float groundDistance = Math.Abs (tileRect.Y - (m_position.Y + m_height));

			// Consider we're grounded
			if (groundDistance == 0f)
			{
				// If this is a platform, we're grounded only if we're on top of it
				if (tileId == 8 && m_position.Y + m_height - 1 < tileRect.Y)
				{
					// We're grounded
					m_bGrounded = true;
					m_bStandingOnPlatform = true;
					// Snap to the platform
					m_position.Y = tileRect.Y - m_height;

					// We're standing on a platform, except if we're also partly standing on a solid tile. Check next
					continue;
				}

				// We're grounded
				m_bGrounded = true;
				// Don't consider like we're on a platform if we're also partly on a solid tile
				m_bStandingOnPlatform = false;
				// Snap to the ground
				m_position.Y = tileRect.Y - m_height;

				break;
			}
		}
	}

	private void CheckForSlope ()
	{
		// Reset the standing on slope flag
		m_bStandingOnSlope = false;
		// Don't check for slope when going upwards
		if (m_velocity.Y < 0f) { return; }
		// Get the needed info
		int tileSize = Level.TILE_SIZE;
		int minColumn = (int)Math.Floor ((float)m_position.X / tileSize);
		int maxColumn = (int)Math.Floor (((float)m_position.X + m_width - 1) / tileSize);
		int closeSlopeRow = (int)Math.Floor (((float)m_position.Y + m_height - 1) / tileSize);
		int farSlopeRow = closeSlopeRow + 1;
		// Clamp to prevent an out of bounds exception when accessing the collision map
		minColumn = MathHelper.Clamp (minColumn, 0, m_level.Width - 1);
		maxColumn = MathHelper.Clamp (maxColumn, minColumn, m_level.Width - 1);
		closeSlopeRow = MathHelper.Clamp (closeSlopeRow, 0, m_level.Height - 1);
		farSlopeRow = MathHelper.Clamp (farSlopeRow, 0, m_level.Height - 1);
		// Cache the tile infos
		int tileId = 0;
		Rectangle tileRect = new Rectangle (0, 0, tileSize, tileSize);

		// Check for slope
		for (int y = closeSlopeRow; y < farSlopeRow + 1; y++)
		{
			for (int x = minColumn; x < maxColumn + 1; x++)
			{
				// Update tile infos
				tileId = m_level[x, y];
				tileRect.X = x * tileSize;
				tileRect.Y = y * tileSize;
				// This tile is not a slope, check next
				if (!Level.IsSlopeTile (tileId)) { continue; }
				bool bXFlipped = Level.IsTileXFlipped (tileId);
				// Calculate the position relative to the tile
				float relativeXPos = m_position.X + m_width / 2f - tileRect.X;
				float relativeYPos = m_position.Y + m_height - tileRect.Y;
				// Check if we're on the top of the slope
				bool bOnSlopeMin = bXFlipped ? relativeXPos >= -m_width / 2f && relativeXPos < 0f : relativeXPos >= tileSize && relativeXPos < m_width / 2f + tileSize;
				bool bOnSlopeMax = bXFlipped ? relativeXPos > tileSize && relativeXPos <= tileSize + m_width / 2f : relativeXPos >= -m_width / 2f && relativeXPos < 0f;
				// Make sure we're on slope
				if ((relativeXPos < 0f || relativeXPos > tileSize) && !bOnSlopeMin && !bOnSlopeMax) { continue; }
				if (relativeYPos < 0f || relativeYPos > tileSize) { continue; }
				// Get the slope Y start and Y end
				Point slopeYPositions = Level.GetSlopeYPositions (tileId);
				int newRelativeYPos = 0;

				if (bOnSlopeMin)
				{
					// Stay on the minimum Y value of the slope
					newRelativeYPos = bXFlipped ? slopeYPositions.X - m_height : slopeYPositions.Y - m_height;
					// Don't set the standing on slope flag
				}
				else if (bOnSlopeMax)
				{
					// Stay on the maximum Y value of the slope
					newRelativeYPos = bXFlipped ? slopeYPositions.Y - m_height : slopeYPositions.X - m_height;
					// Don't set the standing on slope flag
				}
				else
				{
					// Climb the slope normally
					newRelativeYPos = (int)MathHelper.Lerp (slopeYPositions.X, slopeYPositions.Y, relativeXPos / tileSize) - m_height;
					// Consider we're on slope
					m_bStandingOnSlope = true;
				}

				// Update the position on the Y-axis
				m_position.Y = newRelativeYPos + tileRect.Y;
				// We're grounded
				m_bGrounded = true;

				// Break only if we're not on slope min or max: there may be another slope
				if (!bOnSlopeMin && !bOnSlopeMax) { break; }
			}
		}
	}
}

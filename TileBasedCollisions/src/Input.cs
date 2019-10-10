using Microsoft.Xna.Framework.Input;

public static class Input
{
	// Keyboard
	private static KeyboardState m_previousKeyboardState;
	private static KeyboardState m_keyboardState;

	public static void Update ()
	{
		m_previousKeyboardState = m_keyboardState;
		m_keyboardState = Keyboard.GetState ();
	}

	public static bool IsKeyDown (Keys key)
	{
		return m_keyboardState.IsKeyDown (key);
	}

	public static bool IsKeyPressed (Keys key)
	{
		return m_previousKeyboardState.IsKeyUp (key) && m_keyboardState.IsKeyDown (key);
	}

	public static bool IsKeyReleased (Keys key)
	{
		return m_previousKeyboardState.IsKeyDown (key) && m_keyboardState.IsKeyUp (key);
	}
}

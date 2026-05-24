using Godot;

public static class MathExtensions
{
    public static float Clamp01(float value)
    {
        return Mathf.Clamp(value, 0, 1);
    }
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(
            Mathf.Lerp(a.X, b.X, t),
            Mathf.Lerp(a.Y, b.Y, t)
        );
    }

    public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 value)
    {
        return new Vector2(
            Mathf.InverseLerp(a.X, b.X, value.X),
            Mathf.InverseLerp(a.Y, b.Y, value.Y)
        );
    }
}
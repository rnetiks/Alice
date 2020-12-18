using System;

namespace Alice
{
    public struct Vector2 {
        public float X;
        public float Y;

        public Vector2(float x, float y) {
            X = x;
            Y = y;
        }
    }
    public class BezierCurve
    {
        private const float Epsilon = 1.0e-3f;
        public Vector2 v1;
        public Vector2 v2;

        public float Evaluate(float Progress)
        {
            var t = Clamp(Progress, 0, 1);
            float dt;
            do
            {
                dt = -(fx(t) - Progress) / dfx(t);
                if (float.IsNaN(dt))
                    break;
                t += Clamp(dt, -1f, 1f);
            } while (Math.Abs(dt) > Epsilon);
            return Clamp(fy(t), 0f, 1f);
        }

        private float fy(float t)
        {
            return 3 * (1 - t) * (1 - t) * t * v1.Y + 3 * (1 - t) * t * t * v2.Y + t * t * t;
        }

        float fx(float t)
        {
            return 3 * (1 - t) * (1 - t) * t * v1.X + 3 * (1 - t) * t * t * v2.X + t * t * t;
        }

        float dfx(float t)
        {
            return -6 * (1 - t) * t * v1.X + 3 * (1 - t) * (1 - t) * v1.X
                - 3 * t * t * v2.X + 6 * (1 - t) * t * v2.X + 3 * t * t;
        }

        public float Clamp(float value, float min, float max)
        {
            if (min > value)
                return min;
            return max < value ? max : value;
        }
    }
}

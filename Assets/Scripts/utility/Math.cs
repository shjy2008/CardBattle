using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Math
{
    public static class Bezier
    {
        static int BinomialCoefficient(int k, int n)
        {
            if (k == 0 || k == n)
                return 1;
            List<int> s = new List<int>(new int[k]);
            s[0] = n - k + 1;
            for (int i = 1; i < k; ++i)
                s[i] = s[i - 1] * (n - k + 1 + i) / (i + 1);
            return s[k - 1];
        }

        static float BernsteinBasis(int i, int n, float t)
        {
            Debug.Assert(t >= 0 && t <= 1);
            if (i < 0 || i > n)
                return 0; // using property of Bernstein Basis
            return BinomialCoefficient(i, n) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
        }

        /// <summary>
        /// return point on the Bezier curve
        /// </summary>
        /// <param name="points">control points</param>
        /// <param name="t">paramter within range [0, 1]</param>
        /// <returns>point on the Bezier curve</returns>
        public static Vector2 DeCasteljau(List<Vector2> points, float t)
        {
            Debug.Assert(0 <= t && t <= 1);
            List<Vector2> temp = new List<Vector2>(points); // copy original data
            while (temp.Count > 1)
            {
                for (var i = 0; i < temp.Count - 1; i++) temp[i] = (1 - t) * temp[i] + t * temp[i + 1];
                temp.RemoveAt(temp.Count - 1);
            }
            return temp[0];
        }

        /// <summary>
        /// return tangent on the Bezier curve
        /// </summary>
        /// <param name="points">control points</param>
        /// <param name="t">paramter within range [0, 1]</param>
        /// <returns>tangent on the Bezier curve</returns>
        public static Vector2 ComputeTangent(List<Vector2> points, float t)
        {
            int n = points.Count - 1; // n is the n in [0, n] where n = points.Count-1;
            Vector2 res = Vector2.zero;
            for (int i = 0; i <= n - 1; i++)
            {
                var bi1 = n * (points[i + 1] - points[i]);
                res += bi1 * BernsteinBasis(i, n - 1, t);
            }
            return res.normalized;
        }

        public static Vector3 DeCasteljau(List<Vector3> points, float t)
        {
            Debug.Assert(0 <= t && t <= 1);
            List<Vector3> temp = new List<Vector3>(points); // copy original data
            while (temp.Count > 1)
            {
                for (var i = 0; i < temp.Count - 1; i++) temp[i] = (1 - t) * temp[i] + t * temp[i + 1];
                temp.RemoveAt(temp.Count - 1);
            }
            return temp[0];
        }

        /// <summary>
        /// return tangent on the Bezier curve
        /// </summary>
        /// <param name="points">control points</param>
        /// <param name="t">paramter within range [0, 1]</param>
        /// <returns>tangent on the Bezier curve</returns>
        public static Vector3 ComputeTangent(List<Vector3> points, float t)
        {
            int n = points.Count - 1; // n is the n in [0, n] where n = points.Count-1;
            Vector3 res = Vector3.zero;
            for (int i = 0; i <= n - 1; i++)
            {
                var bi1 = n * (points[i + 1] - points[i]);
                res += bi1 * BernsteinBasis(i, n - 1, t);
            }
            return res.normalized;
        }
    }

    public static class Parabola
    {
        /// <summary>
        /// return height on the parabola curve
        /// </summary>
        /// <param name="maxHeight">max height it can reach</param>
        /// <param name="t">parameter within range [0, 1]</param>
        /// <returns></returns>
        public static float ComputeHeight(float maxHeight, float t)
        {
            // equation: y = f(x) = -a*(x-0.5*x0)^2 + y0; where x0 is distance, y0 is maxHeight, a is 4*(y0)/((x0)^2)
            float temp = 2 * (t - 0.5f);
            return maxHeight * (1 - temp * temp);
        }

        public static Vector2 ComputeTangent(float maxHeight, float distance, float t)
        {
            // f'(x) = f'(t*x0) = dfdt = -8*y0*(t-0.5)
            // a tangent line is y-y'=m*(x-x') where x',y' is tangent point. x'=t*x0, y'=f(x')
            // also put (0, y'') into it. We can have tangent is (x''-0, y''-y')
            float m = -8 * maxHeight * (t - 0.5f);
            float x1 = t * distance;
            float y1 = ComputeHeight(maxHeight, t);
            float y2 = m * (0 - x1) + y1;
            Vector2 tangent = new Vector2(x1, y1 - y2);
            return tangent.normalized;
        }
    }
}

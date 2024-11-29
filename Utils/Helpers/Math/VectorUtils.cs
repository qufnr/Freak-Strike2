using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils.Helpers.Math;

public class VectorUtils
{
    /// <summary>
    /// 벡터 정규화
    /// </summary>
    /// <param name="vector">기준 벡터</param>
    /// <returns>정규화된 벡터값</returns>
    public static Vector NormalizeVector(Vector vector)
    {
        var vec = new Vector()
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z
        };
        //  벡터에 대한 규모(광도)
        var gwangdo = MathF.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
        return gwangdo is 0 ? vector : vec / gwangdo;
    }

    /// <summary>
    /// 두 벡터에 대한 거리를 반환합니다.
    /// </summary>
    /// <param name="vec1">시작 지점</param>
    /// <param name="vec2">종료 지점</param>
    /// <returns>두 지점의 거리</returns>
    public static float GetDistance(Vector vec1, Vector vec2)
    {
        var x = vec1.X - vec2.X;
        var y = vec1.Y - vec2.Y;
        var z = vec1.Z - vec2.Z;
        return (float) MathF.Sqrt(x * x + y * y + z * z);
    }
}
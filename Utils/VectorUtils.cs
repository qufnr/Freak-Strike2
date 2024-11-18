using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

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
}
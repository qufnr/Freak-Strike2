namespace FreakStrike2.Utils.Helpers;

public class CommonUtils
{
    /// <summary>
    /// 리스트 자료 안에서 무작위로 하나를 가져옵니다.
    /// </summary>
    /// <param name="list">리스트 객체</param>
    /// <typeparam name="T">리스트 타입</typeparam>
    /// <returns>리스트에서 무작위로 뽑힌 하나의 객체</returns>
    public static T GetRandomInList<T>(List<T> list) => list[new Random().Next(list.Count)];

    /// <summary>
    /// min 부터 max 까지 난수 생성
    /// </summary>
    /// <param name="min">최소값</param>
    /// <param name="max">최대값</param>
    /// <returns>무작위 숫자 (int)</returns>
    public static int GetRandomInt(int min, int max)
    {
        return new Random().Next(min, max + 1);
    }

    public static float GetRandomFloat(float min, float max) => (float)new Random().NextDouble() * (max - min) + min;
}
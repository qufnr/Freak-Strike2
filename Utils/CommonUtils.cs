using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

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
}
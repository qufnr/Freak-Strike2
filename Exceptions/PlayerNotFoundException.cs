namespace FreakStrike2.Exceptions;

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException() : base("플레이어를 찾을 수 없습니다.")
    {
        
    }
}
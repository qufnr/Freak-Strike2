namespace FreakStrike2.Exceptions;

public class PlayerAlreadyHaleException : Exception
{
    public PlayerAlreadyHaleException() : base("해당 플레이어는 이미 헤일입니다.")
    {
        
    }
}
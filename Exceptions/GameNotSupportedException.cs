namespace FreakStrike2.Exceptions;

public class GameNotSupportedException : Exception
{
    public GameNotSupportedException() : base("해당 게임은 지원하지 않습니다.")
    {
        
    }
}
namespace FreakStrike2.Exceptions;

public class HumanUndefinedException : Exception
{
    public HumanUndefinedException() : base("서버에 정의된 인간 클래스가 없습니다.")
    {
        
    }
}
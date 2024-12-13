namespace FreakStrike2.Exceptions;

public class SaxtonHaleUndefinedException : Exception
{
    
    public SaxtonHaleUndefinedException() : base("서버에 정의된 색스턴 헤일 클래스가 없습니다.")
    {
        
    }
}
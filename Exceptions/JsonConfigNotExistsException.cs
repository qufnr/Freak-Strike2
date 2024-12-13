namespace FreakStrike2.Exceptions;

public class JsonConfigNotExistsException : Exception
{
    public JsonConfigNotExistsException(string path) : base($"Couldn't find plugin configuration. [Path: {path}]")
    {
        
    }
}
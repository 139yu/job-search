namespace Commons.Base;

public class BusinessException: Exception
{
    public object ErrorCode { get; }
    public string UserMessage { get;}
    public BusinessException(object errorCode, string userMessage,Exception? inner = null)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }

    public BusinessException(object errorCode)
    {
        
    }
}

public class BusinessException<TErrorCode> : BusinessException where TErrorCode : Enum
{
    public new TErrorCode ErrorCode => (TErrorCode)base.ErrorCode;
    public BusinessException(object errorCode, string userMessage,Exception? inner = null) : base(errorCode, userMessage,inner)
    {
    }
}
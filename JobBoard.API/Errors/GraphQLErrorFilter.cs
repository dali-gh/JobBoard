using HotChocolate.Execution;

namespace JobBoard.API.Errors;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is not null)
        {
            return error.WithMessage(error.Exception.Message);
        }
        return error;
    }
}

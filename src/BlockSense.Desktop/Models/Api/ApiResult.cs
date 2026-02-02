namespace BlockSense.Desktop.Models.Api
{
    public abstract record ApiResult<T> : ApiResult
    {
        protected ApiResult() { }

        public sealed record Success(T Data) : ApiResult<T>
        {
            public override bool IsSuccess
                => true;
        }
    }

    public abstract record ApiResult
    {
        public abstract bool IsSuccess
        {
            get;
        }

        protected ApiResult() { }

        public sealed record Failure(ProblemDetails ProblemDetails) : ApiResult
        {
            public override bool IsSuccess
                => false;
        }
    }
}

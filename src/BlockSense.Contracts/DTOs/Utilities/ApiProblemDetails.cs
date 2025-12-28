namespace BlockSense.Contracts.DTOs.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed record ApiProblemDetails
    {
        /// <summary>
        /// 
        /// </summary>
        public required string? Title
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public required int? Status
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public required string? Detail
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public required string? Instance
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public required string? ResultCode
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyDictionary<string, string[]>? ResultDetails
        {
            get;
            init;
        }

        /// <summary>
        /// 
        /// </summary>
        public required string? TraceId
        {
            get;
            init;
        }
    }
}

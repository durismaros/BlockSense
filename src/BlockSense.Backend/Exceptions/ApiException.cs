namespace BlockSense.Backend.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ApiException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract string Type
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract string Title
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract int Status
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ApiException(string message)
            : base(message) { }
    }
}

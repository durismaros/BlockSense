namespace BlockSense.Backend.Exceptions
{
    public abstract class AppException : Exception
    {
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
        public abstract string Title
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract string ErrorCode
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        protected AppException(string message)
            : base(message) { }
    }
}

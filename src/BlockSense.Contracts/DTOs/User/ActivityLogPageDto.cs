namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a paginated page of activity log entries.
    /// </summary>
    public sealed record ActivityLogPageDto
    {
        /// <summary>
        /// The activity log entries on the current page.
        /// </summary>
        public required IReadOnlyList<ActivityLogDto> Entries
        {
            get;
            init;
        }

        /// <summary>
        /// The total number of activity log entries across all pages.
        /// </summary>
        public required ulong TotalCount
        {
            get;
            init;
        }

        /// <summary>
        /// The current page number (one-based).
        /// </summary>
        public required int Page
        {
            get;
            init;
        }

        /// <summary>
        /// The maximum number of entries per page.
        /// </summary>
        public required int PageSize
        {
            get;
            init;
        }

        /// <summary>
        /// The total number of pages, calculated from <see cref="TotalCount"/> and <see cref="PageSize"/>.
        /// Always at least 1.
        /// </summary>
        public int TotalPages
            => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    }
}
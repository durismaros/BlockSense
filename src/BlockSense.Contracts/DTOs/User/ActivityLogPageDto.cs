namespace BlockSense.Contracts.DTOs.User
{
    public sealed class ActivityLogPageDto
    {
        public required IReadOnlyList<ActivityLogDto> Entries
        {
            get;
            init;
        }

        public required ulong TotalCount
        {
            get;
            init;
        }

        public required int Page
        {
            get;
            init;
        }

        public required int PageSize
        {
            get;
            init;
        }

        public int TotalPages
            => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    }
}

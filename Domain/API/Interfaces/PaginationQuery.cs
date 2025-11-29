
namespace Domain.API.Interfaces
{
    public class PaginationQuery
    {
        public required int Page {  get; set; }
        public required byte PageSize { get; set; }
    }
}

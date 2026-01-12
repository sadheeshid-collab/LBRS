namespace LBRS.Book.Service.DTOs
{
    public enum OperationStatusTypes
    {
        None = 0,
        Success = 1,
        Failure = 2,
        NotFound = 3,
        ValidationError = 4,
        Unauthorized = 5,
        DuplicateEntry = 6
    }
}

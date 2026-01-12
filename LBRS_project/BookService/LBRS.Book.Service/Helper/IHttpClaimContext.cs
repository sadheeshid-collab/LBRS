namespace LBRS.Book.Service.Helper
{
    public interface IHttpClaimContext 
    {
        Guid UserId { get; }
        string Role { get; }
    }
}

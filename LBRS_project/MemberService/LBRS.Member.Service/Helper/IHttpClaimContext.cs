namespace LBRS.Member.Service.Helper
{
    public interface IHttpClaimContext
    {
        Guid UserId { get; }
        string Role { get; }
    }
}

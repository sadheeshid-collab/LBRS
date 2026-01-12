namespace LBRS.Member.Service.Helper
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) HashPassword(string password);

        bool VerifyPassword(string password, string hash, string salt);
    }
}

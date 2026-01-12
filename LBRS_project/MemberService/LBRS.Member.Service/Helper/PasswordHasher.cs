using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace LBRS.Member.Service.Helper
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly ILogger<PasswordHasher> _logger;
        private const int SaltSize = 16;      // 128-bit
        private const int KeySize = 32;       // 256-bit
        private const int Iterations = 100000;

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }


        public (string Hash, string Salt) HashPassword(string password)
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var saltBytes = new byte[SaltSize];
                rng.GetBytes(saltBytes);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    saltBytes,
                    Iterations,
                    HashAlgorithmName.SHA256);

                var hashBytes = pbkdf2.GetBytes(KeySize);

                return (
                    Convert.ToBase64String(hashBytes),
                    Convert.ToBase64String(saltBytes)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception:PasswordHash.");
                throw;
            }
        }


        public bool VerifyPassword(string usrPassword, string curHash, string curSalt)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(curSalt);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    usrPassword,
                    saltBytes,
                    100000,
                    HashAlgorithmName.SHA256);

                var hashBytes = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(
                    hashBytes,
                    Convert.FromBase64String(curHash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Password Verification.");
                throw;
            }
        }
    }
}

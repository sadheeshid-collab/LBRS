using LBRS.Member.DBContext;
using LBRS.Member.DBContext.Models;
using LBRS.Member.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LBRS.Member.Repo
{
    public class MemberRegistrationRepository : IMemberRegistrationRepository
    {
        private readonly MemberServiceDbContext _dbContext;
        private readonly ILogger<MemberRegistrationRepository> _logger;

        public MemberRegistrationRepository(MemberServiceDbContext dbContext, ILogger<MemberRegistrationRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;

        }
        public async Task<(bool, Guid)> Add(User user)
        {
            try
            {
                await _dbContext.Users.AddAsync(user);
                var isAdded = await _dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);

                return (isAdded, !isAdded ? Guid.Empty : user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new user : {username}", user.Username);
                throw;
            }
        }

        public async Task<bool> Update(User user)
        {

            try
            {
                _dbContext.Users.Update(user);
                return await _dbContext.SaveChangesAsync().ContinueWith(t => t.Result > 0);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user : {username}", user.Username);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageSize == 0)
                {
                    var getAllUsers = await _dbContext.Users
                                    .AsNoTracking()
                                    .OrderBy(o => o.Username)
                                    .ToListAsync();

                    return getAllUsers;
                }

                var getUsers = await _dbContext.Users
                                .AsNoTracking()
                                .OrderBy(o => o.Username)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

                return getUsers;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }


        }

        public async Task<User?> GetByUserId(Guid UserID)
        {
            try
            {
                return await _dbContext.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.UserId == UserID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID : {UserID}", UserID);
                throw;
            }

        }

        public async Task<User?> CheckExistingUser(string email)
        {
            try
            {
                var existingUser = await _dbContext.Users
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.Username.ToLower() == email.ToLower()
                                        && u.IsActive == true);
                return existingUser;
         

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existing user by email : {email}", email);
                throw;
            }

        }

    }
}

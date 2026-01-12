using LBRS.Member.DBContext.Enums;
using LBRS.Member.DBContext.Models;
using LBRS.Member.Repo.Interfaces;
using LBRS.Member.Service.DTOs;
using LBRS.Member.Service.Helper;
using LBRS.Member.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace LBRS.Member.Service
{
    public class MemberRegistrationService : IMemberRegistrationService
    {
        private readonly IMemberRegistrationRepository _memberRegistrationRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<MemberRegistrationService> _logger;
        private readonly IJwtAuthTokenService _jwtAuthTokenService;
        private readonly IHttpClaimContext _httpClaimContext;


        public MemberRegistrationService(IMemberRegistrationRepository memberRegistrationRepository,
                                            IPasswordHasher passwordHasher, 
                                            IJwtAuthTokenService jwtAuthTokenService,
                                            IHttpClaimContext httpClaimContext,
                                            ILogger<MemberRegistrationService> logger)
        {
            _memberRegistrationRepository = memberRegistrationRepository;
            _passwordHasher = passwordHasher;
            _jwtAuthTokenService = jwtAuthTokenService;
            _httpClaimContext = httpClaimContext;
            _logger = logger;
        }

        public async Task<(OperationStatusTypes, Guid)> Add(UserAddDTO userDTO, UserRoleTypes userRoleType)
        {
            try
            {
                //Check duplidate user by email ID
                var isUserExists = await _memberRegistrationRepository.CheckExistingUser(userDTO.Username);

                if (isUserExists != null)
                {
                    return (OperationStatusTypes.DuplicateEntry, Guid.Empty);
                }

                // Generate the password Hash and Salt
                var (pwdHash, pwdSalt) = _passwordHasher.HashPassword(userDTO.NewPassword);

                var userModel = new User
                {
                    Username = userDTO.Username,
                    PasswordHash = pwdHash,
                    PasswordSalt = pwdSalt,
                    UserRoleType = userRoleType, // Admin or Member access
                    MobileNumber = userDTO.MobileNumber,
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByUserID = _httpClaimContext.UserId  // Get it from claim
                };


                var addedUser = await _memberRegistrationRepository.Add(userModel);

                return (addedUser.Item1 ? OperationStatusTypes.Success : OperationStatusTypes.Failure, addedUser.Item2);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Adding a new user : {username}.", userDTO.Username);
                throw;
            }


        }

        public async Task<OperationStatusTypes> Update(UserUpdateDTO userDTO)
        {
            try
            {
                var existingUser = await _memberRegistrationRepository.GetByUserId(userDTO.UserId);

                if (existingUser == null)
                {
                    return OperationStatusTypes.NotFound;
                }

                // Map updated fields
                existingUser.MobileNumber = userDTO.MobileNumber;
                existingUser.FirstName = userDTO.FirstName;
                existingUser.LastName = userDTO.LastName;
                existingUser.UpdatedDate = DateTime.UtcNow;
                existingUser.UpdatedByUserID = _httpClaimContext.UserId; // Get it from claim

                var isUpdated = await _memberRegistrationRepository.Update(existingUser);
                return isUpdated ? OperationStatusTypes.Success : OperationStatusTypes.Failure;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Update user : {UserID}.", userDTO.UserId);
                throw;
            }
        }

        public async Task<IEnumerable<UserViewDTO>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                // pageSize=0 => return all records
                if (pageSize < 0)
                    pageSize = 10;


                var users = await _memberRegistrationRepository.GetAll(pageNumber, pageSize);

                if (users != null && users.Any())
                {
                    var userDTOs = users.Select(user => new UserViewDTO
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        MobileNumber = user.MobileNumber,
                        UserRoleType = user.UserRoleType,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                    });

                    return userDTOs;
                }

                return Enumerable.Empty<UserViewDTO>();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Get all users.");
                throw;
            }
        }

        public async Task<UserViewDTO?> GetByUserId(Guid UserID)
        {
            try
            {
                var user = await _memberRegistrationRepository.GetByUserId(UserID);

                if (user == null)
                {
                    return null;
                }
                var userDTO = new UserViewDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    MobileNumber = user.MobileNumber,
                    UserRoleType = user.UserRoleType,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive
                };

                return userDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Get user by ID : {UserID}.", UserID);
                throw;
            }
        }

        public async Task<OperationStatusTypes> useraccountdeactivate(Guid UserID)
        {
            try
            {
                var existingUser = await _memberRegistrationRepository.GetByUserId(UserID);


                if (existingUser == null)
                {
                    return OperationStatusTypes.NotFound;
                }

                existingUser.IsActive = false;
                existingUser.UpdatedDate = DateTime.UtcNow;
                existingUser.UpdatedByUserID = _httpClaimContext.UserId; // Get it from claims

                await _memberRegistrationRepository.Update(existingUser);
                return OperationStatusTypes.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Delete user : {UserID}.", UserID);
                throw;
            }

        }

        public async Task<OperationStatusTypes> UserPasswordReset(UserPasswordResetDTO passwordResetDTO)
        {
            try
            {
                var existingUser = await _memberRegistrationRepository.GetByUserId(passwordResetDTO.UserId);

                if (existingUser == null)
                {
                    return OperationStatusTypes.NotFound;
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(passwordResetDTO.CurrentPassword, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    return OperationStatusTypes.Unauthorized;
                }

                // Check new password and current password is not the same
                if (_passwordHasher.VerifyPassword(passwordResetDTO.NewPassword, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    return OperationStatusTypes.DuplicateEntry;
                }

                // Check if new password and confirm password match
                if (passwordResetDTO.NewPassword != passwordResetDTO.ConfirmPassword)
                {
                    return OperationStatusTypes.ValidationError;
                }


                // Generate new password hash and salt
                var (newPwdHash, newPwdSalt) = _passwordHasher.HashPassword(passwordResetDTO.NewPassword);


                existingUser.PasswordHash = newPwdHash;
                existingUser.PasswordSalt = newPwdSalt;
                existingUser.UpdatedDate = DateTime.UtcNow;
                existingUser.UpdatedByUserID = _httpClaimContext.UserId; // Get it from claims

                var isUpdated = await _memberRegistrationRepository.Update(existingUser);
                return isUpdated ? OperationStatusTypes.Success : OperationStatusTypes.Failure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Password reset");
                throw;
            }


        }

        public async Task<(OperationStatusTypes, string)> UserLogin(string userName, string password)
        {
            try
            {
                var checkUserExists = await _memberRegistrationRepository.CheckExistingUser(userName);

                if (checkUserExists == null)
                {
                    return (OperationStatusTypes.NotFound, string.Empty);
                }


                // Verify password
                var isPasswordValid = _passwordHasher.VerifyPassword(password, checkUserExists.PasswordHash, checkUserExists.PasswordSalt);

                if (isPasswordValid)
                { 
                    var token = await _jwtAuthTokenService.GenerateAccessToken(checkUserExists.UserRoleType.ToString(), checkUserExists.UserId);

                    return (OperationStatusTypes.Authorized, token);

                }

                return (OperationStatusTypes.Unauthorized, string.Empty);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: User login for username: {username}.", userName);
                throw;
            }
        }

    }
}

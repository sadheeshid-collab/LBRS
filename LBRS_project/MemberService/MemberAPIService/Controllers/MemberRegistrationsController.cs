using LBRS.Member.DBContext.Enums;
using LBRS.Member.Service.DTOs;
using LBRS.Member.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberAPIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberRegistrationsController : ControllerBase
    {
        private readonly IMemberRegistrationService _memberRegistrationService;
        private readonly ILogger<MemberRegistrationsController> _logger;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberRegistrationsController(IMemberRegistrationService memberRegistrationService, ILogger<MemberRegistrationsController> logger)
        {
            _memberRegistrationService = memberRegistrationService;
            _logger = logger;

        }

        [AllowAnonymous]
        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok("Member Registration API is running.");
        }


        [AllowAnonymous]
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin(string userName, string password)
        {
            try
            {
                var loginResult = await _memberRegistrationService.UserLogin(userName, password);

                switch (loginResult.Item1)
                {
                    case OperationStatusTypes.Authorized:
                        return Ok(new
                        {
                            success = true,
                            message = "Login successful.",
                            token = loginResult.Item2,
                        });

                    case OperationStatusTypes.Unauthorized:
                        return Unauthorized(new
                        {
                            success = false,
                            message = "Invalid password.",
                            token = string.Empty
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "Invalid username.",
                            token = string.Empty
                        });

                    default:
                        return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: User login request.");
                return StatusCode(500, "Exception: User login request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("MemberAdd")]
        public async Task<IActionResult> UserAdd([FromBody] UserAddDTO userDTO)
        {
            try
            {
                var result = await _memberRegistrationService.Add(userDTO, UserRoleTypes.Member);

                switch (result.Item1)
                {
                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "User created successfully.",
                            data = result.Item2
                        });

                    case OperationStatusTypes.DuplicateEntry:
                        return Conflict(new
                        {
                            success = false,
                            message = "Username already exists.",
                            data = Guid.Empty
                        });

                    default:
                        return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: User Add.");
                return StatusCode(500, "Exception: User Add.");
            }
        }

        [AllowAnonymous]
        [HttpPost("AdminAdd")]
        public async Task<IActionResult> AdminAdd([FromBody] UserAddDTO userDTO)
        {
            try
            {
                var result = await _memberRegistrationService.Add(userDTO, UserRoleTypes.Admin);

                switch (result.Item1)
                {
                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "Admin user created successfully.",
                            data = result.Item2
                        });

                    case OperationStatusTypes.DuplicateEntry:
                        return Conflict(new
                        {
                            success = false,
                            message = "Username already exists.",
                            data = Guid.Empty
                        });

                    default:
                        return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: User Admin.");
                return StatusCode(500, "Exception: User Admin.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("GetByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                var user = await _memberRegistrationService.GetByUserId(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (UserViewDTO?)null
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "User retrieved successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetByUserId request.");
                return StatusCode(500, "Exception: GetByUserId request.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _memberRegistrationService.GetAll(pageNumber, pageSize);

                if (users == null || !users.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User no found.",
                        data = (IEnumerable<UserViewDTO>?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Users retrieved successfully.",
                    data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: GetAll users request.");
                return StatusCode(500, "Exception: GetAll users request.");
            }

        }


        [Authorize(Roles = "Admin")]
        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO userDTO)
        {
            try
            {
                var result = await _memberRegistrationService.Update(userDTO);

                switch (result)
                {
                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "User updated successfully."
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "User not found."
                        });

                    default:
                        return NoContent();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: UserUpdate.");
                return StatusCode(500, "Exception: UserUpdate.");
            }
        }


        [Authorize]
        [HttpPost("useraccountdeactivate/{userId}")]
        public async Task<IActionResult> useraccountdeactivate(Guid userId)
        {
            try
            {
                var result = await _memberRegistrationService.useraccountdeactivate(userId);
                switch (result)
                {
                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "User deleted successfully."
                        });

                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "User not found."
                        });

                    default:
                        return NoContent();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: Delete user.");
                return StatusCode(500, "Exception: Delete user.");
            }
        }

        [Authorize]
        [HttpPost("UserPasswordReset")]
        public async Task<IActionResult> UserPasswordReset([FromBody] UserPasswordResetDTO passwordResetDTO)
        {
            try
            {
                var isPwdUpdated = await _memberRegistrationService.UserPasswordReset(passwordResetDTO);

                switch (isPwdUpdated)
                {
                    case OperationStatusTypes.Success:
                        return Ok(new
                        {
                            success = true,
                            message = "Password updated successfully."
                        });


                    case OperationStatusTypes.NotFound:
                        return NotFound(new
                        {
                            success = false,
                            message = "User not found."
                        });

                    case OperationStatusTypes.Unauthorized:
                        return Unauthorized(new
                        {
                            success = false,
                            message = "Invalid current password."
                        });

                    case OperationStatusTypes.DuplicateEntry:
                        return Conflict(new
                        {
                            success = false,
                            message = "New password cannot be the same as the current password."
                        });


                    case OperationStatusTypes.ValidationError:
                        return BadRequest(new
                        {
                            success = false,
                            message = "New password and confirm password do not match."
                        });

                    default:
                        return NoContent();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: User password reset.");
                return StatusCode(500, "Exception: User password reset.");
            }
        }




    }
}

using FluentAssertions;
using LBRS.Member.DBContext.Enums;
using LBRS.Member.DBContext.Models;
using LBRS.Member.Repo.Interfaces;
using LBRS.Member.Service;
using LBRS.Member.Service.DTOs;
using LBRS.Member.Service.Helper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Member.Service.Tests
{
    public class MemberRegistrationServiceTests
    {

        private readonly MemberRegistrationService _memberRegistrationService;
        private readonly Mock<IMemberRegistrationRepository> _memberRegistrationRepository_Mock;
        private readonly Mock<IHttpClaimContext> _httpClaimContext_Mock;
        private readonly Mock<IPasswordHasher> _passwordHasher_Mock;
        private readonly Mock<ILogger<MemberRegistrationService>> _logger;
        private readonly Mock<IJwtAuthTokenService> _jwtAuthTokenService_Mock;


        public MemberRegistrationServiceTests()
        {
            _memberRegistrationRepository_Mock = new Mock<IMemberRegistrationRepository>();
            _httpClaimContext_Mock = new Mock<IHttpClaimContext>();
            _passwordHasher_Mock = new Mock<IPasswordHasher>();
            _logger = new Mock<ILogger<MemberRegistrationService>>();
            _jwtAuthTokenService_Mock = new Mock<IJwtAuthTokenService>();

            _memberRegistrationService = new MemberRegistrationService(
                _memberRegistrationRepository_Mock.Object,
                _passwordHasher_Mock.Object,
                _jwtAuthTokenService_Mock.Object,
                _httpClaimContext_Mock.Object,
                _logger.Object
                );

        }

        #region User creation test cases


        /// <summary>
        /// Add Member user - Success Test case
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Add_User_To_Member_Role_Should_Return_Success()
        {
            var dto = new UserAddDTO
            {
                Username = "member@gmail.com",
                NewPassword = "Test@123",
                ConfirmPassword = "Test@123",
                MobileNumber = "+919876543210",
                FirstName = "Test",
                LastName = "User"
            };

            var hashCode = "hashedpassword";
            var saltCode = "saltpassword";
            var userRoleType = UserRoleTypes.Member;  //Member access
            var userId = Guid.NewGuid();


            _memberRegistrationRepository_Mock.Setup(x => x.CheckExistingUser(dto.Username))
                .ReturnsAsync((User?)null);

            _passwordHasher_Mock.Setup(x => x.HashPassword(dto.NewPassword))
                .Returns((hashCode, saltCode));

            _memberRegistrationRepository_Mock.Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync((true, userId));




            var resData = await _memberRegistrationService.Add(dto, userRoleType); // Member role



            resData.Item1.Should().Be(OperationStatusTypes.Success);
            resData.Item2.Should().Be(userId);

            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(dto.Username), Times.Once);
            _passwordHasher_Mock.Verify(x => x.HashPassword(dto.NewPassword), Times.Once);

            _memberRegistrationRepository_Mock.Verify(x => x.Add(It.Is<User>(
                u => u.Username == dto.Username &&
                u.UserRoleType == userRoleType && // For testing change UserRoleTypes.Admin for failure
                u.MobileNumber == dto.MobileNumber &&
                u.PasswordHash == hashCode &&
                u.PasswordSalt == saltCode &&
                u.FirstName == dto.FirstName &&
                u.LastName == dto.LastName
            )), Times.Once);

        }



        /// <summary>
        /// Add Admin user - Success Test case
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Add_User_To_Admin_Role_Should_Return_Success()
        {
            var dto = new UserAddDTO
            {
                Username = "admin1@gmail.com",
                NewPassword = "Test@12",
                ConfirmPassword = "Test@12",
                MobileNumber = "+919876543210",
                FirstName = "Admin 1",
                LastName = "User 1"
            };

            var hashCode = "hashedpassword";
            var saltCode = "saltpassword";
            var userRoleType = UserRoleTypes.Admin;  //Admin access
            var userId = Guid.NewGuid();


            _memberRegistrationRepository_Mock.Setup(x => x.CheckExistingUser(dto.Username))
                .ReturnsAsync((User?)null);

            _passwordHasher_Mock.Setup(x => x.HashPassword(dto.NewPassword))
                .Returns((hashCode, saltCode));

            _memberRegistrationRepository_Mock.Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync((true, userId));




            var resData = await _memberRegistrationService.Add(dto, userRoleType); // Admin role



            resData.Item1.Should().Be(OperationStatusTypes.Success);
            resData.Item2.Should().Be(userId);


            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(dto.Username), Times.Once);
            _passwordHasher_Mock.Verify(x => x.HashPassword(dto.NewPassword), Times.Once);

            _memberRegistrationRepository_Mock.Verify(x => x.Add(It.Is<User>(
                u => u.Username == dto.Username &&
                u.UserRoleType == userRoleType && // For testing change UserRoleTypes.Member for failure
                u.MobileNumber == dto.MobileNumber &&
                u.PasswordHash == hashCode &&
                u.PasswordSalt == saltCode &&
                u.FirstName == dto.FirstName &&
                u.LastName == dto.LastName
            )), Times.Once);

        }


        /// <summary>
        /// Check existing user (Member) - Success Test case
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Check_Duplicate_Member_User_Should_Return_DuplicateEntry()
        {
            var dto = new UserAddDTO();

            var userRoleType = UserRoleTypes.Member;  //Member access


            _memberRegistrationRepository_Mock.Setup(x => x.CheckExistingUser(dto.Username))
                .ReturnsAsync(new User());


            var resData = await _memberRegistrationService.Add(dto, userRoleType); // Member role


            resData.Item1.Should().Be(OperationStatusTypes.DuplicateEntry);
            resData.Item2.Should().Be(Guid.Empty);

            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(dto.Username), Times.Once);

        }



        /// <summary>
        /// Check existing user (Admin) - Success Test case
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Check_Duplicate_Admin_User_Should_Return_DuplicateEntry()
        {
            var dto = new UserAddDTO();

            var userRoleType = UserRoleTypes.Admin;  //Admin access

            _memberRegistrationRepository_Mock.Setup(x => x.CheckExistingUser(dto.Username))
                .ReturnsAsync(new User());



            var resData = await _memberRegistrationService.Add(dto, userRoleType); // Admin role



            resData.Item1.Should().Be(OperationStatusTypes.DuplicateEntry);
            resData.Item2.Should().Be(Guid.Empty);

            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(dto.Username), Times.Once);

        }

        #endregion


        #region User login test cases


        /// <summary>
        /// Both Memmber and Admin User can be validated.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UserLogin_Should_Return_Authorized_When_Username_Password_Is_Correct()
        {
            var userID = Guid.NewGuid();

            var inputUsername = "member@gmail.com";
            var inputPassword = "Test@12";


            var userModel = new User
            {
                UserId = userID,
                Username = "member@gmail.com",
                PasswordHash = "hashCode",
                PasswordSalt = "saltCode",
                UserRoleType = UserRoleTypes.Member
            };

            var accessToken = "access_token";

            _memberRegistrationRepository_Mock
                .Setup(x => x.CheckExistingUser(inputUsername))
                .ReturnsAsync(userModel);

            _passwordHasher_Mock
                .Setup(x => x.VerifyPassword(inputPassword, userModel.PasswordHash, userModel.PasswordSalt))
                .Returns(true);

            _jwtAuthTokenService_Mock
                .Setup(x => x.GenerateAccessToken(userModel.UserRoleType.ToString(), userModel.UserId))
                .ReturnsAsync(accessToken);



            var resData = await _memberRegistrationService.UserLogin(inputUsername, inputPassword);



            resData.Item1.Should().Be(OperationStatusTypes.Authorized);
            resData.Item2.Should().Be(accessToken);


            _memberRegistrationRepository_Mock .Verify(x => x.CheckExistingUser(inputUsername), 
                Times.Once);
            _passwordHasher_Mock.Verify(x => x.VerifyPassword(inputPassword, userModel.PasswordHash, userModel.PasswordSalt), 
                Times.Once);
            _jwtAuthTokenService_Mock.Verify(x => x.GenerateAccessToken(userModel.UserRoleType.ToString(), userModel.UserId), 
                Times.Once);
        }

        /// <summary>
        /// Username does not matched.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UserLogin_Should_Return_NotFound_When_Username_Is_Not_Matched()
        {

            var inputUsername = "member@gmail.com";
            var inputPassword = "Test@12";


            _memberRegistrationRepository_Mock
                .Setup(x => x.CheckExistingUser(inputUsername))
                .ReturnsAsync((User?)null);


            var resData = await _memberRegistrationService.UserLogin(inputUsername, inputPassword);



            resData.Item1.Should().Be(OperationStatusTypes.NotFound);
            resData.Item2.Should().BeEmpty();


            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(inputUsername),
                Times.Once);

        }




        /// <summary>
        /// Check password.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UserLogin_Should_Return_UnAuthorized_When_Password_Is_Not_Matched()
        {
            var userID = Guid.NewGuid();

            var inputUsername = "member@gmail.com";
            var inputPassword = "Test@12";


            var userModel = new User
            {
                UserId = userID,
                Username = "member@gmail.com",
                PasswordHash = "hashCode",
                PasswordSalt = "saltCode",
                UserRoleType = UserRoleTypes.Member
            };


            _memberRegistrationRepository_Mock
                .Setup(x => x.CheckExistingUser(inputUsername))
                .ReturnsAsync(userModel);

            _passwordHasher_Mock
                .Setup(x => x.VerifyPassword(inputPassword, userModel.PasswordHash, userModel.PasswordSalt))
                .Returns(false);


            var resData = await _memberRegistrationService.UserLogin(inputUsername, inputPassword);



            resData.Item1.Should().Be(OperationStatusTypes.Unauthorized);
            resData.Item2.Should().BeEmpty();


            _memberRegistrationRepository_Mock.Verify(x => x.CheckExistingUser(inputUsername),
                Times.Once);
            _passwordHasher_Mock.Verify(x => x.VerifyPassword(inputPassword, userModel.PasswordHash, userModel.PasswordSalt),
                Times.Once);
            
        }

        #endregion
    }
}
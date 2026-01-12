using FluentAssertions;
using LBRS.Member.DBContext.Enums;
using LBRS.Member.Service.DTOs;
using LBRS.Member.Service.Interfaces;
using MemberAPIService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Member.API.Tests
{
    public class MemberRegistrationsControllerTests
    {
        private readonly Mock<ILogger<MemberRegistrationsController>> _logger_Mock;
        private readonly Mock<IMemberRegistrationService> _memberRegistrationService_Mock;
        private readonly MemberRegistrationsController _controller;

        public MemberRegistrationsControllerTests()
        {
            _memberRegistrationService_Mock = new Mock<IMemberRegistrationService>();
            _logger_Mock = new Mock<ILogger<MemberRegistrationsController>>();

            _controller = new MemberRegistrationsController(_memberRegistrationService_Mock.Object, _logger_Mock.Object);
        }

        #region User Add Tests

        [Fact]
        public async Task Add_New_User_Should_Return_Ok_Request()
        {
            var dto = new UserAddDTO();
            var userID = Guid.NewGuid();

            var responseMessage = (OperationStatusTypes.Success, userID);

            _memberRegistrationService_Mock.Setup(s => s.Add(dto, UserRoleTypes.Member))
                                          .ReturnsAsync(responseMessage);

            var contrlResult = await _controller.UserAdd(dto);


            var resData = contrlResult.Should().BeOfType<OkObjectResult>().Subject;
            resData.StatusCode.Should().Be(200);

            resData.Value.Should().BeEquivalentTo(new
            {
                success = true,
                message = "User created successfully.",
                data = userID
            });

            _memberRegistrationService_Mock.Verify(x => x.Add(dto, UserRoleTypes.Member), Times.Once);
        }

        [Fact]
        public async Task Add_New_User_Should_Return_Conflict_Request_When_Dplicate_Username()
        {
            var dto = new UserAddDTO();
            var userID = Guid.Empty;

            var responseMessage = (OperationStatusTypes.DuplicateEntry, userID);

            _memberRegistrationService_Mock.Setup(s => s.Add(dto, UserRoleTypes.Member))
                                          .ReturnsAsync(responseMessage);

            var contrlResult = await _controller.UserAdd(dto);


            var resData = contrlResult.Should().BeOfType<ConflictObjectResult>().Subject;
            resData.StatusCode.Should().Be(409);

            resData.Value.Should().BeEquivalentTo(new
            {
                success = false,
                message = "Username already exists.",
                data = userID
            });

            _memberRegistrationService_Mock.Verify(x => x.Add(dto, UserRoleTypes.Member), Times.Once);
        }

        [Fact]
        public async Task Add_New_User_Should_Return_Exception()
        {
            var dto = new UserAddDTO();
            var userID = Guid.Empty;

            var responseMessage = (OperationStatusTypes.NotFound, userID);

            _memberRegistrationService_Mock.Setup(s => s.Add(dto, UserRoleTypes.Member))
                                          .ThrowsAsync(new Exception("Exception: User Add."));

            var contrlResult = await _controller.UserAdd(dto);


            var resData = contrlResult.Should().BeOfType<ObjectResult>().Subject;
            resData.StatusCode.Should().Be(500);

            _logger_Mock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);

            _memberRegistrationService_Mock.Verify(x => x.Add(dto, UserRoleTypes.Member), Times.Once);
        }

        [Fact]
        public void UserAddDTO_Should_Be_Valid_When_All_Fields_Are_Correct()
        {
            var dto = new UserAddDTO
            {
                Username = "member1@gmail.com",
                NewPassword = "Test@12",
                ConfirmPassword = "Test@12",
                MobileNumber = "+919123491234",
                FirstName = "Test 1",
                LastName = "Member"
            };

            var results = ValidateDTOWithAnnotations(dto);

            results.Should().BeEmpty();
            //results.Should().HaveCount(0);
        }

        [Fact]
        public void UserAddDTO_Should_Be_Fail_When_Username_Empty()
        {
            var dto = new UserAddDTO
            {
                Username = "",
                NewPassword = "Test@12",
                ConfirmPassword = "Test@12",
                MobileNumber = "+919123491234",
                FirstName = "Test 1",
                LastName = "Member"
            };

            var results = ValidateDTOWithAnnotations(dto);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserAddDTO.Username)));
            results.Should().Contain(r => r.ErrorMessage == "Username is required. Enter your email ID");
        }

        [Fact]
        public void UserAddDTO_Should_Fail_When_EmailID_Invalid()
        {

            var dto = new UserAddDTO
            {
                Username = "member1@gmail",
                NewPassword = "Test@12",
                ConfirmPassword = "Test@12",
                MobileNumber = "+919123491234",
                FirstName = "Test 1",
                LastName = "Member"
            };

            var results = ValidateDTOWithAnnotations(dto);
            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserAddDTO.Username)));
            results.Should().Contain(r => r.ErrorMessage == "Invalid email format");
        }
        #endregion


        #region UserLogin Tests

        [Fact]
        public async Task User_Login_Should_Return_Ok_Request_When_Login_Successful()
        {
            var username = "member1@gmail.com";
            var pwd = "Test@12";
            var LoginAccessToken = "LoginAccessTokenID";

            _memberRegistrationService_Mock.Setup(x => x.UserLogin(username, pwd))
                                            .ReturnsAsync((OperationStatusTypes.Authorized, LoginAccessToken));

            var result = await _controller.UserLogin(username, pwd);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            okResult.StatusCode.Should().Be(200);

            okResult.Value.Should().BeEquivalentTo(new
            {
                success = true,
                message = "Login successful.",
                token = LoginAccessToken
            });

            _memberRegistrationService_Mock.Verify(x => x.UserLogin(username, pwd), Times.Once);
        }

        [Fact]
        public async Task User_Login_Should_Return_Unauthorized_When_Password_Is_Wrong()
        {
            var username = "member1@gmail.com";
            var pwd = "Test@12345671";
            var LoginAccessToken = ""; //Empty token

            _memberRegistrationService_Mock.Setup(x => x.UserLogin(username, pwd))
                                            .ReturnsAsync((OperationStatusTypes.Unauthorized, LoginAccessToken));

            var result = await _controller.UserLogin(username, pwd);

            var unaResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;

            unaResult.StatusCode.Should().Be(401);

            unaResult.Value.Should().BeEquivalentTo(new
            {
                success = false,
                message = "Invalid password.",
                token = LoginAccessToken
            });

            _memberRegistrationService_Mock.Verify(x => x.UserLogin(username, pwd), Times.Once);
        }



        [Fact]
        public async Task User_Login_Should_Return_Unauthorized_When_Username_Is_Wrong()
        {
            var username = "memAdmin@gmail.com"; //Invalid username
            var pwd = "Test@12";
            var LoginAccessToken = ""; //Empty token

            _memberRegistrationService_Mock.Setup(x => x.UserLogin(username, pwd))
                                            .ReturnsAsync((OperationStatusTypes.NotFound, LoginAccessToken));

            var result = await _controller.UserLogin(username, pwd);

            var unaResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;

            unaResult.StatusCode.Should().Be(404);

            unaResult.Value.Should().BeEquivalentTo(new
            {
                success = false,
                message = "Invalid username.",
                token = LoginAccessToken
            });

            _memberRegistrationService_Mock.Verify(x => x.UserLogin(username, pwd), Times.Once);
        }
        #endregion

        private static IList<ValidationResult> ValidateDTOWithAnnotations(object dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(
                dto,
                context,
                results,
                validateAllProperties: true);

            return results;
        }


    }
}
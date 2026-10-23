using BCrypt.Net;
using TaskManagementSystem.Commons;
using Xunit;

namespace TaskManagementSystem.Tests
{
    public class BasicUnitTests
    {
        [Fact]
        public void ApiResponse_ShouldInitializePropertiesCorrectly()
        {
            // Arrange & Act
            var response = new ApiResponse<string>
            {
                Success = true,
                Message = "Thành công",
                DataDTO = "TestData"
            };

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Thành công", response.Message);
            Assert.Equal("TestData", response.DataDTO);
        }

        [Fact]
        public void PagedResult_ShouldContainCorrectDataAndTotals()
        {
            // Arrange
            var items = new List<string> { "Task 1", "Task 2", "Task 3" };

            // Act
            var result = new PagedResult<string>
            {
                Data = items,
                Total = items.Count,
                Page = 1,
                PageSize = 10
            };

            // Assert
            Assert.Equal(3, result.Total);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public void PasswordHashing_ShouldVerifyCorrectlyWithBCrypt()
        {
            // Arrange
            string rawPassword = "Admin@123456";

            // Act
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            bool isValid = BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
            bool isInvalid = BCrypt.Net.BCrypt.Verify("WrongPassword", hashedPassword);

            // Assert
            Assert.True(isValid);
            Assert.False(isInvalid);
        }
    }
}

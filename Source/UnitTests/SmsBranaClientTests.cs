// using System.Security.Cryptography;
// using Xunit;
//
// namespace SmsBranaClient.UnitTests;
//
// public class SmsBranaClientTests
// {
//     [Fact]
//     public void GenerateAuthParams_CreatesCorrectHash()
//     {
//         // Arrange
//         var password = "mySecretPassword";
//         var time = new DateTime(2023, 10, 25, 12, 00, 00);
//         
//         // Act
//         var result = SmsBranaClient.ComputeAuthParams(password, time);
//
//         // Assert
//         Assert.Equal("20231025T120000", result.time);
//         Assert.NotNull(result.salt);
//         Assert.NotEmpty(result.salt);
//         
//         // Verify Hash: MD5(password + time + salt)
//         var expectedInput = password + result.time + result.salt;
//         var expectedHash = CreateMd5(expectedInput);
//         
//         Assert.Equal(expectedHash, result.auth);
//     }
//
//     private static string CreateMd5(string input)
//     {
//         // Duplicating logic here for verification or just use MD5 directly
//         var hashBytes = MD5.HashData(System.Text.Encoding.UTF8.GetBytes(input));
//         return Convert.ToHexString(hashBytes).ToLowerInvariant();
//     }
// }

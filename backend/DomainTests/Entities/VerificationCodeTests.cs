using Infrastructure.Entities;

namespace DomainTests.Entities;

public class VerificationCodeTests
{
    private const string Phone = "+79991234567";

    public class Create
    {
        [Fact]
        public void Create_NewCode_NotBlocked()
        {
            var code = VerificationCode.Create(Phone);

            Assert.Equal(0, code.AttemptsCount);
            Assert.False(code.IsBlocked);
        }
    }

    public class Verify
    {
        [Fact]
        public void Verify_WithCorrectCode_ReturnsValid()
        {
            var code = VerificationCode.Create(Phone);

            var result = code.Verify(code.Code, code.CreatedAt.AddMinutes(1));

            Assert.Equal(VerificationResult.Valid, result);
            Assert.Equal(0, code.AttemptsCount);
        }

        [Fact]
        public void Verify_WithWrongCode_CountsAttempt()
        {
            var code = VerificationCode.Create(Phone);

            var result = code.Verify("0000", code.CreatedAt.AddMinutes(1));

            Assert.Equal(VerificationResult.Invalid, result);
            Assert.Equal(1, code.AttemptsCount);
        }

        [Fact]
        public void Verify_WithExpiredCode_ReturnsExpiredAndKeepsAttempts()
        {
            var code = VerificationCode.Create(Phone);

            var result = code.Verify(code.Code, code.CreatedAt.AddMinutes(6));

            Assert.Equal(VerificationResult.Expired, result);
            Assert.Equal(0, code.AttemptsCount);
        }

        [Fact]
        public void Verify_AtExpiresAt_ReturnsExpired()
        {
            var code = VerificationCode.Create(Phone);

            var result = code.Verify(code.Code, code.ExpiresAt);

            Assert.Equal(VerificationResult.Expired, result);
            Assert.Equal(0, code.AttemptsCount);
        }

        [Fact]
        public void Verify_AfterFiveWrongAttempts_ReturnsBlocked()
        {
            var code = VerificationCode.Create(Phone);
            for (var i = 0; i < 5; i++)
                code.Verify("0000", code.CreatedAt.AddMinutes(1));

            var result = code.Verify(code.Code, code.CreatedAt.AddMinutes(1));

            Assert.Equal(VerificationResult.Blocked, result);
        }
    }

    public class RegenerateCode
    {
        [Fact]
        public void RegenerateCode_WithinMinute_KeepsCode()
        {
            var code = VerificationCode.Create(Phone);
            var oldCode = code.Code;
            var createdAt = code.CreatedAt;
            var expiresAt = code.ExpiresAt;

            code.RegenerateCode(code.CreatedAt.AddSeconds(30));

            Assert.Equal(oldCode, code.Code);
            Assert.Equal(createdAt, code.CreatedAt);
            Assert.Equal(expiresAt, code.ExpiresAt);
        }

        [Fact]
        public void RegenerateCode_AfterMinute_ResetsAttemptsAndUnblocks()
        {
            var code = VerificationCode.Create(Phone);
            for (var i = 0; i < 5; i++)
                code.Verify("0000", code.CreatedAt.AddMinutes(1));
            Assert.True(code.IsBlocked);

            var createdAt = code.CreatedAt;
            code.RegenerateCode(createdAt.AddMinutes(1));

            Assert.Equal(0, code.AttemptsCount);
            Assert.False(code.IsBlocked);
            Assert.Equal(createdAt.AddMinutes(1), code.CreatedAt);
            Assert.Equal(createdAt.AddMinutes(6), code.ExpiresAt);
        }
    }
}

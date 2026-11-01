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
            Assert.Equal(1, code.SendsInWindow);
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

    public class TryResend
    {
        [Fact]
        public void TryResend_ReplacesCode()
        {
            var code = VerificationCode.Create(Phone);
            var oldCode = code.Code;
            var createdAt = code.CreatedAt;

            var result = code.TryResend(createdAt.AddSeconds(30));

            Assert.True(result);
            Assert.NotEqual(oldCode, code.Code);
            Assert.Equal(createdAt.AddSeconds(30), code.CreatedAt);
            Assert.Equal(createdAt.AddSeconds(30).AddMinutes(5), code.ExpiresAt);
        }

        [Fact]
        public void TryResend_ResetsAttemptsAndUnblocks()
        {
            var code = VerificationCode.Create(Phone);
            for (var i = 0; i < 5; i++)
                code.Verify("0000", code.CreatedAt.AddMinutes(1));
            Assert.True(code.IsBlocked);

            var result = code.TryResend(code.CreatedAt.AddMinutes(1));

            Assert.True(result);
            Assert.Equal(0, code.AttemptsCount);
            Assert.False(code.IsBlocked);
        }

        [Fact]
        public void TryResend_AfterFiveSendsInHour_ReturnsFalse()
        {
            var code = VerificationCode.Create(Phone);
            var now = code.CreatedAt;
            for (var i = 0; i < 4; i++)
                Assert.True(code.TryResend(now.AddMinutes(i + 1)));

            var result = code.TryResend(now.AddMinutes(10));

            Assert.False(result);
            Assert.Equal(5, code.SendsInWindow);
        }

        [Fact]
        public void TryResend_AfterHour_ResetsWindow()
        {
            var code = VerificationCode.Create(Phone);
            var now = code.CreatedAt;
            for (var i = 0; i < 4; i++)
                code.TryResend(now.AddMinutes(i + 1));
            Assert.False(code.TryResend(now.AddMinutes(10)));

            var result = code.TryResend(now.AddHours(2));

            Assert.True(result);
            Assert.Equal(1, code.SendsInWindow);
        }
    }

    public class TryRegisterAttempt
    {
        [Fact]
        public void TryRegisterAttempt_Twentieth_Allows()
        {
            var code = VerificationCode.Create(Phone);
            var now = code.CreatedAt;
            for (var i = 0; i < 20; i++)
                Assert.True(code.TryRegisterAttempt(now.AddSeconds(i)));

            Assert.False(code.TryRegisterAttempt(now.AddMinutes(1)));
        }

        [Fact]
        public void TryRegisterAttempt_AfterHour_ResetsWindow()
        {
            var code = VerificationCode.Create(Phone);
            var now = code.CreatedAt;
            for (var i = 0; i < 20; i++)
                code.TryRegisterAttempt(now.AddSeconds(i));

            var result = code.TryRegisterAttempt(now.AddHours(2));

            Assert.True(result);
            Assert.Equal(1, code.AttemptsInWindow);
        }
    }
}

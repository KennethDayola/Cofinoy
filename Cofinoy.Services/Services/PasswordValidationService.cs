using Cofinoy.Services.Manager;
using System.Collections.Generic;
using System.Linq;

namespace Cofinoy.Services.Services
{
    public class PasswordValidationService
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        public ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(password))
            {
                result.IsValid = false;
                result.Errors.Add("Password is required.");
                return result;
            }

            if (password.Length < 6)
            {
                result.IsValid = false;
                result.Errors.Add("Password must be at least 6 characters long.");
            }

            return result;
        }

        public ValidationResult ValidatePasswordChange(string currentPassword, string newPassword, string confirmPassword, string hashedCurrentPassword)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate current password
            if (!PasswordManager.VerifyPassword(currentPassword, hashedCurrentPassword))
            {
                result.IsValid = false;
                result.Errors.Add("The current password is incorrect.");
                return result;
            }

            // Validate new password requirements
            var newPasswordValidation = ValidatePassword(newPassword);
            if (!newPasswordValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(newPasswordValidation.Errors);
            }

            // Check if new password is same as current
            if (PasswordManager.VerifyPassword(newPassword, hashedCurrentPassword))
            {
                result.IsValid = false;
                result.Errors.Add("The new password cannot be the same as the current password.");
            }

            // Check if passwords match
            if (newPassword != confirmPassword)
            {
                result.IsValid = false;
                result.Errors.Add("New password and confirm password do not match.");
            }

            return result;
        }
    }
}
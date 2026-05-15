using System;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementAPI.Services
{

	public class PasswordService
	{

        private PasswordHasher<string> _passwordHasher ;

        public PasswordService()
		{
            _passwordHasher = new PasswordHasher<string>();
        }

        public string HashPassword(string password)
        {
            string result = _passwordHasher.HashPassword(null, password);

            return result;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            bool result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;

            return result;
        }

    }
}

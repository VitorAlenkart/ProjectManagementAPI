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
            return _passwordHasher.HashPassword(null, password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return _passwordHasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;
        }

    }
}

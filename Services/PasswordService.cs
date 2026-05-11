using Microsoft.CodeAnalysis.Scripting;
using System;
using BCrypt.Net;

namespace ProjectManagementAPI.Services
{
	/// <summary>
	/// Class responsible for handling password hashing and verification using BCrypt.Net
	/// </summary>
	public class PasswordService
	{

		public PasswordService()
		{ }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
}

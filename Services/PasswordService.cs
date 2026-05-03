using System;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementAPI.Services
{
	/// <summary>
	/// Class responsible for handling password hashing and verification using ASP.NET Core Identity's PasswordHasher.
	/// </summary>
	public class PasswordService
	{

		public PasswordService()
		{ }

		public string HashPassword(string password)
		{
			var passwordHasher = new PasswordHasher<string>();
			return passwordHasher.HashPassword(null, password);
		}

		public bool VerifyPassword(string hashedPassword, string providedPassword)
		{
			var passwordHasher = new PasswordHasher<string>();
			var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
			return result == PasswordVerificationResult.Success;
		}

	}
}

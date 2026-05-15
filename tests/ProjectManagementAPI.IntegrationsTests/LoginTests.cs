using ProjectManagementAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProjectManagementAPI.UnitTests
{
   public class LoginTests
   {
        private readonly AuthController _authController;
        public LoginTests(AuthController controller) 
        {
            _authController = controller;
        }

        [Fact]
        public void Login()
        {

        }
   }
}

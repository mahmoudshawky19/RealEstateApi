using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Account
{
    public class UserProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
         public string PhoneNumber { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }
            = AccountType.Individual;

        public string? CompanyName { get; set; }

        public DateTime CreatedAt { get; set; }  

        public DateTime? UpdatedAt { get; set; }
    }
}

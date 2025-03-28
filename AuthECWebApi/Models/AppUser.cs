﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthECWebApi.Models
{
    // Class if we need some extra information for user registration etc
    public class AppUser : IdentityUser
    {
        // Any Extra Properties come here along with email/password in Identity User
        [PersonalData]  // Identity recommend adding this attribute for the personal data.
        [Column(TypeName="nvarchar(150)")]
        public string FullName { get; set; } = null!;
    }
}

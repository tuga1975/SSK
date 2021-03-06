﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Identity
{
    public class SSKDbContext : IdentityDbContext<ApplicationUser>
    {
        private static SSKDbContext _dbContext = null;
        public SSKDbContext()
            : base("SSK", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("Membership_Users", "dbo").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<ApplicationUser>().ToTable("Membership_Users", "dbo").Property(p => p.Id).HasColumnName("UserId");

            modelBuilder.Entity<IdentityUserRole>().ToTable("Membership_UserRoles", "dbo");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("Membership_UserLogins", "dbo");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("Membership_UserClaims", "dbo");
            modelBuilder.Entity<IdentityRole>().ToTable("Membership_Roles", "dbo");
        }

        public static SSKDbContext Create()
        {
            if (_dbContext == null)
            {
                _dbContext = new SSKDbContext();
            }
            return _dbContext;
        }
    }

    

    public class ApplicationIdentityManager
    {
        private static UserManager<ApplicationUser> _userManager = null;
        private static RoleManager<IdentityRole> _roleManager = null;

        public static UserManager<ApplicationUser> GetUserManager()
        {
            if (_userManager == null)
            {
                if (EnumAppConfig.IsLocal)
                {
                    _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(SSKDbContext.Create()));
                }
                else
                {
                    _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(CentralizedDBContext.Create()));
                }
                // Configure validation logic for usernames
                _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = false
                };

                // Configure validation logic for passwords
                _userManager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 0,
                    RequireNonLetterOrDigit = false,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireUppercase = false,
                };
            }
            return _userManager;
        }

        public static RoleManager<IdentityRole> GetRoleManager()
        {
            if (_roleManager == null)
            {
                _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(SSKDbContext.Create()));
            }
            return _roleManager;
        }
    }


    public class CentralizedDBContext : IdentityDbContext<ApplicationUser>
    {
        private static CentralizedDBContext _dbContext = null;
        public CentralizedDBContext()
            : base("CentralizedDB", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("Membership_Users", "dbo").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<ApplicationUser>().ToTable("Membership_Users", "dbo").Property(p => p.Id).HasColumnName("UserId");

            modelBuilder.Entity<IdentityUserRole>().ToTable("Membership_UserRoles", "dbo");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("Membership_UserLogins", "dbo");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("Membership_UserClaims", "dbo");
            modelBuilder.Entity<IdentityRole>().ToTable("Membership_Roles", "dbo");
        }

        public static CentralizedDBContext Create()
        {
            if (_dbContext == null)
            {
                _dbContext = new CentralizedDBContext();
            }
            return _dbContext;
        }
    }
}

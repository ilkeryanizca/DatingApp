using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext pContext)
        {
            _context = pContext;
        }

        public async Task<User> Login(string pUserName, string pPassword)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(p => p.UserName == pUserName);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(pPassword, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public async Task<User> Register(User pUser, string pPassword)
        {
            byte[] password_hash, password_salt;
            CreatePasswordHash(pPassword, out password_hash, out password_salt);

            pUser.PasswordHash = password_hash;
            pUser.PasswordSalt = password_salt;

            await _context.Users.AddAsync(pUser);
            await _context.SaveChangesAsync();

            return pUser;
        }

        public async Task<bool> UserExist(string pUserName)
        {
            if (await _context.Users.AnyAsync(p => p.UserName == pUserName))
                return true;

            return false;
        }

        #region Util Methods
        private void CreatePasswordHash(string pPassword, out byte[] password_hash, out byte[] password_salt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                password_salt = hmac.Key;
                password_hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pPassword));
            }
        }

        private bool VerifyPasswordHash(string pPassword, byte[] pPasswordHash, byte[] pPasswordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(pPasswordSalt))
            {
                var computed_hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pPassword));

                for (int i = 0; i < computed_hash.Length; i++)
                {
                    if (computed_hash[i] != pPasswordHash[i])
                        return false;
                }
            }
            return true;
        }
        #endregion
    }
}
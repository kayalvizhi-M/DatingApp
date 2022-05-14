using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly Datacontext _context;
    private readonly ITokenService _tokenService;

    public AccountController(Datacontext context, ITokenService tokenService)
    {
      _tokenService = tokenService;
      this._context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> RegisterUser(RegisterDTO registerDTO)
    {
      if (await this.UserExists(registerDTO.UserName)) return BadRequest("Username has already taken");

      using var hmac = new HMACSHA512();

      var user = new AppUser
      {
        UserName = registerDTO.UserName.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
        PasswordSalt = hmac.Key
      };

      _context.Users.Add(user);

      await _context.SaveChangesAsync();

      return new UserDTO
      {
        UserName = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }
    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
      var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower());
      if (user == null) return Unauthorized("Invalid User!");
      using var hmac = new HMACSHA512(user.PasswordSalt);
      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
      }

      return new UserDTO
      {
        UserName = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    private async Task<bool> UserExists(string userName)
    {
      return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
    }
  }
}
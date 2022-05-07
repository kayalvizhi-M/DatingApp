using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Datacontext : DbContext
  {
    public DbSet<AppUser> Users { get; set; }
    public Datacontext(DbContextOptions options) : base(options)
    {
    }
  }
}
using Microsoft.EntityFrameworkCore;
using System;

namespace GrpcSample.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<PersonEntity> Persons { get; set; }
}

public class PersonEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Family { get; set; }
    public DateTime BirthDate { get; set; }
    public string NationalCode { get; set; }
}


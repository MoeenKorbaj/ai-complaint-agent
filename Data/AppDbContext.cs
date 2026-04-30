using AIComplaintAgent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.Collections.Generic;

namespace AIComplaintAgent.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ComplaintResultModel> Complaints { get; set; }
}
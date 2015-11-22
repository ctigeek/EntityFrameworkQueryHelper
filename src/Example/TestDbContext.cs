using System.Data.Entity;
using ControlPanel.AuditLog.Controllers;
using ControlPanel.AuditLog.Models;
using log4net;

namespace QueryHelper
{
    internal class TestDbContext : DbContext
    {
        public AuditLogContext() : base("name=TheConnectionStringName")
        {
		  //This will show the generated SQL statements to standard out.
          Database.Log = Log.Debug;
        }
        public DbSet<ThePoco> TableMappingSet { get; set; }
    }
}
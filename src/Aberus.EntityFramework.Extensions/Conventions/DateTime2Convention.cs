using System;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class DateTime2Convention : Convention
    {
        public DateTime2Convention()
        {
            Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
        }
    }
}

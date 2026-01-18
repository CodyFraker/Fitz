using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("job")]
    public class Job
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("last_execution")]
        public DateTime LastExecution { get; set; }
    }
}

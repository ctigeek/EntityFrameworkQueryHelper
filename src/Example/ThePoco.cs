using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryHelper
{
	[Table("admLogs")]
    public class ThePoco
    {
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
		public int Id { get; set; }
		
		[Column("username", TypeName = "varchar")]
        [Required, StringLength(50)]
		public string Name { get; set; }
		
		[Column("description", TypeName = "varchar")]
        [Required, StringLength(2000)]
		public string Description { get; set; }
		
		[Column("timestamp", TypeName = "datetime2")]
        [Required]
		public DateTime TimeStamp { get; set; }
		
		[NotMapped]
        public string Uri { get; set; }
	}
}
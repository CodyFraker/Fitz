namespace Bot.Features.FAQ.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.RegularExpressions;

    [Table("faq")]
    public class Faq
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("regex")]
        public Regex Regex { get; set; }

        [Column("message")]
        public string Message { get; set; }
    }
}

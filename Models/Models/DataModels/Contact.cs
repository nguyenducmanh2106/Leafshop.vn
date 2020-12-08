using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models.DataModels
{
    [Table("Contact")]
   public class Contact
    {
        [Key]
        public int ID { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string Noidung { get; set; }
        public int Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

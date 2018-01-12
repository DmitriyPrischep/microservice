using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Fines.Models
{
    public class Fine
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NameFine { get; set; }
        public int AmountFine { get; set; }
    }
}
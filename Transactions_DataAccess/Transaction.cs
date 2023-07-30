﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions_DataAccess
{
    public class Transaction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}

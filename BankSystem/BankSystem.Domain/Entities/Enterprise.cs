﻿using BankSystem.BankClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public class Enterprise : BankRelatedEntity
    {
        public LegalEntityType OrganizationType { get; set; }
        public string LegalName { get; set; }
        public string TaxpayerIdentificationNumber { get; set; }
        public string BankIdentifierCode { get; set; }
        public string LegalAddress { get; set; } 
    }
}

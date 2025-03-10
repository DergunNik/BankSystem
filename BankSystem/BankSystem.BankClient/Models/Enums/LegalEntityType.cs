using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Enums
{
    /// <summary>
    /// Enum representing different types of legal entities in the system.
    /// </summary>
    public enum LegalEntityType
    {
        /// <summary>
        /// Represents a sole proprietorship (ИП).
        /// </summary>
        SoleProprietorship,

        /// <summary>
        /// Represents a limited liability company (ООО).
        /// </summary>
        LimitedLiabilityCompany,

        /// <summary>
        /// Represents a closed joint-stock company (ЗАО).
        /// </summary>
        ClosedJointStockCompany,

        /// <summary>
        /// Represents an open joint-stock company (ОАО).
        /// </summary>
        OpenJointStockCompany,

        /// <summary>
        /// Represents a public organization (НО).
        /// </summary>
        PublicOrganization,

        /// <summary>
        /// Represents a private joint-stock company (ЧАО).
        /// </summary>
        PrivateJointStockCompany
    }

}

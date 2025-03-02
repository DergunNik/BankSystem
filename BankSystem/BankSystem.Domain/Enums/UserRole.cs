using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Enums
{
    /// <summary>
    /// Enum representing different user roles in the bank.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// The Client role allows the user to register in the system (requires manager approval),
        /// interact with accounts (open, close, etc.), apply for loans and installments (requires manager approval),
        /// and apply for the salary project from an enterprise.
        /// </summary>
        Client,

        /// <summary>
        /// The Operator role allows the user to view transaction statistics for users and cancel a transaction
        /// on an account once (any transfer except cash withdrawal). 
        /// The operator also confirms salary project applications after receiving data from the enterprise.
        /// </summary>
        Operator,

        /// <summary>
        /// The Manager role provides all functionality of the Operator role,
        /// with additional privileges of approving loans and installments,
        /// and canceling operations made by external specialists.
        /// </summary>
        Manager,

        /// <summary>
        /// The External Specialist role allows the user to submit documents for a salary project
        /// and request fund transfers to another enterprise or an employee of the enterprise.
        /// </summary>
        ExternalSpecialist,

        /// <summary>
        /// The Administrator role allows the user to view all action logs (logs can be stored in a separate file and encrypted),
        /// and cancel all actions performed by users in the system.
        /// </summary>
        Administrator,

        /// <summary>
        /// These users are not allowed to log in to the system.
        /// <summary
        BannedOrNotExisting
    }

}

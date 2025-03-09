using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Application
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider services)
        {
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();
            await unitOfWork.DeleteDataBaseAsync();
            await unitOfWork.CreateDataBaseAsync();
            var random = new Random();

            List<Bank> banks = new List<Bank>
            {
                new Bank
                {
                    Address = "124124214124",
                    BIC = "135125125",
                    LegalName = "BankAgeev"
                },
                new Bank
                {
                    Address = "1241242141212",
                    BIC = "1351251235",
                    LegalName = "Super Reliable Bank"
                },
                new Bank
                {
                    Address = "1241124214124",
                    BIC = "135121345",
                    LegalName = "Bank of Belarus"
                },
            };

            unitOfWork.BeginTransaction();
            foreach (var bank in banks)
            {
                await unitOfWork.GetRepository<Bank>().AddAsync(bank);
            }
            await unitOfWork.CommitTransactionAsync();

            unitOfWork.BeginTransaction();
            foreach (var bank in banks)
            {
                var id = unitOfWork.GetRepository<Bank>()
                    .FirstOrDefaultAsync(b => b.BIC == bank.BIC).Result!.Id;
                await unitOfWork.GetRepository<BankReserve>().AddAsync(
                    new BankReserve
                    {
                        BankId = id,
                        Balance = 10_000_000m
                    });
            }
            await unitOfWork.CommitTransactionAsync();

            unitOfWork.BeginTransaction();
            var users = new List<User>();
            var accounts = new List<Account>();
            var enterprises = new List<Enterprise>();
            var externalSpecialists = new List<ExternalSpecialist>();

            foreach (var bank in banks)
            {
                var id = unitOfWork.GetRepository<Bank>()
                    .FirstOrDefaultAsync(b => b.BIC == bank.BIC).Result!.Id;

                var bankUsers = new List<User>
                {
                    new User
                    {
                        FullName = "Operator One",
                        PassportSeriesAndNumber = "AB1234567",
                        IdentificationNumber = "1234567890",
                        Phone = "1234567890",
                        Email = $"operator1@{bank.LegalName.Replace(" ", "").ToLower()}.com",
                        IsActive = true,
                        IsApproved = true,
                        UserRole = UserRole.Operator,
                        BankId = id,
                        RequestDate = DateTime.UtcNow,
                        AnswerDate = DateTime.UtcNow.AddMinutes(1),
                        PasswordHash = Argon2.Hash("password1")
                    },
                    new User
                    {
                        FullName = "Manager One",
                        PassportSeriesAndNumber = "AB1234568",
                        IdentificationNumber = "1234567891",
                        Phone = "1234567891",
                        Email = $"manager1@{bank.LegalName.Replace(" ", "").ToLower()}.com",
                        IsActive = true,
                        IsApproved = true,
                        UserRole = UserRole.Manager,
                        BankId = id,
                        RequestDate = DateTime.UtcNow,
                        AnswerDate = DateTime.UtcNow.AddMinutes(1),
                        PasswordHash = Argon2.Hash("password2")
                    },
                    new User
                    {
                        FullName = "External Specialist One",
                        PassportSeriesAndNumber = "AB1234569",
                        IdentificationNumber = "1234567892",
                        Phone = "1234567892",
                        Email = $"specialist1@{bank.LegalName.Replace(" ", "").ToLower()}.com",
                        IsActive = true,
                        IsApproved = true,
                        UserRole = UserRole.ExternalSpecialist,
                        BankId = id,
                        RequestDate = DateTime.UtcNow,
                        AnswerDate = DateTime.UtcNow.AddMinutes(1),
                        PasswordHash = Argon2.Hash("password3")
                    },
                    new User
                    {
                        FullName = "Administrator One",
                        PassportSeriesAndNumber = "AB1234570",
                        IdentificationNumber = "1234567893",
                        Phone = "1234567893",
                        Email = $"admin1@{bank.LegalName.Replace(" ", "").ToLower()}.com",
                        IsActive = true,
                        IsApproved = true,
                        UserRole = UserRole.Administrator,
                        BankId = id,
                        RequestDate = DateTime.UtcNow,
                        AnswerDate = DateTime.UtcNow.AddMinutes(1),
                        PasswordHash = Argon2.Hash("password4")
                    }
                };

                foreach (var user in bankUsers)
                {
                    users.Add(user);
                }

                for (int i = 1; i <= 100; i++)
                {
                    var clientUser = new User
                    {
                        FullName = $"Client {i}",
                        PassportSeriesAndNumber = $"AB{i:0000000}",
                        IdentificationNumber = $"12345678{i:00}",
                        Phone = $"12345678{i:00}",
                        Email = $"client{i}@{bank.LegalName.Replace(" ", "").ToLower()}.com",
                        IsActive = true,
                        IsApproved = true,
                        UserRole = UserRole.Client,
                        BankId = id,
                        RequestDate = DateTime.UtcNow,
                        AnswerDate = DateTime.UtcNow.AddMinutes(1),
                        PasswordHash = Argon2.Hash("password")
                    };

                    users.Add(clientUser);

                    accounts.Add(new Account
                    {
                        Balance = random.Next(1000, 100000),
                        OwnerId = clientUser.Id,
                        IsBlocked = false,
                        IsFrozen = false,
                        IsSavingAccount = false,
                        MonthlyInterestRate = 0m,
                        CreationDate = DateTime.UtcNow,
                        SavingsAccountUntil = DateTime.UtcNow,
                        OwnerType = AccountOwnerType.IndividualUser,
                        BankId = id
                    });
                }

                for (int i = 1; i <= 5; i++)
                {
                    var enterprise = new Enterprise
                    {
                        OrganizationType = LegalEntityType.PublicOrganization,
                        LegalName = $"Enterprise {i} of {bank.LegalName}",
                        TaxpayerIdentificationNumber = $"TIN{i:0000000}",
                        BankIdentifierCode = bank.BIC,
                        LegalAddress = $"Address {i} of {bank.LegalName}",
                        BankId = id
                    };

                    await unitOfWork.GetRepository<Enterprise>().AddAsync(enterprise);
                    enterprises.Add(enterprise);

                    var enterpriseAccount = new Account
                    {
                        Balance = random.Next(100000, 1000000),
                        OwnerId = enterprise.Id,
                        IsBlocked = false,
                        IsFrozen = false,
                        IsSavingAccount = false,
                        MonthlyInterestRate = 0m,
                        CreationDate = DateTime.UtcNow,
                        SavingsAccountUntil = DateTime.UtcNow,
                        OwnerType = AccountOwnerType.Enterprise,
                        BankId = id
                    };

                    accounts.Add(enterpriseAccount);
                }
            }
            await unitOfWork.CommitTransactionAsync();

            unitOfWork.BeginTransaction();
            foreach (var user in users)
            {
                await unitOfWork.GetRepository<User>().AddAsync(user);
            }
            foreach (var account in accounts)
            {
                await unitOfWork.GetRepository<Account>().AddAsync(account);
            }

            foreach (var user in users.Where(u => u.UserRole == UserRole.ExternalSpecialist))
            {
                var enterprise = unitOfWork.GetRepository<Enterprise>()
                    .ListAsync(e => e.BankId == user.BankId).Result[0];
                var externalSpecialist = new ExternalSpecialist
                {
                    EnterpriseId = enterprise.Id,
                    SpecielistId = user.Id
                };

                externalSpecialists.Add(externalSpecialist);
            }

            foreach (var specialist in externalSpecialists)
            {
                await unitOfWork.GetRepository<ExternalSpecialist>().AddAsync(specialist);
            }

            await unitOfWork.CommitTransactionAsync();
        }
    }
}
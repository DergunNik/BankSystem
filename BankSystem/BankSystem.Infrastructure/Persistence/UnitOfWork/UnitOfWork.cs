﻿using BankSystem.BankClient.Models;
using BankSystem.Infrastructure.Persistence.Repositories;
using BankSystem.BankClient.Abstractions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using BankSystem.Infrastructure.Persistence.Data;

namespace BankSystem.Infrastructure.Persistence.UnitOfWork
{
    public class EfUnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private readonly AppDbContext _context = context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _currentTransaction; 

        public IRepository<T> GetRepository<T>() where T : Entity 
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new EfRepository<T>(_context));
        }

        public async Task CreateDataBaseAsync() => await _context.Database.EnsureCreatedAsync();
        public async Task DeleteDataBaseAsync() => await _context.Database.EnsureDeletedAsync();

        public void BeginTransaction()
        {
            _currentTransaction = _context.Database.BeginTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await _currentTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    await RollbackTransactionAsync();
                    throw; 
                }
                finally
                {
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                try
                {
                    await _currentTransaction.RollbackAsync();
                }
                catch (Exception)
                {
                    throw; 
                }
                finally
                {
                    _currentTransaction = null;
                }
            }
        }
    }
}

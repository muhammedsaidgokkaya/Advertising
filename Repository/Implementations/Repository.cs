using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Repository.Implementations
{
    public class Repository<T> where T : DbContext, new()
    {
        private readonly T _context;

        public Repository(T currentContext)
        {
            _context = currentContext;
            ConfigureDbContext(_context);
        }

        private void ConfigureDbContext(DbContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Default Connection bulunmamaktadır.");
            }

            optionsBuilder.UseNpgsql(connectionString);
            ((Context)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public void Save<E>(E entity) where E : class, new()
        {
            using var dbContextTransaction = _context.Database.BeginTransaction();
            try
            {
                _context.Set<E>().Add(entity);
                _context.SaveChanges();
                dbContextTransaction.Commit();
            }
            catch (Exception)
            {
                dbContextTransaction.Rollback();
                throw;
            }
        }

        public void Update<E>(E entity) where E : class, new()
        {
            using var dbContextTransaction = _context.Database.BeginTransaction();
            try
            {
                _context.Set<E>().Update(entity);
                _context.SaveChanges();
                dbContextTransaction.Commit();
            }
            catch (Exception)
            {
                dbContextTransaction.Rollback();
                throw;
            }
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        public E Find<E>(Expression<Func<E, bool>> expression) where E : class, new()
        {
            return _context.Set<E>().Where(expression).FirstOrDefault();
        }

        public E GetById<E>(int id) where E : class
        {
            return _context.Set<E>().Find(id);
        }

        public IEnumerable<E> Filter<E>(Expression<Func<E, bool>> expression) where E : class, new()
        {
            return _context.Set<E>().Where(expression).AsEnumerable();
        }

        public IQueryable<E> FilterAsQueryable<E>(Expression<Func<E, bool>> expression) where E : class, new()
        {
            return _context.Set<E>().Where(expression).AsQueryable();
        }
    }
}
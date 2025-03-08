﻿using DAL.EF;
using DAL.Interfaces;
using DAL.Models;

namespace DAL.Repositories
{
    // Паттерн Dispose без IDisposable
    // https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    // If your class has an IDisposable field or property but doesn't own it, meaning the class doesn't create the object,
    // then the class doesn't need to implement IDisposable.
    public class UnitOfWork(ApplicationContext context) : IUnitOfWork
    {
        private ApplicationContext _context = context;
        private GenericRepository<Direction>? _directionRepository;
        private GenericRepository<Project>? _projectRepository;
        private GenericRepository<Trainee>? _traineeRepository;

        public GenericRepository<Direction> Directions
        {
            get { return _directionRepository ??= new GenericRepository<Direction>(_context); }
        }

        public GenericRepository<Project> Projects
        {
            get { return _projectRepository ??= new GenericRepository<Project>(_context); }
        }

        public GenericRepository<Trainee> Trainees
        {
            get { return _traineeRepository ??= new GenericRepository<Trainee>(_context); }
        }

        public async Task Save() =>
            await _context.SaveChangesAsync();

        private bool disposed = false;
        protected virtual async Task Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    await _context.DisposeAsync();
                }
            }
            disposed = true;
        }

        public async Task Dispose()
        {
            await Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

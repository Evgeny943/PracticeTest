﻿using DAL.EF;
using DAL.Interfaces;
using DAL.Models;

namespace DAL.Repositories
{
    public class ProjectRepository: IProjectRepository, ICRUDableRepository<Project>
    {
        private ApplicationContext _db;

        public ProjectRepository(ApplicationContext context)
        {
            _db = context;
        }

        public Project? Retrieve(int id) => _db.Projects.FirstOrDefault(x => x.Id == id);

        public void Create(Project project)
        {
            _db.Add(project);
            _db.SaveChanges();
        }

        public void Update(Project project)
        {
            _db.Update(project);
            _db.SaveChanges();
        }

        public Project? Delete(int id)
        {
            var project = _db.Projects.Find(id);
            if (project == null)
                return null;
            _db.Remove(project);
            _db.SaveChanges();
            return project;
        }

        public IEnumerable<Project> GetAll() => _db.Projects.ToList();
    }
}

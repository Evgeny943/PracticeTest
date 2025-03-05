﻿using AutoMapper;
using BLL.DTO;
using BLL.Exeptions;
using BLL.Interfaces;
using BLL.Validators;
using DAL.Interfaces;
using DAL.Models;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;
using WebApi.Models;

namespace BLL.Services
{
    public class TraineeService: ITraineeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public TraineeService(IMapper mapper, IUnitOfWork unitOfWork) 
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task Create(TraineeDTO traineeDto)
        {
            var validador = new TraineeValidatorCreate(_unitOfWork);
            ValidationResult result = await validador.ValidateAsync(traineeDto);
            foreach (var failure in result.Errors)
            {
                throw new ValidationException(failure.ErrorMessage);
            }
            var trainee = _mapper.Map<Trainee>(traineeDto);
            trainee.Project = await _unitOfWork.Projects.Retrieve(x => x.Id == trainee.Project.Id);
            trainee.Direction = await _unitOfWork.Directions.Retrieve(x => x.Id == trainee.Direction.Id);
            await _unitOfWork.Trainees.Create(trainee);
            await _unitOfWork.Save();
        }

        public async Task AttachDirection(TraineeDTO traineeDto, DirectionDTO directionDto)
        {
            var trainee = await _unitOfWork.Trainees.Retrieve(x => x.Id == traineeDto.Id);
            if (trainee == null) 
                throw new ArgumentNullException($"Trainee {traineeDto.Id} doesn`t exist");
            var direction = await _unitOfWork.Directions.Retrieve(x => x.Id == directionDto.Id);
            if (direction == null)
                throw new DirectionNotFoundException($"Direction {directionDto.Id} doesn`t exist");
            trainee.Direction = direction;
            await _unitOfWork.Trainees.Update(trainee);
            await _unitOfWork.Directions.Update(direction);
            await _unitOfWork.Save();
        }

        public async Task AttachProject(TraineeDTO traineeDto, ProjectDTO projectDto)
        {
            var trainee = await _unitOfWork.Trainees.Retrieve(x => x.Id == traineeDto.Id);
            if (trainee == null) 
                throw new ArgumentNullException($"Trainee {traineeDto.Id} doesn`t exist");
            var project = await _unitOfWork.Projects.Retrieve(x => x.Id == projectDto.Id);
            if (project == null)
                throw new ProjectNotFoundException($"Project {projectDto.Id} doesn`t exist");
            trainee.Project = project;
            await _unitOfWork.Trainees.Update(trainee);
            await _unitOfWork.Projects.Update(project);
            await _unitOfWork.Save();
        }

        public async Task Update(TraineeDTO traineeDto)
        {
            var trainee = await _unitOfWork.Trainees.Retrieve(x => x.Id == traineeDto.Id);

            var validador = new TraineeValidatorUpdate(_unitOfWork);
            ValidationResult result = await validador.ValidateAsync(traineeDto);
            foreach (var failure in result.Errors)
            {
                throw new ValidationException(failure.ErrorMessage);
            }

            trainee.Name = traineeDto.Name;
            trainee.Surname = traineeDto.Surname;
            trainee.Gender = (Gender)traineeDto.Gender;
            trainee.Email = traineeDto.Email;
            trainee.BirthDay = traineeDto.BirthDay;
            trainee.Phone = traineeDto.Phone;
            await _unitOfWork.Trainees.Update(trainee);
            await _unitOfWork.Save();
        }

        public async Task<(IEnumerable<TraineeDTO>, int)> GetAll(
            SortingKey sortKey, bool descending = false,
            int index = 0, int size = 10, string? name = null)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index < 0");

            Expression<Func<Trainee, bool>>? predicate = null;
            if (name != null)
                predicate = d => d.Name == name;


            var directions = await _unitOfWork.Trainees.GetAll(
                "", predicate, orderBy: null, descending, index, size);
            return (directions.Item1.Select(x => _mapper.Map<TraineeDTO>(x)), directions.Item2);
        }

        public async Task<TraineeDTO> Retrieve(int id)
        {
            var trainee = await _unitOfWork.Trainees.Retrieve(x => x.Id == id, "Direction,Project");
            if (trainee == null)
                throw new NullReferenceException("Trainee not found");
            return _mapper.Map<TraineeDTO>(trainee);
        }

        public async Task<TraineeDTO> Retrieve(string email)
        {
            var trainee = await _unitOfWork.Trainees.Retrieve(x => x.Email == email, "Direction,Project");
            if (trainee == null)
                throw new NullReferenceException("Trainee not found");
            return _mapper.Map<TraineeDTO>(trainee);
        }

        public IEnumerable<TraineeDTO> FilterByDirections(
            IEnumerable<TraineeDTO> traineeDTOs, int directionId)
        {
            return traineeDTOs.Where(x => x.Direction.Id == directionId);
        }

        public IEnumerable<TraineeDTO> FilterByProjects(
            IEnumerable<TraineeDTO> traineeDTOs, int projectId)
        {
            return traineeDTOs.Where(x => x.Project.Id == projectId);
        }

        public Task<TraineeDTO> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TraineeDTO>> GetAll()
        {
            var trainees = await _unitOfWork.Trainees.GetAll(
                "Project,Direction", null, q => q.OrderBy(x => x.Id), false, 0, 100);
            return trainees.Item1.Select(x => _mapper.Map<TraineeDTO>(x));
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Repositories;
using Snapfish.API.API.ViewModels;
using Car = Snapfish.API.API.Models.Car;

namespace Snapfish.API.API.Commands
{
    public class PatchCarCommand : IPatchCarCommand
    {
        private readonly IActionContextAccessor actionContextAccessor;
        private readonly IObjectModelValidator objectModelValidator;
        private readonly ICarRepository carRepository;
        private readonly IMapper<Car, ViewModels.Car> carToCarMapper;
        private readonly IMapper<Car, SaveCar> carToSaveCarMapper;
        private readonly IMapper<SaveCar, Car> saveCarToCarMapper;

        public PatchCarCommand(
            IActionContextAccessor actionContextAccessor,
            IObjectModelValidator objectModelValidator,
            ICarRepository carRepository,
            IMapper<Car, ViewModels.Car> carToCarMapper,
            IMapper<Car, SaveCar> carToSaveCarMapper,
            IMapper<SaveCar, Car> saveCarToCarMapper)
        {
            this.actionContextAccessor = actionContextAccessor;
            this.objectModelValidator = objectModelValidator;
            this.carRepository = carRepository;
            this.carToCarMapper = carToCarMapper;
            this.carToSaveCarMapper = carToSaveCarMapper;
            this.saveCarToCarMapper = saveCarToCarMapper;
        }

        public async Task<IActionResult> ExecuteAsync(
            int carId,
            JsonPatchDocument<SaveCar> patch,
            CancellationToken cancellationToken)
        {
            var car = await carRepository.Get(carId, cancellationToken);
            if (car == null)
            {
                return new NotFoundResult();
            }

            var saveCar = carToSaveCarMapper.Map(car);
            var modelState = actionContextAccessor.ActionContext.ModelState;
            patch.ApplyTo(saveCar, modelState);
            objectModelValidator.Validate(
                actionContextAccessor.ActionContext,
                validationState: null,
                prefix: null,
                model: saveCar);
            if (!modelState.IsValid)
            {
                return new BadRequestObjectResult(modelState);
            }

            saveCarToCarMapper.Map(saveCar, car);
            await carRepository.Update(car, cancellationToken);
            var carViewModel = carToCarMapper.Map(car);

            return new OkObjectResult(carViewModel);
        }
    }
}
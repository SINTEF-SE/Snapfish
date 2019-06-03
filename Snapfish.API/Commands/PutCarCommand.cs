using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Repositories;
using Snapfish.API.API.ViewModels;
using Car = Snapfish.API.API.Models.Car;

namespace Snapfish.API.API.Commands
{
    public class PutCarCommand : IPutCarCommand
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper<Car, ViewModels.Car> _carToCarMapper;
        private readonly IMapper<SaveCar, Car> _saveCarToCarMapper;

        public PutCarCommand(
            ICarRepository carRepository,
            IMapper<Car, ViewModels.Car> carToCarMapper,
            IMapper<SaveCar, Car> saveCarToCarMapper)
        {
            _carRepository = carRepository;
            _carToCarMapper = carToCarMapper;
            _saveCarToCarMapper = saveCarToCarMapper;
        }

        public async Task<IActionResult> ExecuteAsync(int carId, SaveCar saveCar, CancellationToken cancellationToken)
        {
            var car = await _carRepository.Get(carId, cancellationToken);
            if (car == null)
            {
                return new NotFoundResult();
            }

            _saveCarToCarMapper.Map(saveCar, car);
            car = await _carRepository.Update(car, cancellationToken);
            var carViewModel = _carToCarMapper.Map(car);

            return new OkObjectResult(carViewModel);
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Constants;
using Snapfish.API.API.Repositories;
using Snapfish.API.API.ViewModels;

namespace Snapfish.API.API.Commands
{
    public class PostCarCommand : IPostCarCommand
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper<Models.Car, Car> _carToCarMapper;
        private readonly IMapper<SaveCar, Models.Car> _saveCarToCarMapper;

        public PostCarCommand(
            ICarRepository carRepository,
            IMapper<Models.Car, Car> carToCarMapper,
            IMapper<SaveCar, Models.Car> saveCarToCarMapper)
        {
            _carRepository = carRepository;
            _carToCarMapper = carToCarMapper;
            _saveCarToCarMapper = saveCarToCarMapper;
        }

        public async Task<IActionResult> ExecuteAsync(SaveCar saveCar, CancellationToken cancellationToken)
        {
            var car = _saveCarToCarMapper.Map(saveCar);
            car = await _carRepository.Add(car, cancellationToken);
            var carViewModel = _carToCarMapper.Map(car);

            return new CreatedAtRouteResult(
                CarsControllerRoute.GetCar,
                new { carId = carViewModel.CarId },
                carViewModel);
        }
    }
}
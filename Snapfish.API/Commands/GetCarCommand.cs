using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Models;
using Snapfish.API.API.Repositories;

namespace Snapfish.API.API.Commands
{
    public class GetCarCommand : IGetCarCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICarRepository _carRepository;
        private readonly IMapper<Car, ViewModels.Car> _carMapper;

        public GetCarCommand(
            IActionContextAccessor actionContextAccessor,
            ICarRepository carRepository,
            IMapper<Car, ViewModels.Car> carMapper)
        {
            _actionContextAccessor = actionContextAccessor;
            _carRepository = carRepository;
            _carMapper = carMapper;
        }

        public async Task<IActionResult> ExecuteAsync(int carId, CancellationToken cancellationToken)
        {
            var car = await _carRepository.Get(carId, cancellationToken);
            if (car == null)
            {
                return new NotFoundResult();
            }

            var httpContext = _actionContextAccessor.ActionContext.HttpContext;
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
            {
                if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) &&
                    (modifiedSince >= car.Modified))
                {
                    return new StatusCodeResult(StatusCodes.Status304NotModified);
                }
            }

            var carViewModel = _carMapper.Map(car);
            httpContext.Response.Headers.Add(HeaderNames.LastModified, car.Modified.ToString("R"));
            return new OkObjectResult(carViewModel);
        }
    }
}
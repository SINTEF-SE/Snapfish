using System;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Models;
using Snapfish.API.API.Services;
using Snapfish.API.API.ViewModels;

namespace Snapfish.API.API.Mappers
{
    public class CarToSaveCarMapper : IMapper<Models.Car, SaveCar>, IMapper<SaveCar, Models.Car>
    {
        private readonly IClockService _clockService;

        public CarToSaveCarMapper(IClockService clockService) =>
            _clockService = clockService;

        public void Map(Models.Car source, SaveCar destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.Cylinders = source.Cylinders;
            destination.Make = source.Make;
            destination.Model = source.Model;
        }

        public void Map(SaveCar source, Models.Car destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var now = _clockService.UtcNow;

            if (destination.Created == DateTimeOffset.MinValue)
            {
                destination.Created = now;
            }

            destination.Cylinders = source.Cylinders;
            destination.Make = source.Make;
            destination.Model = source.Model;
            destination.Modified = now;
        }
    }
}
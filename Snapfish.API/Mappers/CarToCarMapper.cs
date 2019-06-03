using System;
using Microsoft.AspNetCore.Mvc;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Constants;
using Snapfish.API.API.ViewModels;

namespace Snapfish.API.API.Mappers
{

    public class CarToCarMapper : IMapper<Models.Car, Car>
    {
        private readonly IUrlHelper _urlHelper;
        public CarToCarMapper(IUrlHelper urlHelper) => _urlHelper = urlHelper;

        public void Map(Models.Car source, Car destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.CarId = source.CarId;
            destination.Cylinders = source.Cylinders;
            destination.Make = source.Make;
            destination.Model = source.Model;
            destination.Url = _urlHelper.AbsoluteRouteUrl(CarsControllerRoute.GetCar, new { CarId = source.CarId });
        }
    }
}
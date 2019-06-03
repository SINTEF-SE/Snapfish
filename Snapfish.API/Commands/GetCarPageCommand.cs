using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.API.Constants;
using Snapfish.API.API.Repositories;
using Snapfish.API.API.ViewModels;
using Car = Snapfish.API.API.Models.Car;

namespace Snapfish.API.API.Commands
{
    public class GetCarPageCommand : IGetCarPageCommand
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper<Car, ViewModels.Car> _carMapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;

        public GetCarPageCommand(
            ICarRepository carRepository,
            IMapper<Car, ViewModels.Car> carMapper,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper)
        {
            _carRepository = carRepository;
            _carMapper = carMapper;
            _httpContextAccessor = httpContextAccessor;
            _urlHelper = urlHelper;
        }

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            var cars = await _carRepository.GetPage(pageOptions.Page.Value, pageOptions.Count.Value, cancellationToken);
            if (cars == null)
            {
                return new NotFoundResult();
            }

            var (totalCount, totalPages) =
                await _carRepository.GetTotalPages(pageOptions.Count.Value, cancellationToken);
            var carViewModels = _carMapper.MapList(cars);
            var page = new PageResult<ViewModels.Car>
            {
                Count = pageOptions.Count.Value,
                Items = carViewModels,
                Page = pageOptions.Page.Value,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            // Add the Link HTTP Header to add URL's to next, previous, first and last pages.
            // See https://tools.ietf.org/html/rfc5988#page-6
            // There is a standard list of link relation types e.g. next, previous, first and last.
            // See https://www.iana.org/assignments/link-relations/link-relations.xhtml
            _httpContextAccessor.HttpContext.Response.Headers.Add(
                "Link",
                GetLinkValue(page));

            return new OkObjectResult(page);
        }

        private string GetLinkValue(PageResult<ViewModels.Car> page)
        {
            var values = new List<string>(4);

            if (page.HasNextPage)
            {
                values.Add(GetLinkValueItem("next", page.Page + 1, page.Count));
            }

            if (page.HasPreviousPage)
            {
                values.Add(GetLinkValueItem("previous", page.Page - 1, page.Count));
            }

            values.Add(GetLinkValueItem("first", 1, page.Count));
            values.Add(GetLinkValueItem("last", page.TotalPages, page.Count));

            return string.Join(", ", values);
        }

        private string GetLinkValueItem(string rel, int page, int count)
        {
            var url = _urlHelper.AbsoluteRouteUrl(
                CarsControllerRoute.GetCarPage,
                new PageOptions {Page = page, Count = count});
            return $"<{url}>; rel=\"{rel}\"";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using HATEOASWebService.Data.Entities;
using HATEOASWebService.Data.Models;
using HATEOASWebService.Helpers;
using HATEOASWebService.ResourceParameters;
using HATEOASWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HATEOASWebService.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(ILibraryRepository libraryRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _libraryRepository = libraryRepository ?? throw new ArgumentNullException(nameof(libraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type) =>
            type switch
            {
                ResourceUriType.PreviousPage => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    }),
                ResourceUriType.NextPage => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    }),
                ResourceUriType.Current => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    }),
                _ => Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    }),
            };

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields = null)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(
                    Url.Link("GetAuthor", new { authorId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(new LinkDto(
                    Url.Link("GetAuthor", new { authorId, fields }),
                    "self",
                    "GET"));
            }

            links.Add(new LinkDto(
                Url.Link("DeleteAuthor", new { authorId }),
                "delete-author",
                "DELETE"));

            links.Add(new LinkDto(
                Url.Link("CreateCourseForAuthor", new { authorId }),
                "create-course-for-author",
                "POST"));

            links.Add(new LinkDto(
                Url.Link("GetCoursesForAuthor", new { authorId }),
                "get-courses-for-author",
                "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "next-page", "GET"));
            }
            if(hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previous-page", "GET"));
            }

            return links;
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevous);

            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorsResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"]);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorId, string fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }
            var author = _libraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            var links = CreateLinksForAuthor(authorId, fields);

            var linkedResourceToReturn = _mapper.Map<AuthorDto>(author).ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);
            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            _libraryRepository.Save();
            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id);
            var linkedResourceToReturn = authorToReturn.ShapeData() as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtAction(nameof(GetAuthor), new { authorId = authorToReturn.Id }, linkedResourceToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorFromRepo);
            _libraryRepository.Save();

            return NoContent();
        }
    }
}

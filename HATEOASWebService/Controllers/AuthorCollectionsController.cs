using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HATEOASWebService.Data.Entities;
using HATEOASWebService.Data.Models;
using HATEOASWebService.Helpers;
using HATEOASWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HATEOASWebService.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository ?? throw new ArgumentNullException(nameof(libraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({ids})", Name = "GetCollectionForAuthor")]
        public IActionResult GetAuthorCollection(
            [FromRoute]
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libraryRepository.GetAuthors(ids);
            if(ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            
            foreach(var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }
            _libraryRepository.Save();

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",", authorCollectionToReturn.Select(x => x.Id));
            return CreatedAtAction(nameof(GetAuthorCollection), new { ids = idsAsString }, authorCollectionToReturn);
        }
    }
}

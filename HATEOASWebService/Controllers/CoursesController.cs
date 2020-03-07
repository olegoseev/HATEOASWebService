using System;
using System.Collections.Generic;
using AutoMapper;
using HATEOASWebService.Data.Entities;
using HATEOASWebService.Data.Models;
using HATEOASWebService.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HATEOASWebService.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository ?? throw new ArgumentNullException(nameof(libraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet(Name = "GetCoursesForAuthor")]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courses = _libraryRepository.GetCourses(authorId);

            return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
        }

        [HttpGet("{courseId}")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _libraryRepository.GetCourse(authorId, courseId);

            if(course == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpPost(Name = "CreateCourseForAuthor")]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Course>(course);
            _libraryRepository.AddCourse(authorId, courseEntity);
            _libraryRepository.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            return CreatedAtAction(nameof(GetCourseForAuthor),
                new { authorId, courseId = courseToReturn.Id }, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _libraryRepository.GetCourse(authorId, courseId);

            if(courseForAuthorFromRepo == null)
            {
                var courseToAdd = _mapper.Map<Course>(course);
                courseToAdd.Id = courseId;
                _libraryRepository.AddCourse(authorId, courseToAdd);
                _libraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtAction(
                    nameof(GetCourseForAuthor),
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }

            // update steps
            // 1. map the entity to a CourseForUpdateDto
            // 2. apply the updated field values to that dto
            // 3. mapt the CourseForUpdateDto back to an entity
            // all the steps completed by one statement
            _mapper.Map(course, courseForAuthorFromRepo);

            _libraryRepository.UpdateCourse(courseForAuthorFromRepo);

            _libraryRepository.Save();

            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthro(
            Guid authorId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if(! _libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _libraryRepository.GetCourse(authorId, courseId);

            if(courseForAuthorFromRepo == null)
            {
                // create course if it dosent exists
                var courseDto = new CourseForUpdateDto();

                patchDocument.ApplyTo(courseDto, ModelState);
                if(!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }
                
                var courseToAdd = _mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;
                _libraryRepository.AddCourse(authorId, courseToAdd);
                _libraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtAction(nameof(GetCourseForAuthor),
                    new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);

            // if error occured during serialization ModelState will filled with an error
            // without ModelState the method ApplyTo will throw an error
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if(!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseForAuthorFromRepo);

            _libraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _libraryRepository.Save();
            return NoContent();
        }


        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _libraryRepository.GetCourse(authorId, courseId);

            if(courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteCourse(courseForAuthorFromRepo);
            _libraryRepository.Save();
            return NoContent();
        }

        // override ValidationProblem to return status code 422 instead of 400
        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}

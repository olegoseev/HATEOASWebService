﻿using System;
using System.Collections.Generic;
using HATEOASWebService.Data.Entities;
using HATEOASWebService.Helpers;
using HATEOASWebService.ResourceParameters;

namespace HATEOASWebService.Services
{
    public interface ILibraryRepository
    {
        IEnumerable<Course> GetCourses(Guid authorId);
        Course GetCourse(Guid authorId, Guid courseId);
        void AddCourse(Guid authorId, Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters);
        IEnumerable<Author> GetAuthors();
        Author GetAuthor(Guid authorId);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        bool Save();
    }
}

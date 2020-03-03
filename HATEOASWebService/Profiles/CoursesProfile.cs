using AutoMapper;


namespace HATEOASWebService.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile()
        {
            CreateMap<Data.Entities.Course, Data.Models.CourseDto>();
            CreateMap<Data.Models.CourseForCreationDto, Data.Entities.Course>();
            CreateMap<Data.Models.CourseForUpdateDto, Data.Entities.Course>();
            CreateMap<Data.Entities.Course, Data.Models.CourseForUpdateDto>();
        }
    }
}

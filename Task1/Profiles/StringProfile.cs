using AutoMapper;
using Task1.DTOs;
using Task1.Models;

namespace Task1.Profiles
{
    public class StringProfile: Profile
    {

        public StringProfile()
        {
            CreateMap<StringProperty, StringPropertyDto>().ReverseMap();

            
        }
    }
}

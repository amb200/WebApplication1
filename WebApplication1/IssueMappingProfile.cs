namespace WebApplication1
{
    using AutoMapper;
    using System.Diagnostics.CodeAnalysis;
    using WebApplication1.Entities;
    using WebApplication1.Models;

    public class IssueMappingProfile : Profile
    {
        [ExcludeFromCodeCoverage]
        public IssueMappingProfile()
        {
            CreateMap<Issue, Issue>()
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore());

            CreateMap<IssueInput, Issue>();
            CreateMap<IssueBulkUpdateInput, Issue>();


        }
    }

}

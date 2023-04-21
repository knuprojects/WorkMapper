﻿using Mapster;
using Vacancies.Core.Common.Queries.Vacancies;
using Vacancies.Core.Entities;

namespace Vacancies.Core.Common.Mapper
{
    public class ResultsMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<VacancyResponse, Vacancy>()
                .RequireDestinationMemberSource(true);
        }
    }
}

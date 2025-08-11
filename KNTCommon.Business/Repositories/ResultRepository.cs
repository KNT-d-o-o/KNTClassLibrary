using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Repositories
{
    public class ResultRepository
    {
        public enum ResultTypes
        {
            Unknown = 0,
            OK = 1,
            Impregnation = 2,
            Scrap = 3,
            Error = 999
        }

        List<Results> Results = new(); // TODO temp solution put in caching

        public List<ResultsDTO> ResultsWithEmptyFilter = new();
        public static int ResultIdEmpty = 987987987;

        readonly Localization _localization;
        readonly IMapper _autoMapper;

        private readonly Tools t = new();

        public ResultRepository(Localization localization, IMapper automapper)
        {
            _localization = localization;
            _autoMapper = automapper;

            Init();
        }

        public void Init()
        {
            using var context = new EdnKntControllerMysqlContext();
            Results = context.Results.ToList();

            SetResultsWithEmptyFilter();
        }

        void SetResultsWithEmptyFilter()
        {
            var r = _autoMapper.Map<List<ResultsDTO>>(Results);

            ResultsWithEmptyFilter = r
                            .GroupBy(o => o.ResultDescription)
                            .Select(g => g
                                .OrderBy(o => string.IsNullOrEmpty(o.ResultDescriptionLang) ? 1 : 0) // full first
                                .First()
                            )
                            .ToList();

            ResultsWithEmptyFilter.Insert(0, new ResultsDTO() { ResultId = ResultIdEmpty, ResultDescription = "..." });

            AddAdditionalTranslations();
            SetResultsTranslations();
        }

        void AddAdditionalTranslations()
        {
            foreach (var result in ResultsWithEmptyFilter)
                if (result.ResultDescriptionLang is null)
                {
                    result.ResultDescriptionLang = $"ResultId_{result.ResultId}";
                    _localization.AddOrUpdateTranslation(result.ResultDescriptionLang, result.ResultDescription ?? "", result.ResultDescription ?? "", result.ResultDescription ?? "", result.ResultDescription ?? "", result.ResultDescription ?? "");
                }
        }

        void SetResultsTranslations()
        {
            foreach (var result in ResultsWithEmptyFilter)
                result.ResultTranslatedDescription = _localization.Get(result.ResultDescriptionLang!);
        }

        public string GetColor(int resultId)
        {
            var color = Results.Where(x => x.ResultId == resultId).Select(x => x.ResultColour).First();



            return color;
        }

        public string GetColor(ResultTypes resultType)
        {
            return GetColor((int)resultType);
        }
    }
}

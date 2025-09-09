using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KNTCommon.Business.Models
{
    public class Column()
    {
        public required string FilterColumn { get; set; }
        public required object FilterParam { get; set; }
        public required string FilterCondition { get; set; } = string.Empty;
    }

    public class SearchPageArgs()
    {
        public List<Column> columns { get; set; } = new();
        public int Skip { get; set; }
        public int Take { get; set; } = 50;


        //Skip(args.Skip.Value).Take(args.Top.Value)
        public string OrderBy { get; set; } = string.Empty;
        public bool OrderByAsc { get; set; } = true; // ASC - true; DESC - false

        public SearchPageArgs DeepCopy()
        {
            var search = new SearchPageArgs(){
                Skip = this.Skip,
                OrderBy = this.OrderBy,
                Take = this.Take
            };

            foreach(var column in this.columns)
                search.columns.Add(new Column() { FilterColumn = column.FilterColumn, FilterCondition = column.FilterCondition, FilterParam = column.FilterParam});

            return search;
        }

    }
}

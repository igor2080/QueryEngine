using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryEngine
{
    public class QueryEngine
    {
        const int FROM_LENGTH = 5;//lengths of the words and the following space
        const int WHERE_LENGTH = 6;
        const int SELECT_LENGTH = 7;
        const string STORAGE_NAME = "QueryEngine.Data";

        private readonly Data _data = Data.Instance;

        public void PrintResult(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            //split the query into 3 parts
            string select = query.Substring(query.IndexOf("select") + SELECT_LENGTH).Trim();
            query = query.Split("select")[0];
            string where = query.Substring(query.IndexOf("where") + WHERE_LENGTH).Trim();
            query = query.Split("where")[0];
            string from = query.Substring(query.IndexOf("from") + FROM_LENGTH).Trim();
            //find the right table from 'from'
            var dataArray = GetSpecificTableData(from);
            //filter by conditions from 'where'
            var filteredData = GetFileteredData(dataArray, where);
            //get only the required properties in 'select', already using the filtered data, also print it
            var specificPropertiesData = PrintSpecificPropertiesData(filteredData, select);            
        }

        private List<List<object>> PrintSpecificPropertiesData(List<IData> filteredData, string select)
        {
            if(string.IsNullOrWhiteSpace(select) || filteredData.Count == 0)
            {
                Console.WriteLine("no select statement provided");
                return null;
            }

            string[] fields = select.Split(',', StringSplitOptions.TrimEntries);
            //retrieve the correct properties, make a list out of them
            var results = filteredData
                .Select(x => x.GetType().GetProperties()
                    .Where(y => fields.Contains(y.Name))
                    .Select(z => z.GetValue(x))
                    .ToList())
                .ToList();

            foreach (var field in fields)//query headers
            {
                Console.Write("{0,-30}\t",field);
            }

            Console.WriteLine();

            foreach (var result in results)//query result
            {
                foreach (var item in result)
                {
                    Console.Write("{0,-30}\t", item);
                }
                Console.WriteLine();
            }

            return results;
        }

        private List<IData> GetSpecificTableData(string from)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                Console.WriteLine("no from statement provided");
            }

            switch (from.ToLower())//assuming table names are case insensitive
            {
                case "users":
                    return _data.Users;
                case "orders":
                    return _data.Orders;
            }
            //no table found
            return null;
        }

        private List<IData> GetFileteredData(List<IData> dataArray, string where)
        {
            if (string.IsNullOrWhiteSpace(where) || dataArray.Count==0)
            {
                return null;
            }

            //separate into one condition per string
            string[] splitConditions = where.Split(new string[] { " and ", " or " }, StringSplitOptions.None);
            //find out if theyre an 'or' or an 'and' condition, first one doesn't matter
            string[] conditionType = Regex.Matches(where, "(and) | (or)").OfType<Match>().Select(m => m.Groups[0].Value.Trim()).ToArray();
            //condition total
            int conditions = splitConditions.Length;

            string sign = Regex.Split(splitConditions[0], "(<=)|(>=)|(<)|(>)|(=)")[1];
            string field = splitConditions[0].Split(sign, StringSplitOptions.TrimEntries)[0].Trim('\'');
            string value = splitConditions[0].Split(sign, StringSplitOptions.TrimEntries)[1].Trim('\'');
            //do the first filter without any 'and' or 'or' checks
            var conditionResult = GetConditionResult(dataArray, sign, field, value);
            var filteredData = conditionResult;

            for (int i = 1; i < conditions; i++)
            {//remaining condition outcomes depend on where theyre an "and" or an "or" condition
                sign = Regex.Split(splitConditions[i], "(<=)|(>=)|(<)|(>)|(=)")[1];
                field = splitConditions[i].Split(sign, StringSplitOptions.TrimEntries)[0].Trim('\'');
                value = splitConditions[i].Split(sign, StringSplitOptions.TrimEntries)[1].Trim('\'');
                conditionResult = GetConditionResult(dataArray, sign, field, value);

                if (conditionType[i - 1].Contains("and"))
                {
                    filteredData = filteredData.Intersect(conditionResult);
                }
                else // or
                {
                    filteredData = filteredData.Union(conditionResult);
                }
            }

            return filteredData.ToList();
        }

        private IEnumerable<IData> GetConditionResult(List<IData> dataArray, string sign, string field, string value)
        {
            if (string.IsNullOrWhiteSpace(sign) || string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (sign == "=")
            {//equal sign is checked simply with string comparison
                return dataArray.Where(x => x.GetType().GetProperty(field).GetValue(x).ToString() == value);
            }
            else
            {
                try
                {//in case the sign is something else, try converting into a number and do the condition
                    double numValue = double.Parse(value);
                    switch (sign)
                    {//(<=)|(>=)|(<)|(>)
                        case "<=":
                            return dataArray.Where(x => Convert.ToDouble(x.GetType().GetProperty(field).GetValue(x)) <= numValue);
                        case ">=":
                            return dataArray.Where(x => Convert.ToDouble(x.GetType().GetProperty(field).GetValue(x)) >= numValue);
                        case "<":
                            return dataArray.Where(x => Convert.ToDouble(x.GetType().GetProperty(field).GetValue(x)) < numValue);
                        case ">":
                            return dataArray.Where(x => Convert.ToDouble(x.GetType().GetProperty(field).GetValue(x)) > numValue);
                    }
                }
                catch
                {//left operand is not a number, the condition is invalid
                    return null;
                }
            }
            //unrecognized sign
            return null;
        }
    }
}

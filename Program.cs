using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QueryEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            QueryEngine queryEngine = new QueryEngine();
            queryEngine.PrintResult("from Users where Email = 'jobs@ravendb.net' or Age >= 18 and Age <= 99 select FullName, Email");
        }
    }

}

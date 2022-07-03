using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryEngine
{
    internal class Data
    {
        private static Data instance = null;

        public static Data Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Data();
                }
                return instance;
            }
        }

        public List<IData> Users { get; }
        public List<IData> Orders { get; }
        public Data()
        {
            Users = new List<IData>
            {
                new User(){Age=5, Email="user1@domain.com",FullName="User Zero"},
                new User(){Age=20, Email="user1@domain.com",FullName="User One"},
                new User(){Age=30, Email="user2@domain.com",FullName="User Two"},
                new User(){Age=40, Email="user3@domain.com",FullName="User Three"},
                new User(){Age=50, Email="user4@domain.com",FullName="User Four"},
                new User(){Age=125, Email="jobs@ravendb.net",FullName="User Five"},
            };
            Orders = new List<IData>
            {
                new Order(){Id=10, Description="Order #10",Name = "Food"},
                new Order(){Id=20, Description="Order #20",Name = "Drinks"},
                new Order(){Id=30, Description="Order #30",Name = "Candy"},
            };
        }
    }
}

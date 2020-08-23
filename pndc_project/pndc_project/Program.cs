using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp;
using System.Threading;

namespace pndc_project
{
    class Program
    {
        static double uoh=10;
        static int accident_count = 0;
        static int hazard_count = 0;
        static int crime_count = 0;
        static int other_count = 0;
        static bool incident_data_fetched_from_FB = false;
  
        static IFirebaseClient client;
        static void Main(string[] args)
        {

            IFirebaseConfig config = new FirebaseConfig();

            config.BasePath = "https://fyp1-f5c3d.firebaseio.com/";
            config.AuthSecret = "uzzEKiizJ1kOvi8ZFEB3YkV6msZfwL4J2I2jUako";
            client = new FirebaseClient(config);
            if (client != null)
            {
                // Console.WriteLine("connection established");
            }
            

            incident_type_count();
   

            while (incident_data_fetched_from_FB == false)
            {

            }

            Console.WriteLine("# of acidents " + accident_count);
            Console.WriteLine("# of hazards " + hazard_count);
            Console.WriteLine("# of crimes " + crime_count);
            Console.WriteLine("other types of incidents " + other_count);
            int total = other_count + crime_count + hazard_count + accident_count;
            Console.WriteLine("\ntotal incidents incidents " + total);

            Console.WriteLine();


            int[] incidents = new int[4];
            incidents[0] = accident_count;
            incidents[1] = hazard_count;
            incidents[2] = crime_count;
            incidents[3] = other_count;

            List<Task<double>> taskList = new List<Task<double>>();
            for (int i = 0; i < incidents.Length; i++)
            {
                Task<double> mTask = new Task<double>(get_micro_values, incidents[i]);
                taskList.Add(mTask);
            }

            for (int i = 0; i < taskList.Count; i++)
            {
                taskList[i].Start();
            }

            Task.WaitAll(taskList.ToArray());

            double defuz = 0;
            for (int i = 0; i < taskList.Count; i++)
            {
                Task<double> mTask = taskList[i];
                defuz += mTask.Result;
            }
            defuz /= 4;
            Console.WriteLine("\nsafety index value {0}\n",defuz * 100);
            if (defuz == 1)
                Console.WriteLine("totally safe");
            else if (defuz >= 0.9)
                Console.WriteLine("slighty unsafe");
            else if (defuz >= 0.7)
                Console.WriteLine("unsafe");
            else if (defuz > 0)
                Console.WriteLine("extremely unsafe");
            else
                Console.WriteLine("totally unsafe");

            Console.WriteLine();
        }
        public static double get_micro_values(object variable)
        {
            int var_ = (int)variable;
            double micro_var = (uoh - var_);
            var_ = (var_ == 0) ? 1 : var_;
            micro_var /= (10 * var_);
            var_ = (var_ == 0) ? 1 : var_;
            Console.WriteLine("value from distributed method " + var_ * micro_var);
            return var_ * micro_var;
        }

        public static async void incident_type_count()
        {

            var fetch = await client.GetAsync("incident");
            Dictionary<string, incident> usersd = fetch.ResultAs<Dictionary<string, incident>>();

            foreach (var user in usersd)
            {
                accident_count = (user.Value.subtype == "accident") ? ++accident_count : accident_count;
                hazard_count = (user.Value.subtype == "hazard") ? ++hazard_count : hazard_count;
                crime_count = (user.Value.subtype == "crime") ? ++crime_count : crime_count;
                other_count = (user.Value.subtype == "others") ? ++other_count : other_count;
            }
            incident_data_fetched_from_FB = true;
        }


        class incident
        {
            public string subtype { get; set; }
        }

    }
}

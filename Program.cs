using System;
using System.Collections.Generic;
using System.IO;

namespace exam
{

    struct Task
    {

        public string Name { get; set; }
        public string Project { get; set; }
        public int Priority { get; set; }

        public Task(string name, string project, int priority)
        {
            Name = name;
            Project = project;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"{Name}\t#{Project}\tp{Priority}";
        }

    }

    struct DBManager
    {

        const string SupportedCharacters = "abcdefghijklmonpqrstuvwxyz";
        static Random gen = new Random();
        Dictionary<string, Task> IdToTask; 
        string FileName { get; set; }


        void LoadFile()
        {
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] line = sr.ReadLine().Split('\t');
                        IdToTask.Add(line[0], new Task(line[1], line[2].Substring(1), Convert.ToInt32(line[3].Substring(1))));
                    }
                }
            }
        }

        public DBManager(string fileName)
        {
            FileName = fileName;
            IdToTask = new Dictionary<string, Task>();
            this.LoadFile();
        }

        string GenerateId()
        {
            int n = SupportedCharacters.Length;
            string res = "";
            int count = 0;
                                                                                  // 17576 = 26 ^ 3 = количество различных id из 3х
                                                                                  // строчных английских букв
            while (res.Length == 0 || IdToTask.ContainsKey(res)) // условие с count - костыль, фу так писать
            {
                res = ""; // Поскольку не стираются старые данные в res
                while (res.Length < 3)
                {
                    res += SupportedCharacters[gen.Next(n)];
                }
                count++;
            }

            return res;
        }

        static string GetFileLine(string id, Task task)
        {
            return id + "\t" + task.ToString();
        }

        static string GetAlignedLine(string id, Task task)
        {
            return $"{id}\t{task.Name,-10}\t{'#' + task.Project,-10}\tp{task.Priority,-1}";
        }
        void WriteTaskToFile(string id, Task task)
        {
            using (StreamWriter sw = File.AppendText(FileName))
            {
                sw.WriteLine(GetFileLine(id, task));
            }
        }

        void WriteWholeBaseToFile()
        {
            using (StreamWriter sw = new StreamWriter(FileName))
            {
                foreach (string id in IdToTask.Keys)
                {
                    sw.WriteLine(GetFileLine(id, IdToTask[id]));
                }
            }
        }

        public void AddTask(string name, string project, int priority)
        {
            Task task = new Task(name, project, priority);
            string id = GenerateId();
            IdToTask.Add(id, task);
            WriteTaskToFile(id, task);
        }

        public void Done(string id)
        {
            bool found = IdToTask.ContainsKey(id);
            if (found) {
                IdToTask.Remove(id);
                WriteWholeBaseToFile();
                Console.WriteLine("Задача удалена");
            } else {
                Console.WriteLine("Задача не обнаружена");
            }
        }

        public void Change(string id, string name, string project, int priority) {
            bool found = IdToTask.ContainsKey(id);
            if (found) {
                IdToTask[id] = new Task(name, project, priority);
                WriteWholeBaseToFile();
                Console.WriteLine("Задача изменена");
            } else {
                Console.WriteLine("Задача не обнаружена");
            }
        }

        // оффтоп: генераторы
        public IEnumerable<string> PrintAllIterate()
        {
            foreach (string id in IdToTask.Keys)
            {
                yield return GetAlignedLine(id, IdToTask[id]);
            }
        }

        public void PrintAll()
        {
            foreach (string id in IdToTask.Keys)
            {
                Console.WriteLine(GetAlignedLine(id, IdToTask[id]));
            }
        }

        public void PrintProject(string project)
        {
            foreach (string id in IdToTask.Keys)
            {
                if (IdToTask[id].Project == project)
                {
                    Console.WriteLine(GetAlignedLine(id, IdToTask[id]));
                }
            }
        }


    }
    class Program
    {

        delegate void Command(string[] args);
        static void Main(string[] args)
        {
            DBManager dbm = new DBManager("todo.txt");
            Dictionary<string, Command> cmdToLambda = new Dictionary<string, Command>();


            Console.WriteLine("X-pech Simple Todo.txt-like task console manager");
            Console.WriteLine("commands: list, lspr <project>, done <id>, add <name> <project> <priority>, change <id> <params>");

            cmdToLambda.Add("list", (string[] args) =>
            {
                dbm.PrintAll();
            });

            cmdToLambda.Add("add", (string[] args) =>
            {
                try
                {
                    dbm.AddTask(args[1], args[2], Convert.ToInt32(args[3]));
                }
                catch (FormatException)
                {
                    Console.WriteLine("Введите корректный приоритет");
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Недостаточно аргументов");
                }
            });

            cmdToLambda.Add("done", (string[] args) =>
            {
                try
                {
                    dbm.Done(args[1]);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Недостаточно аргументов");
                }
            });
            cmdToLambda.Add("lspr", (string[] args) =>
            {
                try
                {
                    dbm.PrintProject(args[1]);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Недостаточно аргументов");
                }
            });
            cmdToLambda.Add("iterator", (string[] args) =>
            {
                foreach (string line in dbm.PrintAllIterate())
                {
                    Console.WriteLine(line);
                }
            });
            cmdToLambda.Add("change", (string[] args) =>
            {
                try {
                    dbm.Change(args[1], args[2], args[3], Convert.ToInt32(args[4]));
                } catch (IndexOutOfRangeException) {
                    Console.WriteLine("Недостаточно аргументов");
                } catch (FormatException) {
                    Console.WriteLine("Задайте корректный приоритет");
                }
            });

            string line = "";
            while (line != "exit")
            {
                line = Console.ReadLine();
                string[] arguments = line.Split('\t');
                line = arguments[0];

                if (line == "exit")
                {
                    return;
                }

                Command command = null;

                bool found = cmdToLambda.TryGetValue(line, out command);

                if (!found)
                {
                    Console.WriteLine("Введите корректную команду");
                }
                else
                {
                    command(arguments);
                }
            }

        }
    }
}

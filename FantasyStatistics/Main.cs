using System;
using System.IO;
using System.Data.SQLite;

namespace FantasyStatistics
{
    class Program
    {
        private static void UpdateDB()
        {
            String path = "Configuration.ini";

            if (!File.Exists(path))
            {
                Console.WriteLine("File {0} not found", path);
                Console.ReadLine();
                return;
            }

            Manager mng = Manager.ReadIniFile(path);
            mng.StartUpdateDB();

            Console.ReadLine();
        }
   
        static void Main(string[] args)
        {
            try
            {
                if (args.Length.Equals(0))
                {
                    String path = "Configuration.ini";

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("File {0} not found", path);
                        Console.ReadLine();
                        return;
                    }

                    Manager mng = Manager.ReadIniFile(path);

                    mng.SelectTopPlayersFor3Tours();

                    Console.ReadLine();

                    return;
                }

                foreach (var arg in args)
                {
                    if (arg.Equals("-updateDB"))
                    {
                        UpdateDB();

                        Console.Write("\n");

                        continue;
                    }

                    if (arg.Equals("-leaders"))
                    {
                        String path = "Configuration.ini";

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("File {0} not found", path);
                            Console.ReadLine();
                            return;
                        }

                        Manager mng = Manager.ReadIniFile(path);

                        mng.Leaders();

                        Console.Write("\n");

                        continue;
                    }

                    if (arg.Equals("man") || arg.Equals("-?"))
                    {
                        Console.WriteLine("Статистика fantasy-игроков\n");
                        Console.WriteLine("FantasyStatistics [-parameter]\n");
                        Console.WriteLine("Параметры:\n");
                        Console.WriteLine("     -topPlayers:s-f   Вывод топ-игроков за тур или определенный промежуток.");
                        Console.WriteLine("     -updateDB         Обновление базы данных.");
                        Console.WriteLine("     -leaders          Вывод лидеров турнира.");
                        Console.WriteLine("     man or -?         Вывод справки у консольной утилиты.");

                        Console.Write("\n");

                        continue;
                    }

                    if (arg.Substring(0, 12).Equals("-topPlayers:"))
                    {
                        String parameters = arg.Remove(0, 12);

                        int secondTour = Convert.ToInt32(parameters.Substring(0, parameters.IndexOf('-')));
                        int firstTour = Convert.ToInt32(parameters.Substring(parameters.IndexOf('-') + 1, parameters.Length - parameters.IndexOf('-') - 1));

                        String path = "Configuration.ini";

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("File {0} not found", path);
                            Console.ReadLine();
                            return;
                        }

                        Manager mng = Manager.ReadIniFile(path);
                        mng.SelectTopPlayers(secondTour, firstTour);

                        Console.Write("\n");

                        continue;
                    }
                }
            }
            catch (ArgumentOutOfRangeException argEx)
            {
                //...
            }
            catch (Exception ex)
            {
                Console.WriteLine("Проверьте правильность введенных данных в конфигурационном файле!\n");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("StackTrace:\n{0}", ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}

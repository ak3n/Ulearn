using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uLearn; 

namespace U13_Consistancy
{
	[Slide("Отложенные ошибки", "00e04195-0c84-401f-aee9-19b646eaa69b")]
	class S030_Отложенные_ошибки
	{
		//#video ma_bwcRxxmE
		/*
		## Заметки по лекции
		*/
        public class Transaction
        {
            public int DepartmentId;
            public double Profit;
        }
        public class UnsafeReportData
        {
            public int DepartmentsCount;
            public List<Transaction> Transactions = new List<Transaction>();
            public void PrintProfits()
            {
                var profits = new double[DepartmentsCount];
                foreach (var e in Transactions)
                    profits[e.DepartmentId] += e.Profit;
                for (int i = 0; i < DepartmentsCount; i++)
                    Console.WriteLine("{0,-10}{1}", i, profits[i]);
            }
        }
        public class UnsafeProgram
        {
            public static void Main()
            {
                var report = new UnsafeReportData
                {
                    DepartmentsCount = 2,
                    Transactions = {
                    new Transaction { DepartmentId=0, Profit=10000 },
                    new Transaction { DepartmentId=1, Profit=20000 },
                    new Transaction { DepartmentId=1, Profit=10000 }
                }
                };
                report.PrintProfits();

                //но что, если кто-то напишет так?
                report.DepartmentsCount = -1;
                //а в этом месте будут тысячи строк кода?
                report.PrintProfits();
                //отложенная ошибка - это ситуация, в которой 
                //сообщение об ошибке приходит не в том месте, в котором реально
                //было выполнено ошибочное действие.

            }

        }


        public class SafeReportData
        {
            //Делаем поле приватным
            private int departmentsCount;

            //и организуем для доступа к нему методы
            public int GetDepartmentsCount() { return departmentsCount; }
            public void SetDepartmentsCount(int value)
            {
                if (value < 0) throw new ArgumentException();
                departmentsCount = value;
            }

            public List<Transaction> Transactions = new List<Transaction>();
            public void PrintProfits()
            {
                var profits = new double[departmentsCount];
                foreach (var e in Transactions)
                    profits[e.DepartmentId] += e.Profit;
                for (int i = 0; i < departmentsCount; i++)
                    Console.WriteLine("{0,-10}{1}", i, profits[i]);
            }
        }

        public class Program
        {
            public static void MainX()
            {
                var report = new SafeReportData
                {
                    // departmentCount=2, -- так писать нельзя, ведь поле приватное
                    Transactions = {
                    new Transaction { DepartmentId=0, Profit=10000 },
                    new Transaction { DepartmentId=1, Profit=20000 },
                    new Transaction { DepartmentId=1, Profit=10000 }
                }
                };
                report.SetDepartmentsCount(2); //поэтому придется так.
                report.PrintProfits();

                //но что, если кто-то напишет так?
                //report.departmentsCount = -1; - так теперь написать нельзя
                report.SetDepartmentsCount(-1); //так написать можно, но ошибка возникнет сразу же.

                // [....]
                // Учтите, что в реальной разработке в этом месте может пройти несколько часов действий пользователя
                report.PrintProfits(); // и ошибка произойдет теперь не в самом конце, а в момент ее совершения.

                // Все прекрасно, но как написать report.departmentsCount++? Вот так:
                report.SetDepartmentsCount(report.GetDepartmentsCount() + 1);
                // Слегка неизящно.

                // То, что защита целостности выглядит извне сложно и неизящно 
                // мешает программисту ее использовать, и программы получаются плохими.
            }
        }
    }
}

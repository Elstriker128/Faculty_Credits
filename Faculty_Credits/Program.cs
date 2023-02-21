using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

namespace Faculty_Credits
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string Duom1 = "Duom1.txt";
            const string Duom2 = "Duom2.txt";
            const string Rez = "Rez.txt";
            Faculty FirstFaculty = InOutUtils.ReadFaculty(Duom1);
            Faculty SecondFaculty = InOutUtils.ReadFaculty(Duom2);

            Faculty FirstFiltered = TaskUtils.Select(FirstFaculty);
            Faculty SecondFiltered = TaskUtils.Select(SecondFaculty);

            FirstFiltered.Sort();
            SecondFiltered.Sort();

            if(File.Exists(Rez))
            {
                File.Delete(Rez);
            }
            InOutUtils.PrintFaculty(FirstFaculty, Rez, FirstFaculty.faculty);
            InOutUtils.PrintFaculty(SecondFaculty,Rez,SecondFaculty.faculty);

            InOutUtils.PrintFaculty(FirstFiltered,Rez,FirstFiltered.faculty);
            InOutUtils.PrintFaculty(SecondFiltered,Rez,SecondFiltered.faculty);

            if(FirstFiltered>SecondFiltered)
            {
                File.AppendAllText(Rez,$"Daugiau studentų yra: {FirstFiltered.faculty} fakulete");
            }
            else if(FirstFiltered < SecondFiltered)
            {
                File.AppendAllText(Rez, $"Daugiau studentų yra: {SecondFiltered.faculty} fakulete");
            }
            else if(FirstFiltered == SecondFiltered)
            {
                File.AppendAllText(Rez, "Abiejuose fakultetuose kreditus viršijusių žmonių yra vienodai");
            }
        }
    }
    class Student
    {
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public string Group { get; private set; }
        private int[] Credits { get; set; }

        public Student(string surname, string name, string group, int[] credits)
        {
            Surname = surname;
            Name = name;
            Group = group;
            Credits = credits;
        }
        public int Sum(int ii)
        {
            if(ii<Credits.Count())
            {
                return Credits[ii] + Sum(ii + 1);
            }
            else
            {
                return 0;
            }
        }
        public int[] GetCredits()
        {
            return this.Credits;
        }


        public static bool operator >(Student a, Student b)
        {
            if (a.Group != b.Group)
            {
                return string.Compare(a.Group, b.Group) > 0;
            }
            else if (a.Surname != b.Surname)
            {
                 return string.Compare(a.Surname, b.Surname) > 0;
            }
            else
            {
                return false;
            }
        }
        public static bool operator <(Student a, Student b)
        {
            if (a.Group != b.Group)
            {
                return string.Compare(a.Group, b.Group) < 0;
            }
            else if (a.Surname != b.Surname)
            {
                return string.Compare(a.Surname, b.Surname) < 0;
            }
            else
            {
                return false;
            }
        }

    }
    class Faculty
    {
        private List<Student> allStudents;
        public string faculty { get; private set; }
        public int creditCount { get; private set; }
        public Faculty()
        {
            allStudents = new List<Student>();
        }
        public Faculty(List<Student> AllStudents) :this()
        {
            foreach(Student student in AllStudents)
            {
                this.allStudents.Add(student);
            }
        }
        public Faculty(string faculty, int creditCount)
        {
            allStudents = new List<Student>();
            this.faculty = faculty;
            this.creditCount = creditCount;
        }
        public void Add(Student student)
        {
            allStudents.Add(student);
        }
        public int StudentCount()
        {
            return allStudents.Count();
        }
        public Student Get(int index)
        {
            return allStudents[index];
        }
        public void Sort()
        {
            for (int i = 0; i < allStudents.Count()-1; i++)
            {
                for (int j = i+1; j < allStudents.Count(); j++)
                {
                    if (allStudents[i] > allStudents[j])
                    {
                        var cur = allStudents[i];
                        allStudents[i] = allStudents[j];
                        allStudents[j] = cur;
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Faculty faculty &&
                   EqualityComparer<List<Student>>.Default.Equals(allStudents, faculty.allStudents);
        }

        public override int GetHashCode()
        {
            return -233307388 + EqualityComparer<List<Student>>.Default.GetHashCode(allStudents);
        }

        public static bool operator >(Faculty a, Faculty b)
        {
            return a.StudentCount() > b.StudentCount();
        }
        public static bool operator <(Faculty a, Faculty b)
        {
            return a.StudentCount() < b.StudentCount();
        }
        public static bool operator ==(Faculty a, Faculty b)
        {
            return a.StudentCount() == b.StudentCount();
        }
        public static bool operator !=(Faculty a, Faculty b)
        {
            return a.StudentCount() != b.StudentCount();
        }

    }
    class TaskUtils
    {
        public static Faculty Select(Faculty faculty)
        {
            Faculty filtered = new Faculty(faculty.faculty,faculty.creditCount);
            int maxCredits = faculty.creditCount;
            for (int i = 0; i < faculty.StudentCount(); i++)
            {
                Student current = faculty.Get(i);
                if(current.Sum(0)>maxCredits)
                {
                    filtered.Add(current);
                }
            }
            return filtered;
        }
    }
    class InOutUtils
    {
        public static Faculty ReadFaculty(string FileName)
        {
            string[] Lines = File.ReadAllLines(FileName);

            string[] first = Regex.Split(Lines[0], ", ");
            //string[] first = Lines[0].Split(',');
            Faculty allStudents = new Faculty(first[0], int.Parse(first[1]));

            foreach(string line in Lines.Skip(1))
            {
                string[] values = Regex.Split(line, ", ");
                string surname = values[0];
                string name = values[1];
                string group = values[2];

                int[] credits = new int[values.Count()-3];
                for (int i = 0; i < values.Count()-3; i++)
                {
                    credits[i] = int.Parse(values[3+i]);
                }

                Student current = new Student(surname,name,group,credits);
                allStudents.Add(current);
            }
            return allStudents;
        }
        public static void PrintFaculty(Faculty faculty, string fileName, string header)
        {
            string[] Lines = new string[faculty.StudentCount()+7];
            Lines[0] = String.Format(new string('-', 75));
            Lines[1] = String.Format(" {0,-15}", header);
            Lines[2] = String.Format(new string('-', 75));
            Lines[3] = String.Format(" {0,-15} {1,-15} {2,-10} {3,-15} ", "Surname", "Name", "Group", "Credits");
            Lines[4] = String.Format(new string('-', 75));
            for (int i = 0; i < faculty.StudentCount(); i++)
            {
                Student current = faculty.Get(i);
                int[] credits = current.GetCredits();
                Lines[5 + i] = String.Format(" {0,-15} {1,-15} {2,-10} ", current.Surname, current.Name, current.Group);
                for (int j = 0; j < credits.Count(); j++)
                {
                    Lines[5+i] += String.Format($" {credits[j],-2} ");
                }
            }
            Lines[faculty.StudentCount()+5] = String.Format(new string('-', 75));
            File.AppendAllLines(fileName, Lines, Encoding.UTF8);

        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Yeast.Test
{
    [HasDerivedClasses(typeof(Student), typeof(Professor))]
    public abstract class Person
    {
        public string name;
        public int age;
        private float height;

        public Person(string name, int age, float height)
        {
            this.name = name;
            this.age = age;
            this.height = height;
        }

        public void SetHeight(float height)
        {
            this.height = height;
        }

        public override string ToString()
        {
            return name + " " + age + " " + height;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Person p = (Person)obj;
            return (name == p.name) && (age == p.age) && (height == p.height);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(name, age, height);
        }
    }

    [IsDerivedClass("Student")]
    public class Student : Person
    {
        public int studentID;

        public Student() : base("Unknown", 0, 0)
        {
            studentID = 0;
        }

        public Student(string name, int age, float height, int studentID) : base(name, age, height)
        {
            this.studentID = studentID;
        }

        public override string ToString()
        {
            return base.ToString() + " " + studentID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Student s = (Student)obj;
            return base.Equals(s) && (studentID == s.studentID);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(base.GetHashCode(), studentID);
        }
    }

    [IsDerivedClass("Professor")]
    public class Professor : Person
    {
        public int professorID;

        public Professor() : base("Unknown", 0, 0)
        {
            professorID = 0;
        }

        public Professor(string name, int age, float height, int professorID) : base(name, age, height)
        {
            this.professorID = professorID;
        }

        public override string ToString()
        {
            return base.ToString() + " " + professorID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Professor s = (Professor)obj;
            return base.Equals(s) && (professorID == s.professorID);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(base.GetHashCode(), professorID);
        }
    }

    public class School
    {
        public string name;
        public List<Student> students;
        public List<Person> people;
        public School() { }

        public School(string name, List<Student> students, List<Person> people)
        {
            this.name = name;
            this.students = students;
            this.people = people;
        }

        public override string ToString()
        {
            string result = name + ":\n";
            foreach (Person student in students)
            {
                result += student.ToString() + "\n";
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            School s = (School)obj;
            return (name == s.name) && (students == s.students || students.SequenceEqual(s.students)) && (people == s.people || people.SequenceEqual(s.people));
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(name, students);
        }
    }
}

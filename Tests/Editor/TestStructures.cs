using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yeast.Test
{
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

    public class School
    {
        public string name;
        public List<Person> students;
        public School() { }

        public School(string name)
        {
            this.name = name;
            students = new List<Person>();
        }

        public void AddStudent(Person student)
        {
            students.Add(student);
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
            return (name == s.name) && students.SequenceEqual(s.students);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(name, students);
        }
    }
}

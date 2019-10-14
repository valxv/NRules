using System;

namespace TestApp
{
    public class TestFact
    {
        public string PropOne { get; set; }
        public string PropTwo { get; set; }

        public void ShowProperties()
        {
            Console.WriteLine($"Prop1: {PropOne}; Prop2: {PropTwo}");
        }
    }
}

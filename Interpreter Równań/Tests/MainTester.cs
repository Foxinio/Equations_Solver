using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Równań.Tests {
    class MainTester {

        public static void Main(string[] args) {
            string[] data = importEquations("DecoderTests.txt");
            CheckModule(data, DecoderTester.testEquationDecoder);
        }

        private static string[] importEquations(string path) {
            string[] resultArray = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < resultArray.Length; i++) {
                resultArray[i] = resultArray[i].Trim();
            }
            return resultArray;
        }

        public static bool CheckModule(string[] equationsToBeTested, Func<string,bool> tester) {
            int passedTests = 0;
            foreach (var item in equationsToBeTested) {
                bool testResult = tester(item);
                passedTests += testResult ? 1 : 0;
                DisplayTestsResults(item, testResult);
            }
            DisplayCheckupSummary(passedTests, equationsToBeTested.Length);
            return passedTests == equationsToBeTested.Length;
        }

        private static void DisplayCheckupSummary(int passedTests, int testAmount) {
            var defaultColor = Console.ForegroundColor;
            Console.Write("Summary. Tests passed: ");
            Console.ForegroundColor = passedTests == testAmount ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(passedTests + " out of " + testAmount);
            Console.ForegroundColor = defaultColor;
        }

        private static void DisplayTestsResults(string testedInput, bool testResult) {
            var defaultColor = Console.ForegroundColor;
            Console.Write("Test for equation: \"" + testedInput + "\" is ");
            if (testResult) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("POSITIVE");
            }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("NEGATIVE");
            }
            Console.ForegroundColor = defaultColor;
        }

        public static void SaveErrorRaport(Exception exception, string inputEquation) {
            string path = "TestRaport.txt";
            using (System.IO.StreamWriter file = System.IO.File.Exists(path) 
                                               ? System.IO.File.AppendText(path) 
                                               : System.IO.File.CreateText(path)) 
            {
                file.WriteLine(inputEquation + "\t" + exception.Message);
                file.Close();
            }
        }

    }
}

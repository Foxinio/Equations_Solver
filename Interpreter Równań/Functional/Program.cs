using System;
using System.Collections.Generic;

namespace Interpreter_Równań {
    class Program {

        private static string input;
        private static bool debugVersion;
        private static ChangeHandler output;
        
        static void Main(string[] args) {
            ExecuteNormalProgram();
        }


        private static void ExecuteNormalProgram() {
            while (true) {
                Console.Write("Do you want to enter debug mode? <Y/N> : ");
                var answer = Console.ReadLine();
                if (answer == "Y") {
                    debugVersion = true;
                    Console.Clear();
                    break;
                }
                else if (answer == "N") {
                    debugVersion = false;
                    Console.Clear();
                    break;
                }
                Console.Clear();
            }
            while (true) {
                input = Console.ReadLine();
                InterpretAndSimplifyInput();
            }
        }

        private static void InterpretAndSimplifyInput() {
            ChangeHandler output = s => { Console.WriteLine(Element.EquationToString(s)); };

            if(input.Length < 1)
                Console.WriteLine("Equation cannot be empty.");

            if (HandleCommands())
                return;
            
            var equation = HandleEquationDecoder();
            if (equation is null) {
                Console.WriteLine("Equation is constructed poorly.");
                return;
            }

            if (debugVersion)
                HandleSimplifierInDebug(equation.decodedEquation);
            else
                HandleSimplifier(equation.decodedEquation);
        }

        private static void HandleSimplifier(List<Element> decodedEquation) {
            output?.Invoke(decodedEquation);
            try {
                Simplifier simplifier = new Simplifier(decodedEquation, output);
                Number result = simplifier.Simplify();
                Console.WriteLine(result.ToString());
            }
            catch (WrongSolverChoosenException e) {
                Console.WriteLine(e.Message);
            }
        }

        private static void HandleSimplifierInDebug(List<Element> decodedEquation) {
            try {
                Simplifier simplifier = new Simplifier(decodedEquation, output);
                Number result = simplifier.Simplify();
                Console.WriteLine(result.ToString());
            }
            catch (WrongSolverChoosenException e) {
                Console.WriteLine(e.Message);
            }
            catch (Exception exception) {
                SaveErrorRaport(exception);
                Console.WriteLine("There seems to be an error with your expression. " + Environment.NewLine +
                                 "ErrorRaport.txt was created and it is advised to send this file to author to this app. " + Environment.NewLine +
                                 "For now i sugest you to try different expresion." + Environment.NewLine);
            }
        }

        public static void SaveErrorRaport(Exception exception) {
            string path = "ErrorRaport.txt";
            using (System.IO.StreamWriter file = System.IO.File.Exists(path)
                                               ? System.IO.File.AppendText(path)
                                               : System.IO.File.CreateText(path)) {
                file.WriteLine(input + "\t" + exception.Message);
                file.Close();
            }
        }

        private static InputEquationDecoder HandleEquationDecoder() {
            try {
                InputEquationDecoder equation = new InputEquationDecoder(input);
                equation.DecodeEquation();
                return equation;
            }
            catch (InvalidEquationException e) {
                return null;
            }
        }

        private static bool HandleCommands() {
            CommandType? commandType;
            if ((commandType = CommandHandler.StringToCommandType(input.Split(' ')[0])) != null) {
                Result result = CommandHandler.Execute((CommandType)commandType);
                if (result.success) {
                    if (result.message != null) {
                        Console.WriteLine(result.message);
                    }
                    return true;
                }
                return true;
            }
            return false;
        }
    }
    
}

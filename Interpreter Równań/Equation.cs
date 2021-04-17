using System;
using System.IO;
using System.Collections.Generic;

namespace Interpreter_Równań {

    public class Equation {

        public string rawEquation { get; private set; }
        List<Element> deconstructed;

        public event EquationSolver.ChangeHandler changed;


        public Equation(string _rawEquation, EquationSolver.ChangeHandler changedEvent) {
            if (_rawEquation == null) {
                throw new NullReferenceException();
            }
            rawEquation = _rawEquation;
            changed += changedEvent;
        }


        public Result Deconstruct() {
            try {
                deconstructed = EquationSolver.DecodeEquationLegacyCode(rawEquation);
            }
            catch (InvalidEquationException e) {
                return new Result(false, "Equation is constructed poorly.");
            }
            return new Result(true);
        }


        public Result Simplify(bool debugVersion = false) {
            if (debugVersion) {
                changed?.Invoke(new EquationSolver.MessageData().ToString(deconstructed));
                try {
                    Number result = EquationSolver.Simplify(deconstructed, new EquationSolver.MessageData(changed));
                    return new Result(true, result.ToString());
                }
                catch (WrongSolverChoosenException e) {
                    return new Result(false, e.Message);
                }
            }
            else {
                try {
                    Number result = EquationSolver.Simplify(deconstructed, new EquationSolver.MessageData(changed));
                    return new Result(true, result.ToString());
                }
                catch (WrongSolverChoosenException e) {
                    return new Result(false, e.Message);
                }
                catch (Exception e) {
                    string path = "ErrorRaport.txt";
                    if (File.Exists(path)) {
                        using (StreamWriter file = File.AppendText(path)) {
                            file.WriteLine(rawEquation);
                            file.WriteLine(e.Message);
                            file.Close();
                        }
                    }
                    else {
                        using (StreamWriter file = File.CreateText(path)) {
                            file.WriteLine(rawEquation);
                            file.WriteLine(e.Message);
                            file.Close();
                        }
                    }
                    string message = "There seems to be an error with your expression. " + Environment.NewLine +
                                     "ErrorRaport.txt was created and it is advised to send this file to author to this app. " + Environment.NewLine +
                                     "For now i sugest you to try different expresion." + Environment.NewLine;
                    return new Result(false, message);
                }
            }
        }


    }
}

using System;
using System.Linq;
using MathNet;

namespace Interpreter_Równań {
    public enum FunctionType { sinh, sech, cosh, csch, ctgh, tgh, sin, sec, cos, csc, ctg, tg, asin, asec, acos, acsc, actg, atg, ln, exp, log, sqrt, cbrt, sign, floor, ceil, round, fact, abs };
    public class Function: Prioritabel {
        public static string[] array = new string[] { "sinh", "sech", "cosh", "csch", "ctgh", "tgh", "sin", "sec", "cos", "csc", "ctg", "tg", "asin", "asec", "acos", "acsc", "actg", "atg", "ln", "exp", "log", "sqrt", "cbrt", "sign", "floor", "ceil", "round", "fact", "abs" };
        public FunctionType functionType { get; }
        private int _priority;
        public override int priority {
            get {
                return _priority;
            }
            set {
                int additionalPriority = Operation.MAX_PRIORITY * value;
                _priority = Operation.MAX_PRIORITY - 1 + additionalPriority;
            }
        }

        public Function(FunctionType _functionType, int additionalPriority) {
            functionType = _functionType;
            elementType = ElementType.Function;
            priority = additionalPriority;
        }
        public override Number Execute(Number[] arguments) {
            double result = 0;
            switch (functionType) {
            case FunctionType.sin:
                result = Math.Sin(arguments[0].value);
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case FunctionType.cos:
                result = Math.Cos(arguments[0].value);
                break;
            case FunctionType.ln:
                result = Math.Log(arguments[0].value);
                break;
            case FunctionType.exp:
                result = Math.Exp(arguments[0].value);
                break;
            case FunctionType.log:
                if (arguments.Length == 1) {
                    result = Math.Log10(arguments[0].value);
                    break;
                }
                else {
                    result = Math.Log(arguments[0].value, arguments[1].value);
                    break;
                }
            case FunctionType.tg:
                result = Math.Tan(arguments[0].value);
                break;
            case FunctionType.sec:
                result = 1 / Math.Sin(arguments[0].value);
                break;
            case FunctionType.csc:
                result = 1 / Math.Cos(arguments[0].value);
                break;
            case FunctionType.ctg:
                result = 1 / Math.Tan(arguments[0].value);
                break;
            case FunctionType.sqrt:
                result = Math.Sqrt(arguments[0].value);
                break;
            case FunctionType.cbrt:
                result = Math.Pow(arguments[0].value, 1 / 3);
                break;
            case FunctionType.abs:
                result = Math.Abs(arguments[0].value);
                break;
            case FunctionType.sign:
                result = Math.Sign(arguments[0].value);
                break;
            case FunctionType.asin:
                result = Math.Asin(arguments[0].value);
                break;
            case FunctionType.asec:
                double X = arguments[0].value;
                result = 2 * Math.Atan(1) - Math.Atan(Math.Sign(X) / Math.Sqrt(X * X - 1));
                break;
            case FunctionType.acos:
                result = Math.Acos(arguments[0].value);
                break;
            case FunctionType.acsc:
                X = arguments[0].value;
                result = Math.Atan(Math.Sign(X) / Math.Sqrt(X * X - 1));
                break;
            case FunctionType.atg:
                result = Math.Atan(arguments[0].value);
                break;
            case FunctionType.actg:
                X = arguments[0].value;
                result = 2 * Math.Atan(1) - Math.Atan(X);
                break;
            case FunctionType.sinh:
                result = Math.Sinh(arguments[0].value);
                break;
            case FunctionType.sech:
                result = 1 / Math.Sinh(arguments[0].value);
                break;
            case FunctionType.cosh:
                result = Math.Cosh(arguments[0].value);
                break;
            case FunctionType.csch:
                result = 1 / Math.Cosh(arguments[0].value);
                break;
            case FunctionType.tgh:
                result = Math.Tanh(arguments[0].value);
                break;
            case FunctionType.ctgh:
                result = 1 / Math.Tanh(arguments[0].value);
                break;
            case FunctionType.fact:
                result = MathNet.Numerics.SpecialFunctions.Gamma(arguments[0].value + 1.0);
                break;
            case FunctionType.floor:
                result = Math.Floor(arguments[0].value);
                break;
            case FunctionType.ceil:
                result = Math.Ceiling(arguments[0].value);
                break;
            case FunctionType.round:
                result = Math.Round(arguments[0].value);
                break;
            }
            if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                result = Math.Round(result);
            }
            return new Number(result);
            //sin, cos, ln, exp, log, tg, sec, csc, ctg, sqrt, cbrt, abs, sign, asin, asec, acos, 
            //acsc, atg, actg, sinh, sech, cosh, csch, tgh, ctgh,  fact, floor, ceil, round, 
        }

        public static string FunctionTypeToString(FunctionType functionType) {
            return array[(int)functionType];
        }
        public static FunctionType? StringToFunctionType(string input, int startingIndex = 0, int count = -1) {// expects string containing function, returns first function found(with lowest index in array);
            if (count < 0) {
                count = input.Length - startingIndex;
            }
            foreach (var item in array) {
                if (input.ToLower().IndexOf(item, startingIndex, count) > -1) {
                    if (item == "abs" || item == "mod") {
                        return FunctionType.abs;
                    }
                    else if (item == "!" || item == "fact") {
                        return FunctionType.fact;
                    }
                    else {
                        return (FunctionType)(Function.array.ToList().IndexOf(item));
                    }
                }
            }
            return null;
        }

        public override string ToString() {
            return FunctionTypeToString(functionType);
        }
    }

}

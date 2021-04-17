using System;
using System.Linq;


namespace Interpreter_Równań {


    public enum OperationType { Addition, Subtraction, Multiplication, Division, Power, Factorial };

    public class Operation: Prioritabel {  // implement Factorial, to be implemented in class Function

        public OperationType operationType { get; }

        private int _priority;
        public override int priority {
            get {
                return _priority;
            }
            set {
                int additionalPriority = MAX_PRIORITY * value;
                if (operationType == OperationType.Addition || operationType == OperationType.Subtraction) {
                    _priority = 0 + additionalPriority;
                }
                else if (operationType == OperationType.Multiplication || operationType == OperationType.Division) {
                    _priority = 1 + additionalPriority;
                }
                else if (operationType == OperationType.Power) {
                    _priority = 2 + additionalPriority;
                }
                else if (operationType == OperationType.Factorial) {
                    _priority = 3 + additionalPriority;
                }
            }
        }

        public static char[] array = new char[] { '+', '-', '*', '/', '^', '!' };
        public const int MAX_PRIORITY = 4 + 1;// 4 operations and 1 function


        public Operation(OperationType _operationType, int additionalPriorityInput = 0) {
            elementType = ElementType.Operation;
            operationType = _operationType;
            priority = additionalPriorityInput;
        }


        public static char OperationTypeToChar(OperationType operationType) {
            return array[(int)operationType];
        }

        public static OperationType? CharToOperationType(char input) { // mozna zoptymalizowac switchem przez krotka dlugosc arrayu
            foreach (var item in array) {
                if (item == input) {
                    return (OperationType)(array.ToList().IndexOf(item));
                }
            }
            return null;
        }


        public override Number Execute(Number[] arguments) {// left hand side, right hand side
            double result = 0;
            switch (operationType) {
            case OperationType.Addition:
                result = Settings.Round(arguments[0].value + arguments[1].value);
                result = arguments[0].value + arguments[1].value;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Subtraction:
                result = Settings.Round(arguments[0].value - arguments[1].value);
                break;
            case OperationType.Division:
                result = Settings.Round(arguments[0].value / arguments[1].value);
                break;
            case OperationType.Multiplication:
                result = Settings.Round(arguments[0].value * arguments[1].value);
                break;
            case OperationType.Power:
                result = Settings.Round(Math.Pow(arguments[0].value, arguments[1].value));
                break;
            case OperationType.Factorial:
                result = Settings.Round(new Function(FunctionType.fact, 0).Execute(arguments).value);
                break;
            }
            return new Number(result);
        }


        public override string ToString() {
            return OperationTypeToChar(this.operationType).ToString();
        }
    }

}

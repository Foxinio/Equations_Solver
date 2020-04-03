using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Interpreter_Równań {
    class Program {
        static void Main(string[] args) {
            while (true) {
                new Equation();
            }
        }
    }

    public static class Settings {
        public static int lvlOfAproximation = 10;
    }
    
    public class EquationSolver {

        [Serializable]
        public class InvalidEquationException: Exception {

            public InvalidEquationException() {

            }
        }
        [Serializable]
        public class TestException: Exception {
            const string defaultMessage = "Tests resulted in finding Error: ";

            public TestException(
                string message) :
                    base(String.Format("{0} - {1}",
                        defaultMessage, message)) {
            }
        }

        private enum Type {
            BracketsOpen,
            Number,
            Word,
            Operator,
            BracketsClose,
            Equality,
            Factorial,
            Comma,
            Null
        }
        private static char[] operators = { '+', '-', '*', '/', '^' };
        public delegate void ChangeHandler(string input);
        private static string[] Split(string input) {
            List<string> result = new List<string>();

            string currentString = string.Empty;
            bool isDot = false;
            Type previousType = Type.Null;
            Type beforePreviousType = Type.Null;

            foreach (var item in input) {

                Type currentType = Type.Null;

                if (operators.Contains(item)) {
                    currentType = Type.Operator;
                }
                else if (item == '(') {
                    currentType = Type.BracketsOpen;
                    beforePreviousType = previousType;
                }
                else if (item == ')') {
                    currentType = Type.BracketsClose;
                }
                else if (item >= '0' && item <= '9' || item == '.') {
                    currentType = Type.Number;
                    if (item == '.' && (isDot || currentString.Length == 0)) {
                        throw new InvalidEquationException();
                    }
                }
                else if (item >= 'a' && item <= 'z' || item >= 'A' && item <= 'Z') {
                    currentType = Type.Word;
                }
                else if (item == '=' || item == '<' || item == '>') {
                    currentType = Type.Equality;
                    if (previousType == Type.Factorial) {
                        beforePreviousType = Type.Factorial;
                    }
                }
                else if (item == '!') {
                    currentType = Type.Factorial;
                }
                else if (item == ',') {
                    currentType = Type.Comma;
                }
                else if (item == ' ') {
                    continue;
                }
                else {
                    throw new FormatException();
                }



                if (previousType == Type.Null) {//                                          Null
                    currentString = string.Empty + item;
                    if (currentType == Type.Factorial || currentType == Type.Comma) {
                        throw new InvalidEquationException();
                    }
                }
                else if (previousType == Type.BracketsOpen) {//                             Brackets Open
                    if (currentType == Type.Operator || currentType == Type.Comma) {
                        throw new InvalidEquationException();
                    }
                    result.Add(currentString);
                    currentString = string.Empty + item;
                    if (currentType == Type.BracketsClose) {
                        result.Remove(result.Last());
                        //if(beforePreviousType == Type.Null) {// dlaczego
                        //    throw new InvalidEquationException();
                        //}
                        previousType = beforePreviousType;
                        continue;
                    }
                }
                else if (previousType == Type.Number) {//                                   Number
                    if (currentType == previousType) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        if (currentType == Type.BracketsOpen || currentType == Type.Word) {
                            result.Add("*");
                        }
                        currentString = string.Empty + item;
                        isDot = false;
                    }
                }
                else if (previousType == Type.Word) {//                                     Word
                    if (currentType == previousType) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == Type.Operator) {//                                 Operator
                    if (new Type[] {
                            Type.Operator,
                            Type.BracketsClose,
                            Type.Comma
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == Type.BracketsClose) {//                             Brackets Close
                    result.Add(currentString);
                    if (new Type[] {
                            Type.Number,
                            Type.BracketsOpen,
                            Type.Word
                        }.Contains(currentType)) {
                        result.Add("*");
                    }
                    currentString = string.Empty + item;
                }
                else if (previousType == Type.Factorial) {//                                 Factorial
                    if (currentType == Type.Equality) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == Type.Equality) {//                                  Equality
                    if (new Type[] {
                            Type.Operator,
                            Type.BracketsClose,
                            Type.Comma
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    else if (currentType == Type.Equality && beforePreviousType == Type.Factorial) {
                        result.Add("!");
                        currentString = "=" + item;
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == Type.Comma) {//                                     Comma
                    if (new Type[] {
                            Type.Operator,
                            Type.BracketsClose,
                            Type.Equality,
                            Type.Factorial,
                            Type.Comma
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    result.Add(currentString);
                    currentString = string.Empty + item;
                }
                previousType = currentType;
            }
            result.Add(currentString);
            return result.ToArray();
        }
        public static List<Element> DecodeEquation(string input) { // TODO this needs review, Start here
            List<Element> result = new List<Element>();
            if(input.Length < 1) {
                throw new InvalidEquationException();
            }

            List<Bracket> openBrackets = new List<Bracket>();
            List<Function> twoArgumentFunctions = new List<Function>();//   TODO extend this (its needed because of commas)
            int additionalPriority = 0;
            bool isEqualitySign = false;
            List<int> commas = new List<int>();
            commas.Add(0);

            string[] array = Split(input);

            foreach (var item in array) {
                if (item[0] == '(') {
                    var bracket = new Bracket(BracketType.Opening);
                    commas.Add(0);
                    openBrackets.Add(bracket);
                    result.Add(bracket);
                    additionalPriority++;
                }
                else if (item[0] >= '0' && item[0] <= '9') {
                    result.Add(new Number(double.Parse(item, CultureInfo.InvariantCulture)));
                }
                else if (item.ToLower()[0] >= 'a' && item.ToLower()[0] <= 'z') {
                    var temp = WordInterpreter.InterpretWord(item, additionalPriority);
                    foreach (var subitem in temp) {
                        result.Add(subitem);
                    }
                }
                else if (operators.Contains(item[0])) {
                    result.Add(new Operation((OperationType)Operation.CharToOperationType(item[0]), additionalPriority));
                }
                else if (item[0] == ')') {
                    if (openBrackets.Count == 0) {
                        if (!isEqualitySign) {
                            var bracket = new Bracket(BracketType.Opening);
                            foreach (var subitem in result) {
                                if (subitem.elementType == ElementType.Operation) {
                                    (subitem as Operation).priority += 1;
                                }
                            }
                            result.Insert(0, bracket);
                            result.Add(new Bracket(BracketType.Closing, bracket));
                        }
                        else {
                            int equalityIndex = result.FindLastIndex(c => { return c.elementType == ElementType.Equality; });
                            var bracket = new Bracket(BracketType.Opening);
                            foreach (var subitem in result) {
                                if (subitem.elementType == ElementType.Operation && result.IndexOf(subitem) > equalityIndex) {
                                    (subitem as Operation).priority += 1;
                                }
                            }
                            result.Insert(equalityIndex + 1, bracket);
                            result.Add(new Bracket(BracketType.Closing, bracket));
                        }
                    }
                    else {
                        var bracket = openBrackets.Last();
                        openBrackets.Remove(bracket);
                        commas.Remove(commas.Last());
                        result.Add(new Bracket(BracketType.Closing, bracket));
                        additionalPriority--;
                    }
                }
                else if (item[0] == '=' || item[0] == '<' || item[0] == '>') {
                    if (openBrackets.Count > 0) {
                        foreach (var subitem in openBrackets) {
                            result.Remove(subitem);
                        }
                        openBrackets.Clear();
                        additionalPriority = 0;
                    }
                    result.Add(new Equality((EqualityType)Equality.StringToEqualityType(item)));
                    isEqualitySign = true;
                }
                else if (item[0] == ',') {
                    if (commas.Last() == Comma.MAX_COMMAS) {
                        throw new InvalidEquationException();
                    }
                    result.Add(new Comma());
                    commas[commas.Count - 1]++;
                }
                else if (item[0] == '!') {
                    result.Add(new Operation(OperationType.Factorial, additionalPriority));
                }
            }

            if (openBrackets.Count > 0) {
                int count = openBrackets.Count;
                foreach (var subitem in openBrackets) {
                    result.Add(new Bracket(BracketType.Closing,subitem));
                }
                openBrackets.Clear();
                additionalPriority = 0;
            }

            if (isEqualitySign) {// removes unnessesery brackets to avoid sth like this: (((X)))
                int equalityCount = result.Count(c => { return c.elementType == ElementType.Equality; });
                int[] equalitySignIndex = new int[equalityCount];
                equalitySignIndex[0] = result.FindIndex(c => { return c.elementType == ElementType.Equality; });
                for (int i = 1; i < equalitySignIndex.Length; i++) {
                    equalitySignIndex[i] = result.FindIndex(equalitySignIndex[i - 1] + 1, c => { return c.elementType == ElementType.Equality; });
                }
                for (int i = 0; i < equalityCount + 1; i++) {
                    while (result[i == 0 ? 0 : equalitySignIndex[i - 1]].elementType == ElementType.Brackets && result[i == equalityCount + 1 ? result.Count - 1 : equalitySignIndex[i]].elementType == ElementType.Brackets) {
                        result.RemoveAt(i == 0 ? 0 : equalitySignIndex[i - 1]);
                        result.RemoveAt(i == equalityCount + 1 ? result.Count - 1 : equalitySignIndex[i]);
                    }
                }
            }
            else {
                while (result[0].elementType == ElementType.Brackets && result[result.Count - 1].elementType == ElementType.Brackets) {
                    result.RemoveAt(0);
                    result.RemoveAt(result.Count - 1);
                }
            }
            return result;
        }
        public struct MessageData {

            public string beforeString { get; set; }
            public string afterString { get; set; }
            public ChangeHandler change;
            
            public MessageData(ChangeHandler _change, List<Element> equation = null, int openBracketIndex = 0, int closeBracketIndex = 0, string beforeBeforeString = "", string afterAfterString = "", bool withFunction = false ) {
                change = _change;
                beforeString = beforeBeforeString;
                afterString = string.Empty;
                if (equation != null && closeBracketIndex != 0) {
                    for (int i = 0; i < equation.Count; i++) {
                        if (i <= (openBracketIndex - (withFunction ? 2 : 0))) {
                            beforeString += equation[i].ToString();
                        }
                        else if (i >= closeBracketIndex + (withFunction ? 1 : 0)) {
                            afterString += equation[i].ToString();
                        }
                    }
                }
                afterString += afterAfterString;
            }
            public string ToString(List<Element> equation) {
                string result = (beforeString!=null?String.Copy(beforeString):string.Empty);
                foreach (var item in equation) {
                    result += item.ToString();
                }
                return result + (afterString!=null?afterString:string.Empty);
            }
        }
        public static Number Simplify(List<Element> argument, MessageData messageData, Function function = null) {
            var simplifyied = new List<Element>();
            for (int i = 0; i < argument.Count; i++) {
                if (argument[i].elementType == ElementType.Brackets) {
                    var temp = new List<Element>();
                    int endIndex = argument.IndexOf((argument[i] as Bracket).enclosureReference);
                    for (int j = i + 1; j < endIndex; j++) {
                        temp.Add(argument[j]);
                    }
                    if (argument[i - 1].elementType == ElementType.Function) {
                        simplifyied.RemoveAt(simplifyied.Count - 1);
                        simplifyied.Add(Simplify(temp, new MessageData(messageData.change, argument, i, endIndex, messageData.beforeString, messageData.afterString), argument[i - 1] as Function));
                    }
                    else {
                        simplifyied.Add(Simplify(temp, new MessageData(messageData.change, argument, i, endIndex, messageData.beforeString, messageData.afterString)));
                    }
                    i = endIndex;
                }
                else if (argument[i].elementType == ElementType.Constant) {
                    simplifyied.Add((argument[i] as Constant).Execute());
                }
                else {
                    simplifyied.Add(argument[i]);
                }
            }

            var commas = simplifyied.Count(e => { return e.elementType == ElementType.Comma; });
            while (simplifyied.Count - 2 * commas > 1) {
                var maxPriority = (from op in simplifyied where op.elementType == ElementType.Operation || op.elementType == ElementType.Function select op as Prioritabel).ToArray().Max((Prioritabel x) => { return x.priority; });
                for (int i = 0; i < simplifyied.Count; i++) {
                    if (simplifyied[i].elementType == ElementType.Operation) {
                        var element = simplifyied[i] as Operation;
                        if (element.priority == maxPriority) {
                            var array = simplifyied.ToArray();
                            if (element.operationType == OperationType.Factorial) {
                                simplifyied.Insert(i, element.Execute((simplifyied[i - 1] as Number).value));
                                simplifyied.Remove(array[i - 1]);
                                simplifyied.Remove(element);
                                messageData.change?.Invoke(messageData.ToString(simplifyied));
                            }
                            else {
                                simplifyied.Insert(i, element.Execute((simplifyied[i - 1] as Number).value, (simplifyied[i + 1] as Number).value));
                                simplifyied.Remove(array[i - 1]);
                                simplifyied.Remove(array[i + 1]);
                                simplifyied.Remove(element);
                                messageData.change?.Invoke(messageData.ToString(simplifyied));
                            }
                        }
                    }
                    else if (simplifyied[i].elementType == ElementType.Function) {
                        var element = simplifyied[i] as Function;
                        if (element.priority == maxPriority) {
                            var array = simplifyied.ToArray();
                            simplifyied.Insert(i, element.Execute(new Number[] { simplifyied[i + 1] as Number }));
                            simplifyied.Remove(array[i + 1]);
                            simplifyied.Remove(element);
                            messageData.change?.Invoke(messageData.ToString(simplifyied));
                        }
                    }
                }
            }
            if(function != null){
                var arguments = (from op in simplifyied where op.elementType == ElementType.Number select op as Number).ToArray();
                messageData.change?.Invoke(messageData.ToString(new Element[] { function.Execute(arguments) }.ToList()));
                return function.Execute(arguments);
            }
            return simplifyied[0] as Number;
        }
        private static string Write(List<Element> elements) {
            string result = String.Empty;
            foreach (var item in elements) {
                result += item.ToString();
                result += " ";
            }
            return result;
        }
    }

    public class Equation {
        public string rawEquation { get; private set; }
        List<Element> deconstructed;
        bool isProper = true;

        public event EquationSolver.ChangeHandler changed;

        public Equation(string _rawEquation = null) {
            rawEquation = _rawEquation;
            if (_rawEquation == null) {
                rawEquation = Console.ReadLine();
            }
            try {
                deconstructed = EquationSolver.DecodeEquation(rawEquation);
            }
            catch (EquationSolver.InvalidEquationException e) {
                isProper = false;
                Console.WriteLine("Equation is constructed poorly.");
            }
            if (isProper) {
                changed += (string s) => { Console.WriteLine(s); };
                changed?.Invoke(new EquationSolver.MessageData().ToString(deconstructed));
                EquationSolver.Simplify(deconstructed, new EquationSolver.MessageData(changed));
            }
            Console.ReadKey();
        }
    }



    public class WordInterpreter {
        class FunctionIndex: IComparable<FunctionIndex> {
            public FunctionType functionType { get; set; }
            public int index { get; set; }
            public FunctionIndex(FunctionType _functionType, int _index) {
                functionType = _functionType;
                index = _index;
            }

            public int CompareTo(FunctionIndex compare) {
                // A null value means that this object is greater.
                if (compare == null)
                    return 1;

                else
                    return this.index.CompareTo(compare.index);
            }
            public static List<FunctionIndex> GetArray(string input) {
                var result = new List<FunctionIndex>();
                for (int i = 0; i < Function.array.Length; i++) {
                    if (input.IndexOf(Function.array[i]) != -1) {
                        result.Add(new FunctionIndex((FunctionType)i, input.IndexOf(Function.array[i])));
                    }
                }
                result.Sort();
                return result.Count > 0 ? result : null;
            }

        }
        public static List<Element> InterpretWord(string input, int additionalPriority) {
            if (input.Length == 0) {
                throw new NullReferenceException("Input of InterpretWord(string input) cannot have length of 0.");
            }
            List<Element> result = new List<Element>();
            ElementType lastAddedElement = ElementType.Comma;
            if (Function.StringToFunctionType(input) == null) {
                ConstantType? constant = Constant.StringToConstantType(input);
                if (constant != null) {
                    int[] powerArray = new int[Constant.array.Length];
                    while (constant != null) {
                        powerArray[(int)constant]++;
                        input = input.Remove(input.IndexOf(Constant.ConstantTypeToString((ConstantType)constant)), Constant.ConstantTypeToString((ConstantType)constant).Length);
                        constant = Constant.StringToConstantType(input);
                    }
                    for (int i = 0; i < powerArray.Length; i++) {
                        if (powerArray[i] > 0) {
                            if(lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                                result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                            }
                            result.Add(new Constant((ConstantType)i, powerArray[i]));
                            lastAddedElement = ElementType.Constant;
                        }
                    }
                }
                foreach (var item in input) {
                    var power = input.Count(c => { return c == item; });
                    if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                        result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                    }
                    result.Add(new Variable(item, power));
                    lastAddedElement = ElementType.Parameter;
                    while (input.IndexOf(item) != -1) {
                        input = input.Remove(input.IndexOf(item), 1);
                    }
                }
            }
            else {
                while (input.Length > 0) {
                    List<FunctionIndex> functionIndex = FunctionIndex.GetArray(input);
                    int toSubtract = 0;
                    ConstantType? constant = Constant.StringToConstantType(input, 0, functionIndex!=null?functionIndex[0].index:input.Length);
                    if (constant != null) {
                        int[] powerArray = new int[Constant.array.Length];
                        while (constant != null) {
                            powerArray[(int)constant]++;
                            input = input.Remove(input.IndexOf(Constant.ConstantTypeToString((ConstantType)constant), 0, (functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract), Constant.ConstantTypeToString((ConstantType)constant).Length);
                            toSubtract += Constant.ConstantTypeToString((ConstantType)constant).Length;
                            constant = Constant.StringToConstantType(input, 0, ((functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract));
                        }
                        for (int j = 0; j < powerArray.Length; j++) {
                            if (powerArray[j] > 0) {
                                if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                                    result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                                }
                                result.Add(new Constant((ConstantType)j, powerArray[j]));
                                lastAddedElement = ElementType.Constant;
                            }
                        }
                    }
                    if((functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract >= 0){
                        foreach (var item in input.Substring(0, (functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract)) {
                            var power = input.Substring(0, ((functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract)).Count(c => { return c == item; });
                            if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                                result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                            }
                            result.Add(new Variable(item, power));
                            lastAddedElement = ElementType.Parameter;
                            while (input.IndexOf(item, 0, ((functionIndex != null ? functionIndex[0].index : input.Length) - toSubtract)) != -1) {
                                input = input.Remove(input.IndexOf(item), 1);
                                toSubtract++;
                            }
                        }
                        // jesli wszystko zadzaialalo to w tym momencie funkcja powinna zaczyac sie na indexie 0
                        if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                            result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                        }
                        result.Add(new Function(functionIndex[0].functionType, additionalPriority));
                        lastAddedElement = ElementType.Function;
                        input = input.Remove(0, Function.FunctionTypeToString((FunctionType)functionIndex[0].functionType).Length);
                    }
                }
            }
            if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                result.Add(new Operation(OperationType.Multiplication, additionalPriority));
            }
            return result;
        }
    }

    public enum ElementType {
        Unknown,    // nie obslugiwane
        Number,
        Operation,
        Equality,   // w wiekszosci nie obslugiwane
        Brackets,
        Function,
        Parameter,  // w wiekszosci nie oblsugiwane
        Constant,
        Comma,
        Factorial
    };
    public abstract class Element {
        public ElementType elementType { get; set; }
    }

    public class Number: Element {
        public double value { get; }

        public Number(double _quantity) {
            elementType = ElementType.Number;
            value = _quantity;
        }

        public override string ToString() {
            return value.ToString();
        }
    }

    public abstract class Prioritabel : Element {
        abstract public int priority { get; set; }

    }

    public enum OperationType { Addition, Subtraction, Multiplication, Division, Power, Factorial };
    public class Operation: Prioritabel {  // implement Factorial, to be implemented in class Function
        public OperationType operationType { get; }
        private int _priority;
        public override int priority {
            get {
                return _priority;
            }
            set {
                int additionalPriority = (int)Math.Pow(MAX_PRIORITY, value);
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
        public static char[] array = new char[] {'+', '-', '*', '/', '^', '!'};
        public const int MAX_PRIORITY = 4 + 1;// 4 operations and 1 function

        public Operation(OperationType _operationType, int additionalPriorityInput = 0) {
            elementType = ElementType.Operation;
            operationType = _operationType;
            int additionalPriority = (int)Math.Pow(MAX_PRIORITY, additionalPriorityInput);
            if(operationType == OperationType.Addition || operationType == OperationType.Subtraction) {
                _priority = 0 + additionalPriority;
            }
            else if (operationType == OperationType.Multiplication || operationType == OperationType.Division) {
                _priority = 1 + additionalPriority;
            }
            else if(operationType == OperationType.Power) {
                _priority = 2 + additionalPriority;
            }
            else if(operationType == OperationType.Factorial) {
                _priority = 3 + additionalPriority;
            }
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

        public Number Execute(double lhs, double? rhs = null) {// left hand side, right hand side
            double result = 0;
            switch (operationType) {
            case OperationType.Addition:
                result = lhs + (double)rhs;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Subtraction:
                result = lhs - (double)rhs;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Division:
                result = lhs / (double)rhs;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Multiplication:
                result = lhs * (double)rhs;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Power:
                result = Math.Pow(lhs, (double)rhs);
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            case OperationType.Factorial:
                result = new Function(FunctionType.fact, 0).Execute(new Number[] { new Number(lhs) }).value;
                if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * Settings.lvlOfAproximation)) {
                    result = Math.Round(result);
                }
                break;
            }
            return new Number(result);
        }

        public override string ToString() {
            return OperationTypeToChar(this.operationType).ToString();
        }
    }
    
    public enum FunctionType                        { sinh,   sech,   cosh,   csch,   ctgh,   tgh,   sin,   sec,   cos,   csc,   ctg,   tg,   asin,   asec,   acos,   acsc,   actg,   atg,   ln,   exp,   log,   sqrt,   cbrt,   sign,   floor,   ceil,   round,   fact,   abs };
    public class Function : Prioritabel {
        public static string[] array = new string[] {"sinh", "sech", "cosh", "csch", "ctgh", "tgh", "sin", "sec", "cos", "csc", "ctg", "tg", "asin", "asec", "acos", "acsc", "actg", "atg", "ln", "exp", "log", "sqrt", "cbrt", "sign", "floor", "ceil", "round", "fact", "abs" };
        public FunctionType functionType { get; }
        private int _priority;
        public override int priority {
            get {
                return _priority;
            }
            set {
                int additionalPriority = (int)Math.Pow(Operation.MAX_PRIORITY, value);
                _priority = Operation.MAX_PRIORITY - 1 + additionalPriority;
            }
        }

        public Function(FunctionType _functionType, int additionalPriority) {
            functionType = _functionType;
            elementType = ElementType.Function;
            priority = additionalPriority;
        }
        public Number Execute(Number[] arguments) {
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
                result = 1/Math.Sin(arguments[0].value);
                break;
            case FunctionType.csc:
                result = 1/Math.Cos(arguments[0].value);
                break;
            case FunctionType.ctg:
                result = 1/Math.Tan(arguments[0].value);
                break;
            case FunctionType.sqrt:
                result = Math.Sqrt(arguments[0].value);
                break;
            case FunctionType.cbrt:
                result = Math.Pow(arguments[0].value, 1/3);
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
                result = 1/Math.Sinh(arguments[0].value);
                break;
            case FunctionType.cosh:
                result = Math.Cosh(arguments[0].value);
                break;
            case FunctionType.csch:
                result = 1/Math.Cosh(arguments[0].value);
                break;
            case FunctionType.tgh:
                result = Math.Tanh(arguments[0].value);
                break;
            case FunctionType.ctgh:
                result = 1/Math.Tanh(arguments[0].value);
                break;
            case FunctionType.fact:// TODO  to be implemented
                return null;
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
            if(count < 0) {
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

    public enum EqualityType { EqualTo, LessThan, MoreThan, LessOrEqualTo, MoreOrEqualTo, Compare }
    public class Equality : Element{    // TODO Expand this
        public EqualityType equalityType { get; }
        public static string[] array = new string[] { "=", "<", ">", "<=", ">=", "==" };

        public Equality() { // dont use, delete asap
            elementType = ElementType.Equality;
            equalityType = EqualityType.EqualTo;
        }
        public Equality(EqualityType _equalityType) {
            elementType = ElementType.Equality;
            equalityType = _equalityType;
        }

        public static string EqualityTypeToString(EqualityType equalityType) {
            return array[(int)equalityType];
        }
        public static EqualityType? StringToEqualityType(string input) {
            foreach (var item in array) {
                if(item.Equals(input)) {
                    return (EqualityType)(Equality.array.ToList().IndexOf(item));
                }
            }
            return null;
        }

        public override string ToString() {
            return EqualityTypeToString(equalityType);
        }
    }

    public enum BracketType { Opening, Closing };
    public class Bracket : Element{
        public Bracket enclosureReference { get; set; }
        public BracketType bracketType { get; }

        public Bracket(BracketType _bracketType, Bracket otherSide = null ) {
            elementType = ElementType.Brackets;
            bracketType = _bracketType;
            enclosureReference = otherSide;
            if(otherSide != null) {
                otherSide.enclosureReference = this;
            }
        }

        public override string ToString() {
            switch (bracketType) {
            case BracketType.Opening: return "(";
            case BracketType.Closing: return ")";
            default: return "";
            }
        }
    }

    public class Variable : Element {// TODO refactor this so that Constant : Variable
        public char symbol;
        public double power;

        public Variable(char _symbol, double _power, ElementType _elementType = ElementType.Parameter) {
            symbol = _symbol;
            power = _power;
            if(_elementType != ElementType.Parameter && _elementType != ElementType.Unknown) {
                throw new FormatException();
            }
            elementType = _elementType;
        }

        public override string ToString() {
            return symbol.ToString();
        }
    }

    public enum ConstantType { e, pi, phi, rand }
    public class Constant: Element {
        ConstantType constant;
        public double power;
        public static string[] array = new string[] { "e", "pi", "phi", "rand" };

        public Constant(ConstantType _constant, int _power) {
            elementType = ElementType.Constant;
            constant = _constant;
            power = _power;
        }

        public static string ConstantTypeToString(ConstantType constant) {
            return array[(int)constant];
        }
        public static ConstantType? StringToConstantType(string input, int startingIndex = 0, int count = -1) {
            if(count < 0) {
                count = input.Length - startingIndex;
            }
            foreach (var item in array) {
                if (input.ToLower().IndexOf(item, startingIndex, count) > -1) {
                    return (ConstantType)(Constant.array.ToList().IndexOf(item));
                }
            }
            return null;
        }

        public Number Execute() {
            switch (constant) {
            case ConstantType.e:
                return new Number(Math.Pow(Math.E, power));
            case ConstantType.pi:
                return new Number(Math.Pow(Math.PI,power));
            case ConstantType.phi:
                return new Number(Math.Pow(1.61803398875,power));
            case ConstantType.rand:
                return new Number(Math.Pow(new Random().NextDouble(),power));
            default:
                return null;
            }
        }

        public override string ToString() {
            return ConstantTypeToString(constant);
        }
    }

    public class Comma : Element {
        public const int MAX_COMMAS = 1;
        public Comma() {
            this.elementType = ElementType.Comma;
        }

        public override string ToString() {
            return ",";
        }
    }

    public class Factiorial : Element {         // nie w użyciu

        public Factiorial() {
            this.elementType = ElementType.Factorial;
        }

        public Number Execute(Number argument) {
            return new Function(FunctionType.fact, 0).Execute(new Number[] { argument });
        }
    }
}

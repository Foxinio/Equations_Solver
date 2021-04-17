using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Interpreter_Równań {

    public class EquationSolver {

        private enum SplitElementType {
            BracketsOpen,
            Number,
            Word,
            Operator,
            BracketsClose,
            Equality,
            Factorial,
            Comma,
            Negative,
            Null
        }

        private static char[] operators = { '+', '*', '/', '^' };

        public delegate void ChangeHandler(string input);

        private static string[] SplitLegacyCode(string input) {
            List<string> result = new List<string>();

            string currentString = string.Empty;
            bool isDot = false;
            SplitElementType previousType = SplitElementType.Null;
            SplitElementType beforePreviousType = SplitElementType.Null;

            foreach (var item in input) {

                SplitElementType currentType = SplitElementType.Null;

                if (operators.Contains(item)) {
                    currentType = SplitElementType.Operator;
                }
                else if (item == '(') {
                    currentType = SplitElementType.BracketsOpen;
                    beforePreviousType = previousType;
                }
                else if (item == ')') {
                    currentType = SplitElementType.BracketsClose;
                }
                else if (item == '-') {
                    currentType = SplitElementType.Negative;
                }
                else if (item >= '0' && item <= '9' || item == '.') {
                    currentType = SplitElementType.Number;
                    if (item == '.' && (isDot || currentString.Length == 0)) {
                        throw new InvalidEquationException();
                    }
                }
                else if (item >= 'a' && item <= 'z' || item >= 'A' && item <= 'Z') {
                    currentType = SplitElementType.Word;
                }
                else if (item == '=' || item == '<' || item == '>') {
                    currentType = SplitElementType.Equality;
                    if (previousType == SplitElementType.Factorial) {
                        beforePreviousType = SplitElementType.Factorial;
                    }
                }
                else if (item == '!') {
                    currentType = SplitElementType.Factorial;
                }
                else if (item == ',') {
                    currentType = SplitElementType.Comma;
                }
                else if (item == ' ') {
                    continue;
                }
                else {
                    throw new InvalidEquationException();
                }



                if (previousType == SplitElementType.Null) {//                                          Null
                    currentString = string.Empty + item;
                    if (currentType == SplitElementType.BracketsClose) {
                        currentString = string.Empty;
                    }
                    else if (new SplitElementType[] {
                                SplitElementType.Operator,
                                SplitElementType.Comma,
                                SplitElementType.Equality,
                                SplitElementType.Factorial
                            }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                }
                else if (previousType == SplitElementType.BracketsOpen) {//                             Brackets Open
                    if (currentType == SplitElementType.Operator || currentType == SplitElementType.Comma || currentType == SplitElementType.Factorial) {
                        throw new InvalidEquationException();
                    }
                    if (currentType == SplitElementType.BracketsClose) {
                        currentString = string.Empty;
                        previousType = beforePreviousType;
                        continue;
                    }
                    else if (currentType == SplitElementType.Equality) {
                        currentString = string.Empty + item;
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == SplitElementType.Number) {//                                   Number
                    if (currentType == previousType) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        if (currentType == SplitElementType.BracketsOpen || currentType == SplitElementType.Word) {
                            result.Add("*");
                        }
                        if (currentType == SplitElementType.Negative) {
                            result.Add("+");
                            currentType = SplitElementType.Number;
                        }
                        currentString = string.Empty + item;
                        isDot = false;
                    }
                }
                else if (previousType == SplitElementType.Word) {//                                     Word
                    if (currentType == previousType) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == SplitElementType.Operator) {//                                 Operator
                    if (new SplitElementType[] {
                            SplitElementType.Operator,
                            SplitElementType.BracketsClose,
                            SplitElementType.Comma,
                            SplitElementType.Equality,
                            SplitElementType.Factorial
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    else {
                        result.Add(currentString);
                        currentString = string.Empty + item;
                        if (currentType == SplitElementType.Negative) {
                            currentType = SplitElementType.Number;
                        }
                    }
                }
                else if (previousType == SplitElementType.BracketsClose) {//                             Brackets Close
                    result.Add(currentString);
                    if (new SplitElementType[] {
                            SplitElementType.Number,
                            SplitElementType.BracketsOpen,
                            SplitElementType.Word
                        }.Contains(currentType)) {
                        result.Add("*");
                    }
                    if (currentType == SplitElementType.Negative) {
                        result.Add("+");
                    }
                    currentString = string.Empty + item;
                }
                else if (previousType == SplitElementType.Factorial) {//                                 Factorial
                    if (currentType == SplitElementType.Equality) {
                        currentString += item;
                    }
                    else {
                        result.Add(currentString);
                        if (currentType == SplitElementType.Negative) {
                            result.Add("+");
                        }
                        else if (new SplitElementType[] {
                            SplitElementType.BracketsOpen,
                            SplitElementType.Number,
                            SplitElementType.Word
                        }.Contains(currentType)) {

                        }
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == SplitElementType.Equality) {//                                  Equality
                    if (new SplitElementType[] {
                            SplitElementType.Operator,
                            SplitElementType.Comma,
                            SplitElementType.Factorial
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    else if (currentType == SplitElementType.BracketsClose) {
                        currentType = previousType;
                        beforePreviousType = SplitElementType.Equality; // stupid way to indicate that this happened
                    }
                    else if (currentType == SplitElementType.Equality) {
                        if (beforePreviousType == SplitElementType.Factorial) {
                            result.Add("!");
                            currentString = "=" + item;
                        }
                        else if (beforePreviousType == SplitElementType.Equality) {// catches if Equality is > 2 chars long
                            throw new InvalidEquationException();
                        }
                        else {
                            beforePreviousType = SplitElementType.Equality;
                            currentString += item;
                        }
                    }
                    else {
                        result.Add(currentString);
                        if (currentType == SplitElementType.BracketsOpen) {
                            beforePreviousType = SplitElementType.Equality;
                        }
                        if (currentType == SplitElementType.Negative) {
                            currentType = SplitElementType.Number;
                        }
                        currentString = string.Empty + item;
                    }
                }
                else if (previousType == SplitElementType.Comma) {//                                     Comma
                    if (new SplitElementType[] {
                            SplitElementType.Operator,
                            SplitElementType.BracketsClose,
                            SplitElementType.Equality,
                            SplitElementType.Factorial,
                            SplitElementType.Comma
                        }.Contains(currentType)) {
                        throw new InvalidEquationException();
                    }
                    result.Add(currentString);
                    if (currentType == SplitElementType.Negative) {
                        currentType = SplitElementType.Number;
                    }
                    currentString = string.Empty + item;
                }
                else if (previousType == SplitElementType.Negative) {
                    if (currentType == SplitElementType.Number) {
                        currentString += item;
                    }
                    else if (currentType == SplitElementType.BracketsOpen || currentType == SplitElementType.Word) {
                        result.Add(currentString + "1");
                        result.Add("*");
                        currentString = string.Empty + item;
                    }
                    else {
                        throw new InvalidEquationException();
                    }
                }
                previousType = currentType;
            }
            if (new SplitElementType[] {
                            SplitElementType.Operator,
                            SplitElementType.Equality,
                            SplitElementType.Comma,
                            SplitElementType.Null
                        }.Contains(previousType)) {
                throw new InvalidEquationException();
            }
            else if (previousType != SplitElementType.BracketsOpen) {
                result.Add(currentString);
            }
            return result.ToArray();
        }

        public static List<Element> DecodeEquationLegacyCode(string input) { 
            List<Element> result = new List<Element>();
            if (input.Length < 1) {
                throw new InvalidEquationException();
            }

            List<Bracket> openBrackets = new List<Bracket>();
            List<Function> twoArgumentFunctions = new List<Function>();
            int additionalPriority = 0;
            bool isEqualitySign = false;
            List<int> commas = new List<int>();
            commas.Add(0);
            Element previousElement = null;

            string[] array = SplitLegacyCode(input);

            foreach (var item in array) {
                if (item[0] == '(') {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Constant
                         || previousElement.elementType == ElementType.Parameter
                         || previousElement.elementType == ElementType.Unknown) {
                            result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                        }
                    }
                    var bracket = new Bracket(BracketType.Opening);
                    commas.Add(0);
                    openBrackets.Add(bracket);
                    result.Add(bracket);
                    additionalPriority++;
                    previousElement = result.Last();
                }
                else if (item[0] >= '0' && item[0] <= '9' || item[0] == '-') {// SM - B.Close, Fact, Number, Const/P
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Constant
                         || previousElement.elementType == ElementType.Parameter
                         || previousElement.elementType == ElementType.Unknown) {
                            result.Add(new Operation(OperationType.Multiplication, additionalPriority));
                        }
                    }
                    result.Add(new Number(double.Parse(item, CultureInfo.InvariantCulture)));
                    previousElement = result.Last();
                }
                else if (item.ToLower()[0] >= 'a' && item.ToLower()[0] <= 'z') {
                    var temp = new WordInterpreter(item, additionalPriority).InterpretWord();
                    if (previousElement != null) {
                        if ((previousElement.elementType == ElementType.Number ? (previousElement as Number).value < 0 : false) &&
                            (temp.Last().elementType == ElementType.Constant || temp.Last().elementType == ElementType.Parameter || temp.Last().elementType == ElementType.Unknown)) {
                            result.Add(new Operation(OperationType.Addition, additionalPriority));
                        }
                    }
                    foreach (var subitem in temp) {
                        result.Add(subitem);
                    }
                    previousElement = result.Last();
                }
                else if (operators.Contains(item[0])) {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Function) {
                            throw new InvalidEquationException();
                        }
                    }
                    result.Add(new Operation((OperationType)Operation.CharToOperationType(item[0]), additionalPriority));
                    previousElement = result.Last();
                }
                else if (item[0] == ')') {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Function) {
                            throw new InvalidEquationException();
                        }
                    }
                    if (openBrackets.Count == 0) {
                        if (!isEqualitySign) {
                            var bracket = new Bracket(BracketType.Opening);
                            foreach (var subitem in result) {
                                if (subitem.elementType == ElementType.Operation || subitem.elementType == ElementType.Function) {
                                    (subitem as Prioritabel).priority = (int)Math.Log((subitem as Prioritabel).priority, Operation.MAX_PRIORITY) + 1;
                                }
                            }
                            result.Insert(0, bracket);
                            result.Add(new Bracket(BracketType.Closing, bracket));
                        }
                        else {
                            int equalityIndex = result.FindLastIndex(c => { return c.elementType == ElementType.Equality; });
                            var bracket = new Bracket(BracketType.Opening);
                            foreach (var subitem in result) {
                                if ((subitem.elementType == ElementType.Operation || subitem.elementType == ElementType.Function) && result.IndexOf(subitem) > equalityIndex) {
                                    (subitem as Prioritabel).priority = (int)Math.Log((subitem as Prioritabel).priority, Operation.MAX_PRIORITY) + 1;
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
                    previousElement = result.Last();
                }
                else if (item[0] == '=' || item[0] == '<' || item[0] == '>') {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Function) {
                            throw new InvalidEquationException();
                        }
                    }
                    if ((result.Last().elementType == ElementType.Operation) ? (result.Last() as Operation).operationType == OperationType.Multiplication : false) {
                        result.RemoveAt(result.Count - 1);
                    }
                    if (openBrackets.Count > 0) {
                        foreach (var subitem in openBrackets) {
                            result.Remove(subitem);
                        }
                        openBrackets.Clear();
                        additionalPriority = 0;
                    }
                    result.Add(new Equality((EqualityType)Equality.StringToEqualityType(item)));
                    isEqualitySign = true;
                    previousElement = result.Last();
                }
                else if (item[0] == ',') {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Function) {
                            throw new InvalidEquationException();
                        }
                    }
                    if (commas.Last() == Comma.MAX_COMMAS) {
                        throw new InvalidEquationException();
                    }
                    result.Add(new Comma());
                    commas[commas.Count - 1]++;
                    previousElement = result.Last();
                }
                else if (item[0] == '!') {
                    if (previousElement != null) {
                        if (previousElement.elementType == ElementType.Function) {
                            throw new InvalidEquationException();
                        }
                    }
                    result.Add(new Operation(OperationType.Factorial, additionalPriority));
                    previousElement = result.Last();
                }
            }
            if (openBrackets.Count > 0) {
                int count = openBrackets.Count;
                foreach (var subitem in openBrackets) {
                    result.Add(new Bracket(BracketType.Closing, subitem));
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
            if (result.Last().elementType == ElementType.Function) {
                throw new InvalidEquationException();
            }
            return result;
        }


        public static List<Element> DecodeEquationIP(string input) {
            InputEquationDecoder decoder = new InputEquationDecoder(input);
            decoder.DecodeEquation();
            return decoder.decodedEquation;
        }

        public struct MessageData {

            public string beforeString { get; set; }
            public string afterString { get; set; }
            public ChangeHandler change;

            public MessageData(ChangeHandler _change, List<Element> equation = null, int openBracketIndex = 0, int closeBracketIndex = 0, string beforeBeforeString = "", string afterAfterString = "", bool withFunction = false) {
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
            public MessageData(ChangeHandler _change) {
                change = _change;
                beforeString = string.Empty;
                afterString = string.Empty;
            }
            public MessageData(ChangeHandler _change, List<Element> beforeEquation = null, List<Element> equation = null, int openBracketIndex = 0, int closeBracketIndex = 0, string beforeBeforeString = "", string afterAfterString = "", bool withFunction = false) {
                change = _change;
                beforeString = beforeBeforeString;
                afterString = string.Empty;
                if (equation != null && closeBracketIndex != 0) {
                    foreach (var item in beforeEquation) {
                        beforeString += item.ToString();
                    }
                    if (withFunction) {
                        beforeString += equation[openBracketIndex - 1].ToString();
                    }
                    beforeString += equation[openBracketIndex].ToString();
                    for (int i = closeBracketIndex; i < equation.Count; i++) {
                        afterString += equation[i].ToString();
                    }
                }
                afterString += afterAfterString;
            }
            public string ToString(List<Element> equation) {
                string result = (beforeString != null ? String.Copy(beforeString) : string.Empty);
                foreach (var item in equation) {
                    result += item.ToString();
                }
                return result + (afterString ?? string.Empty);
            }
        }

        public static Number Simplify(List<Element> argument, MessageData messageData, Function function = null) {
            var simplifyied = new List<Element>();
            if (argument.Count(c => { return new ElementType[] { ElementType.Unknown, ElementType.Equality, ElementType.Parameter }.Contains(c.elementType); }) > 0) {
                throw new WrongSolverChoosenException(SolvingMode.Simplyfication);
            }
            for (int i = 0; i < argument.Count; i++) {
                if (argument[i].elementType == ElementType.Brackets) {
                    var temp = new List<Element>();
                    int endIndex = argument.IndexOf((argument[i] as Bracket).enclosureReference);
                    for (int j = i + 1; j < endIndex; j++) {
                        temp.Add(argument[j]);
                    }
                    if (i != 0 ? argument[i - 1].elementType == ElementType.Function : false) {
                        simplifyied.RemoveAt(simplifyied.Count - 1);
                        simplifyied.Add(Simplify(temp, new MessageData(messageData.change, simplifyied, argument, i, endIndex, messageData.beforeString, messageData.afterString, true), argument[i - 1] as Function));
                    }
                    else {
                        simplifyied.Add(Simplify(temp, new MessageData(messageData.change, simplifyied, argument, i, endIndex, messageData.beforeString, messageData.afterString)));
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
            messageData.change?.Invoke(messageData.ToString(simplifyied));
            var commas = simplifyied.Count(e => { return e.elementType == ElementType.Comma; });
            while (simplifyied.Count - 2 * commas > 1) {
                var maxPriority = (from op in simplifyied where op.elementType == ElementType.Operation || op.elementType == ElementType.Function select op as Prioritabel).ToArray().Max((Prioritabel x) => { return x.priority; });
                for (int i = 0; i < simplifyied.Count; i++) {
                    if (simplifyied[i].elementType == ElementType.Operation) {
                        var element = simplifyied[i] as Operation;
                        if (element.priority == maxPriority) {
                            var array = simplifyied.ToArray();
                            if (element.operationType == OperationType.Factorial) {
                                var number = element.Execute(new Number[] { simplifyied[i - 1] as Number });
                                simplifyied.Insert(i, number);
                                simplifyied.Remove(array[i - 1]);
                                simplifyied.Remove(element);
                                i = simplifyied.IndexOf(number);
                                messageData.change?.Invoke(messageData.ToString(simplifyied));
                            }
                            else {
                                var number = element.Execute(new Number[] { simplifyied[i - 1] as Number, simplifyied[i + 1] as Number });
                                simplifyied.Insert(i, number);
                                simplifyied.Remove(array[i - 1]);
                                simplifyied.Remove(array[i + 1]);
                                simplifyied.Remove(element);
                                i = simplifyied.IndexOf(number);
                                messageData.change?.Invoke(messageData.ToString(simplifyied));
                            }
                        }
                    }
                    else if (simplifyied[i].elementType == ElementType.Function) {
                        var element = simplifyied[i] as Function;
                        if (element.priority == maxPriority) {
                            var array = simplifyied.ToArray();
                            var number = element.Execute(new Number[] { simplifyied[i + 1] as Number });
                            simplifyied.Insert(i, number);
                            simplifyied.Remove(array[i + 1]);
                            simplifyied.Remove(element);
                            i = simplifyied.IndexOf(number);
                            messageData.change?.Invoke(messageData.ToString(simplifyied));
                        }
                    }
                }
            }
            if (function != null) {
                var arguments = (from op in simplifyied where op.elementType == ElementType.Number select op as Number).ToArray();
                return function.Execute(arguments);
            }
            return simplifyied[0] as Number;
        }


    }
}

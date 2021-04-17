using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;


namespace Interpreter_Równań {
    class InputEquationDecoder {
        private List<Bracket> openBrackets = new List<Bracket>();
        private List<Function> twoArgumentFunctions = new List<Function>();
        private int additionalPriority = 0;
        private bool isEqualitySign = false;
        private List<int> commas = new List<int>();
        private Element previousElement = null;
        public readonly string[] splitedInputEquation;
        public List<Element> decodedEquation { get; } = new List<Element>();


        public InputEquationDecoder(string inputEquation) {
            if (inputEquation.Length < 1) {
                throw new InvalidEquationException();
            }
            commas.Add(0);

            Splitter splitter = new Splitter(inputEquation);
            splitedInputEquation = splitter.Split();
        }


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

        private class Splitter {

            string input;

            List<string> splitedString = new List<string>();
            string currentString = string.Empty;
            bool isDot = false;
            SplitElementType previousType = SplitElementType.Null;
            SplitElementType beforePreviousType = SplitElementType.Null;

            public Splitter(string input) {
                this.input = input;
            }

            public string[] Split() {

                foreach (var character in input) {

                    SplitElementType currentType = currentTypeDetection(character);



                    if (previousType == SplitElementType.Null) {//                                          Null
                        currentString = string.Empty + character;
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
                        if (currentType == SplitElementType.Operator ||
                            currentType == SplitElementType.Comma ||
                            currentType == SplitElementType.Factorial) {
                            throw new InvalidEquationException();
                        }
                        if (currentType == SplitElementType.BracketsClose) {
                            currentString = string.Empty;
                            previousType = beforePreviousType;
                            continue;
                        }
                        else if (currentType == SplitElementType.Equality) {
                            currentString = string.Empty + character;
                        }
                        else {
                            splitedString.Add(currentString);
                            currentString = string.Empty + character;
                        }
                    }
                    else if (previousType == SplitElementType.Number) {//                                   Number
                        if (currentType == previousType) {
                            currentString += character;
                        }
                        else {
                            splitedString.Add(currentString);
                            if (currentType == SplitElementType.BracketsOpen ||
                                currentType == SplitElementType.Word) {
                                splitedString.Add("*");
                            }
                            if (currentType == SplitElementType.Negative) {
                                splitedString.Add("+");
                                currentType = SplitElementType.Number;
                            }
                            currentString = string.Empty + character;
                            isDot = false;
                        }
                    }
                    else if (previousType == SplitElementType.Word) {//                                     Word
                        if (currentType == previousType) {
                            currentString += character;
                        }
                        else {
                            splitedString.Add(currentString);
                            currentString = string.Empty + character;
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
                            splitedString.Add(currentString);
                            currentString = string.Empty + character;
                            if (currentType == SplitElementType.Negative) {
                                currentType = SplitElementType.Number;
                            }
                        }
                    }
                    else if (previousType == SplitElementType.BracketsClose) {//                             Brackets Close
                        splitedString.Add(currentString);
                        if (new SplitElementType[] {
                                SplitElementType.Number,
                                SplitElementType.BracketsOpen,
                                SplitElementType.Word
                            }.Contains(currentType)) {
                            splitedString.Add("*");
                        }
                        if (currentType == SplitElementType.Negative) {
                            splitedString.Add("+");
                        }
                        currentString = string.Empty + character;
                    }
                    else if (previousType == SplitElementType.Factorial) {//                                 Factorial
                        if (currentType == SplitElementType.Equality) {
                            currentString += character;
                        }
                        else {
                            splitedString.Add(currentString);
                            if (currentType == SplitElementType.Negative) {
                                splitedString.Add("+");
                            }
                            else if (new SplitElementType[] {
                                SplitElementType.BracketsOpen,
                                SplitElementType.Number,
                                SplitElementType.Word
                            }.Contains(currentType)) {

                            }
                            currentString = string.Empty + character;
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
                                splitedString.Add("!");
                                currentString = "=" + character;
                            }
                            else if (beforePreviousType == SplitElementType.Equality) {// catches if Equality is >2 chars long
                                throw new InvalidEquationException();
                            }
                            else {
                                beforePreviousType = SplitElementType.Equality;
                                currentString += character;
                            }
                        }
                        else {
                            splitedString.Add(currentString);
                            if (currentType == SplitElementType.BracketsOpen) {
                                beforePreviousType = SplitElementType.Equality;
                            }
                            if (currentType == SplitElementType.Negative) {
                                currentType = SplitElementType.Number;
                            }
                            currentString = string.Empty + character;
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
                        splitedString.Add(currentString);
                        if (currentType == SplitElementType.Negative) {
                            currentType = SplitElementType.Number;
                        }
                        currentString = string.Empty + character;
                    }
                    else if (previousType == SplitElementType.Negative) {
                        if (currentType == SplitElementType.Number) {
                            currentString += character;
                        }
                        else if (currentType == SplitElementType.BracketsOpen || currentType == SplitElementType.Word) {
                            splitedString.Add(currentString + "1");
                            splitedString.Add("*");
                            currentString = string.Empty + character;
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
                    splitedString.Add(currentString);
                }
                return splitedString.ToArray();
            }

            private SplitElementType currentTypeDetection(char character) {
                SplitElementType result = SplitElementType.Null;

                if (operators.Contains(character)) {
                    result = SplitElementType.Operator;
                }
                else if (character == '(') {
                    result = SplitElementType.BracketsOpen;
                    beforePreviousType = previousType;
                }
                else if (character == ')') {
                    result = SplitElementType.BracketsClose;
                }
                else if (character == '-') {
                    result = SplitElementType.Negative;
                }
                else if (character >= '0' && character <= '9' || character == '.') {
                    result = SplitElementType.Number;
                    if (character == '.' && (isDot || currentString.Length == 0)) {
                        throw new InvalidEquationException();
                    }
                }
                else if (character >= 'a' && character <= 'z' || character >= 'A' && character <= 'Z') {
                    result = SplitElementType.Word;
                }
                else if (character == '=' || character == '<' || character == '>') {
                    result = SplitElementType.Equality;
                    if (previousType == SplitElementType.Factorial) {
                        beforePreviousType = SplitElementType.Factorial;
                    }
                }
                else if (character == '!') {
                    result = SplitElementType.Factorial;
                }
                else if (character == ',') {
                    result = SplitElementType.Comma;
                }
                else {
                    throw new InvalidEquationException();
                }
                return result;
            }

        }



        public List<Element> DecodeEquation() {
            foreach (var item in splitedInputEquation) {
                SwitchStatement(item);
            }

            CloseOpenBrackets();

            if (isEqualitySign) {// removes unnessesery brackets to avoid sth like this: (((X)))
                var equalityCount = GetEqualityCount(decodedEquation);
                var equalitySignIndex = GetEqualitySignIndex(equalityCount);
                for (int i = 0; i < equalityCount + 1; i++) {
                    while (decodedEquation[i == 0 ? 0 : equalitySignIndex[i - 1] + 1] is Bracket && 
                           decodedEquation[equalitySignIndex[i]] is Bracket) { // checks whether both ends are brackets
                        decodedEquation.RemoveAt(i == 0 ? 0 : equalitySignIndex[i - 1]);
                        decodedEquation.RemoveAt(equalitySignIndex[i]);
                    }
                }
                //  This needs to stay here until equality is implemented, dont know which version of the code is correct
                //
                //for (int i = 0; i <= equalityCount + 1; i++) {
                //    while (decodedEquation[i == 0 ? 0 : equalitySignIndex[i - 1]] is Bracket &&
                //           decodedEquation[i == equalityCount + 1 ? decodedEquation.Count - 1 : equalitySignIndex[i]] is Bracket) {
                //        decodedEquation.RemoveAt(i == 0 ? 0 : equalitySignIndex[i - 1]);
                //        decodedEquation.RemoveAt(i == equalityCount + 1 ? decodedEquation.Count - 1 : equalitySignIndex[i]);
                //    }
                //}
            }
            else {
                while (decodedEquation.IndexOf((decodedEquation[0] as Bracket).enclosureReference) == decodedEquation.Count - 1) {
                    decodedEquation.RemoveAt(decodedEquation.Count - 1);
                    decodedEquation.RemoveAt(0);
                }
            }
            if (decodedEquation.Last() is Function) {
                throw new InvalidEquationException();
            }
            return decodedEquation;
        }

        private int[] GetEqualitySignIndex(int equalityCount) {
            int[] equalitySignIndex;
            if (equalityCount == 0) {
                equalitySignIndex = new int[] { decodedEquation.Count - 1 };
            }
            else {
                equalitySignIndex = GetEqualitySignIndexArray(decodedEquation, equalityCount);
            }

            return equalitySignIndex;
        }

        private static int[] GetEqualitySignIndexArray(List<Element> result, int equalityCount) {
            int[] equalitySignIndex = new int[equalityCount];
            equalitySignIndex[0] = result.FindIndex(c => { return c.elementType == ElementType.Equality; });
            for (int i = 1; i < equalitySignIndex.Length; i++) {
                equalitySignIndex[i] = result.FindIndex(equalitySignIndex[i - 1] + 1, c => { return c.elementType == ElementType.Equality; });
            }

            return equalitySignIndex;
        }

        private static int GetEqualityCount(List<Element> result) => result.Count(c => { return c.elementType == ElementType.Equality; });

        private void SwitchStatement(string equationElement) {
            if (equationElement[0] == '(') {
                OpenBracketCase();
            }
            else if (equationElement[0] >= '0' && equationElement[0] <= '9' || equationElement[0] == '-') {// SM - B.Close, Fact, Number, Const/P
                NumberCase(equationElement);
            }
            else if (equationElement.ToLower()[0] >= 'a' && equationElement.ToLower()[0] <= 'z') {
                WordCase(equationElement);
            }
            else if (operators.Contains(equationElement[0])) {
                OperatorCase((OperationType)Operation.CharToOperationType(equationElement[0]));
            }
            else if (equationElement[0] == ')') {
                ClosingBracketCase();
            }
            else if (equationElement[0] == '=' || equationElement[0] == '<' || equationElement[0] == '>') {
                EqualityCase((EqualityType)Equality.StringToEqualityType(equationElement));
            }
            else if (equationElement[0] == ',') {
                CommaCase();
            }
            else if (equationElement[0] == '!') {
                FactorioCase();
            }

        }

        private void FactorioCase() {
            if (previousElement is Function) {
                throw new InvalidEquationException();
            }
            decodedEquation.Add(new Operation(OperationType.Factorial, additionalPriority));
            previousElement = decodedEquation.Last();
        }

        private void CommaCase() {
            if (previousElement is Function) {
                throw new InvalidEquationException();
            }
            if (commas.Last() == Comma.MAX_COMMAS) {
                throw new InvalidEquationException();
            }
            decodedEquation.Add(new Comma());
            commas[commas.Count - 1]++;
            previousElement = decodedEquation.Last();
        }

        private void ClosingBracketCase() {
            if (previousElement is Function) {
                throw new InvalidEquationException();
            }
            if (openBrackets.Count == 0) {
                if (!isEqualitySign) {
                    InsertMissingBracket(0);
                }
                else {
                    var equalityIndex = GetLastEqualityIndex();
                    InsertMissingBracket(equalityIndex + 1);
                }
            }
            else {
                var bracket = openBrackets.Last();
                openBrackets.Remove(bracket);
                commas.Remove(commas.Last());
                decodedEquation.Add(new Bracket(BracketType.Closing, bracket));
                additionalPriority--;
            }
            previousElement = decodedEquation.Last();
        }

        private int GetLastEqualityIndex() => decodedEquation.FindLastIndex(c => { return c.elementType == ElementType.Equality; });

        private void InsertMissingBracket(int insertionIndex) {
            var bracket = new Bracket(BracketType.Opening);
            foreach (var equationElement in decodedEquation) {
                if (equationElement is Prioritabel) {
                    (equationElement as Prioritabel).priority = (int)Math.Log((equationElement as Prioritabel).priority, Operation.MAX_PRIORITY) + 1;
                }
            }
            decodedEquation.Insert(insertionIndex, bracket);
            decodedEquation.Add(new Bracket(BracketType.Closing, bracket));
        }

        private void OperatorCase(OperationType operationType) {
            if (previousElement is Function) {
                throw new InvalidEquationException();
            }
            decodedEquation.Add(new Operation(operationType, additionalPriority));
            previousElement = decodedEquation.Last();
        }

        private void WordCase(string word) {
            List<Element> interpretedWord = new WordInterpreter(word, additionalPriority).InterpretWord();
            var previousElementAsNumber = previousElement as Number;
            if ((previousElementAsNumber is null ? false : previousElementAsNumber.value < 0) &&
                (interpretedWord.Last() is Constant ||
                 interpretedWord.Last() is Variable )) {                                                  // TODO Needs looking into
                decodedEquation.Add(new Operation(OperationType.Addition, additionalPriority));
            }
            decodedEquation.AddRange(interpretedWord);
            previousElement = decodedEquation.Last();
        }

        private void NumberCase(string argument) {
            if (previousElement is Constant ||
                previousElement is Variable) {
                decodedEquation.Add(new Operation(OperationType.Multiplication, additionalPriority));
            }
            decodedEquation.Add(new Number(double.Parse(argument, CultureInfo.InvariantCulture)));
            previousElement = decodedEquation.Last();
        }

        private void OpenBracketCase() {
            if (previousElement is Constant ||
                previousElement is Variable) {
                decodedEquation.Add(new Operation(OperationType.Multiplication, additionalPriority));
            }
            var bracket = new Bracket(BracketType.Opening);
            commas.Add(0);
            openBrackets.Add(bracket);
            decodedEquation.Add(bracket);
            additionalPriority++;
            previousElement = decodedEquation.Last();
        }

        private void EqualityCase(EqualityType equalityType) {
            if (previousElement is Function) {
                throw new InvalidEquationException();
            }

            Operation previousElementAsOperation = previousElement as Operation;    // is it nessesery?
            if (previousElementAsOperation is null ? false : previousElementAsOperation.operationType == OperationType.Multiplication) {
                decodedEquation.RemoveAt(decodedEquation.Count - 1);
            }

            CloseOpenBrackets();
            decodedEquation.Add(new Equality(equalityType));
            isEqualitySign = true;
            previousElement = decodedEquation.Last();
        }

        private void CloseOpenBrackets() {
            if (openBrackets.Count > 0) {
                foreach (var subitem in openBrackets) {
                    decodedEquation.Add(new Bracket(BracketType.Closing, subitem));
                }
                openBrackets.Clear();
                additionalPriority = 0;
            }
        }
    }
}

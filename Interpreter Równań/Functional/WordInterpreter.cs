using System;
using System.Linq;
using System.Collections.Generic;

namespace Interpreter_Równań {

    public class WordInterpreter {

        private string input;
        private int additionalPriority;

        private List<Element> interpretedWord = new List<Element>();
        private ElementType lastAddedElement = ElementType.Comma;
        private List<FunctionIndex> functionIndex = null;
        private int toSubtract = 0;

        public WordInterpreter(string input, int additionalPriority) {
            this.input = input;
            this.additionalPriority = additionalPriority;
        }

        private class FunctionIndex: IComparable<FunctionIndex> {

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

        public List<Element> InterpretWord() {
            if (input.Length == 0) {
                throw new NullReferenceException("Input of InterpretWord(string input) cannot have length of 0.");
            }
            InterpretWordImp();
            return interpretedWord;
        }

        private void InterpretWordImp() {
            while (input.Length > 0) {
                functionIndex = FunctionIndex.GetArray(input);
                HandleConstants();
                HandleParameters();
                // jesli wszystko zadzaialalo to w tym momencie funkcja powinna zaczyac sie na indexie 0
                if (functionIndex != null) {
                    AddNecessaryMultiplications();
                    interpretedWord.Add(new Function(functionIndex[0].functionType, additionalPriority));
                    lastAddedElement = ElementType.Function;
                    input = input.Remove(0, Function.FunctionTypeToString((FunctionType)functionIndex[0].functionType).Length);
                }
            }
        }

        private void HandleConstants() {
            ConstantType? constant = Constant.StringToConstantType(input, 0, functionIndex != null ? functionIndex[0].index : input.Length);
            if (constant != null) {
                var powerArray = getPowersOfConstantsAndRemoveCountedOnes(constant);
                ConstantsFillIn(powerArray);
            }
        }

        private void HandleParameters() {
            foreach (var item in input.Substring(0, CharactersToSubtractCounter())) {
                var power = input.Substring(0, CharactersToSubtractCounter()).Count(c => { return c == item; });
                AddNecessaryMultiplications();

                interpretedWord.Add(new Variable(item, power));
                lastAddedElement = ElementType.Parameter;

                RemoveHandledParameters(item);
            }
        }

        private void RemoveHandledParameters(char item) {
            while (input.IndexOf(item, 0, (CharactersToSubtractCounter())) != -1) {
                input = input.Remove(input.IndexOf(item), 1);
                toSubtract++;
            }
        }

        private void AddNecessaryMultiplications() {
            if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                interpretedWord.Add(new Operation(OperationType.Multiplication, additionalPriority));
            }
        }

        private string RemoveRedundendConstantsFromInputString(ConstantType constant, int numberOfCharactersToExamine) {
            return input.Remove(input.IndexOf(Constant.ConstantTypeToString(constant), 0, numberOfCharactersToExamine),
                                Constant.ConstantTypeToString(constant).Length);
        }

        private int CharactersToSubtractCounter() {
            if (functionIndex != null) {
                return functionIndex[0].index - toSubtract;
            }
            else {
                return input.Length;
            }
        }

        private List<Element> InputWithoutFunctionCase() {
            HandleConstants();
            HandleParameters();
            return interpretedWord;
        }

        private void ConstantsFillIn(int[] powerArray) {
            for (int i = 0; i < powerArray.Length; i++) {
                if (powerArray[i] > 0) {
                    if (lastAddedElement == ElementType.Constant || lastAddedElement == ElementType.Parameter) {
                        interpretedWord.Add(new Operation(OperationType.Multiplication, additionalPriority));
                    }
                    interpretedWord.Add(new Constant((ConstantType)i, powerArray[i]));
                    lastAddedElement = ElementType.Constant;
                }
            }
        }

        private int[] getPowersOfConstantsAndRemoveCountedOnes(ConstantType? constant) {
            int[] powerArray = new int[Constant.array.Length];
            while (constant != null) {
                powerArray[(int)constant]++;
                input = RemoveRedundendConstantsFromInputString((ConstantType)constant, CharactersToSubtractCounter());
                toSubtract += Constant.ConstantTypeToString((ConstantType)constant).Length;
                constant = Constant.StringToConstantType(input, 0, CharactersToSubtractCounter());
            }

            return powerArray;
            /*
            int[] powerArray = new int[Constant.array.Length];
            while (constant != null) {
                powerArray[(int)constant]++;
                input = RemoveRedundendConstantsFromInputString((ConstantType)constant, input.Length);
                constant = Constant.StringToConstantType(input);
            }

            return powerArray;*/
        }


    }
}

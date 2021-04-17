using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Równań {
    public delegate void ChangeHandler(List<Element> elements);

    class Simplifier {
        List<Element> input;

        public ChangeHandler output;


        public Simplifier(List<Element> equationToSimplify, ChangeHandler func) {
            input = equationToSimplify;
            output = func;
            if (ContainsUnsupportedElements()) {
                throw new WrongSolverChoosenException(SolvingMode.Simplyfication);
            }
        }

        public Number Simplify() {
            Bracket bracket = getLastOpenBracket();
            while (bracket != null) {
                int indexOfBracket = input.IndexOf(bracket);
                SimplifyPart(indexOfBracket,  input.IndexOf(bracket.enclosureReference));
                if (indexOfBracket > 0) HandleFunctionConectedtobrackets(bracket, indexOfBracket);
                input.Remove(bracket.enclosureReference);
                input.Remove(bracket);
                bracket = getLastOpenBracket();
            }
            SimplifyPart(0, input.Count);
            return input[0] as Number;
        }

        private void HandleFunctionConectedtobrackets(Bracket bracket, int indexOfBracket) {
            if (input[indexOfBracket - 1] is Function) {
                var list = input.GetRange(indexOfBracket, input.IndexOf(bracket.enclosureReference) - indexOfBracket + 1);
                var arguments = (from op in list where op is Number select (op as Number)).ToArray();
                var result = (input[indexOfBracket - 1] as Function).Execute(arguments);
                RemoveArguments(list);
                input.Insert(indexOfBracket - 1, result);
                OutputChange();
            }
        }

        private void RemoveArguments(List<Element> list) {
            var arrayToRemove = (from op in list where !(op is Bracket) select op).ToArray();
            foreach (var item in arrayToRemove) {
                input.Remove(item);
            }
        }

        private Bracket getLastOpenBracket() {
            return input.FindLast(e => {
                Bracket b = e as Bracket;
                return (b is null) ? false : b.bracketType == BracketType.Opening;
            }) as Bracket;
        }
        
        private void SimplifyPart(int beginIndex, int endIndex) {
            var prioritableList = GetPrioritabels(input.GetRange(beginIndex, endIndex - beginIndex + 1));
            foreach (var prioritable in prioritableList) {
                Element[] array = input.ToArray();
                int index = input.IndexOf(prioritable);
                if (prioritable is Function) {
                    HandleFunctionCase(prioritable, array, index);
                }
                else {
                    HandleOperationCase(prioritable, array, index);
                }
            }
        }

        private void HandleOperationCase(Prioritabel prioritable, Element[] array, int index) {
            Number result = prioritable.Execute(new Number[] { array[index - 1] as Number, array[index + 1] as Number });
            input.Insert(index, result);
            input.Remove(prioritable);
            input.Remove(array[index + 1]);
            input.Remove(array[index - 1]);
            OutputChange();
        }

        private void HandleFunctionCase(Prioritabel prioritable, Element[] array, int index) {
            Number result = prioritable.Execute(new Number[] { array[index + 1] as Number });
            input.Insert(index, result);
            input.Remove(prioritable);
            input.Remove(array[index + 1]);
            OutputChange();
        }

        private static List<Prioritabel> GetPrioritabels(List<Element> simplifyied) => (
            (from p in
                 from op in simplifyied
                 where op is Prioritabel
                 select op as Prioritabel
             orderby p.priority descending
             select p).ToList()
            );
        
        public void OutputChange() {
            output?.Invoke(input);
        }

        private  bool ContainsUnsupportedElements() {
            int unsupportedElementCount = input.Count(c => { return new ElementType[] {
                ElementType.Unknown,
                ElementType.Equality,
                ElementType.Parameter
            }.Contains(c.elementType); });
            return unsupportedElementCount > 0;
        }
    }
}

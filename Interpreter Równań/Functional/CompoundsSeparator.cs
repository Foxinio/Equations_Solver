using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Równań.Functional {
    class CompoundsSeparator {
        private List<Element> inputList;
        private List<Operation> multiplicationsReferencesList;

        CompoundsSeparator(List<Element> input) {
            inputList = input;
            multiplicationsReferencesList = GetAllMultiplicationsWithBrackets(input);
        }

        private List<Operation> GetAllMultiplicationsWithBrackets(List<Element> input) =>
                    (from op in input
                    where op is Operation ? 
                        (op as Operation).operationType == OperationType.Multiplication && 
                        (input[input.IndexOf(op) - 1] is Bracket || input[input.IndexOf(op) + 1] is Bracket) 
                     : false 
                    select op as Operation).ToList();

        public List<List<Element>> Seperate() {
            if (multiplicationsReferencesList.Count > 0) {
                foreach (var item in multiplicationsReferencesList) {
                    Element preElement = inputList[inputList.IndexOf(item) - 1];
                    bool isPreElementBracket = preElement is Bracket;

                    Element postElement = inputList[inputList.IndexOf(item) + 1];
                    bool isPostElementBracket = postElement is Bracket;
                    if(isPreElementBracket && isPostElementBracket) {
                        
                    }
                    else {

                    }
                }
            }
            return null;
        }
    }
}

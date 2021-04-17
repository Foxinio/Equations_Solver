using System;
namespace Interpreter_Równań {

    public class Variable: Element {// TODO refactor this so that Constant : Variable

        public char symbol;
        public double power;


        public Variable(char _symbol, double _power, ElementType _elementType = ElementType.Parameter) {
            symbol = _symbol;
            power = _power;
            if (_elementType != ElementType.Parameter && _elementType != ElementType.Unknown) {
                throw new FormatException();
            }
            elementType = _elementType;
        }


        public override string ToString() {
            return symbol.ToString();
        }
    }
}

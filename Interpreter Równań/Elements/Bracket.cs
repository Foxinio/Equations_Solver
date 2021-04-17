
namespace Interpreter_Równań {

    public enum BracketType { Opening, Closing };

    public class Bracket: Element {

        public Bracket enclosureReference { get; set; }
        public BracketType bracketType { get; }


        public Bracket(BracketType _bracketType, Bracket otherSide = null) {
            elementType = ElementType.Brackets;
            bracketType = _bracketType;
            enclosureReference = otherSide;
            if (otherSide != null) {
                otherSide.enclosureReference = this;
            }
        }


        public override string ToString() {
            switch (bracketType) {
            case BracketType.Opening:
                return "(";
            case BracketType.Closing:
                return ")";
            default:
                return "";
            }
        }
    }
}

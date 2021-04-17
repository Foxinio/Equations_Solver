using System.Linq;

namespace Interpreter_Równań {

    public enum EqualityType { EqualTo, LessThan, MoreThan, LessOrEqualTo, MoreOrEqualTo, Compare }

    public class Equality: Element {    // TODO Expand this

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
                if (item.Equals(input)) {
                    return (EqualityType)(Equality.array.ToList().IndexOf(item));
                }
            }
            return null;
        }


        public override string ToString() {
            return EqualityTypeToString(equalityType);
        }

    }

}

using System;
namespace Interpreter_Równań {

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



    public enum SolvingMode { Simplyfication };

    [Serializable]
    public class WrongSolverChoosenException: Exception {
        public static string[] typeNames = new string[] { "Symplification" };
        static string[] defaultMessage = new string[] { "Wrong method of solving choosen, ", " cannot do anything with your expression." };
        public WrongSolverChoosenException(
            SolvingMode typeInUse) :
                base(String.Format("{0}{1}{2}",
                    defaultMessage[0], typeNames[(int)typeInUse], defaultMessage[1])) {
        }
    }

}

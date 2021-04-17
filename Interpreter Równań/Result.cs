namespace Interpreter_Równań {

    public struct Result {

        public bool success;
        public string message;


        public Result(bool _success = true, string _message = null) {
            success = _success;
            message = _message;
        }

    }
}

namespace Gorgon.Core.Test.Support
{
    class InternalActivatorTestClass
    {
        public int TestThis()
        {
            return 123;
        }
    }

    public class InnerActivatorTestClass
    {
        public class InnerClass
        {
            public int TestThis()
            {
                return 789;
            }
        }
    }

    public class ActivatorTestClass
    {
        public int TestThis()
        {
            return 111;
        }
    }

    public class ActivatorTestClassMultipleParams
    {
        private int _value1;
        private int _value2;

        public int TestThis()
        {
            return _value1 + _value2;
        }

        public ActivatorTestClassMultipleParams(int v1, int v2)
        {
            _value1 = v1;
            _value2 = v2;
        }
    }

    public class ActivatorTestClassMultipleCtors
    {
        private int _value1;
        private int _value2;

        public int TestThis()
        {
            return _value1 + _value2;
        }

        public ActivatorTestClassMultipleCtors(int v)
        {
            _value1 = _value2 = v;
        }

        public ActivatorTestClassMultipleCtors(int v1, int v2)
        {
            _value1 = v1;
            _value2 = v2;
        }

        public ActivatorTestClassMultipleCtors()
        {
            _value2 = 1;
            _value1 = 1;
        }
    }
}

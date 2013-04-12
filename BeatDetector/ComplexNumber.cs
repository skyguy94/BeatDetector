namespace BeatDetector
{
    public class ComplexNumber
    {
        public double Real { get; private set; }
        public double Imaginary { get; private set; }

        public static readonly ComplexNumber Zero = new ComplexNumber();

        public ComplexNumber()
        {
            Real = 0d;
            Imaginary = 0d;
        }

        public ComplexNumber(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public ComplexNumber Add(ComplexNumber right)
        {
            Real += right.Real;
            Imaginary += right.Imaginary;

            return this;
        }

        public ComplexNumber Subtract(ComplexNumber right)
        {
            Real -= right.Real;
            Imaginary -= right.Imaginary;

            return this;
        }

        public ComplexNumber Multiply(ComplexNumber right)
        {
            Real = Real * right.Real - Imaginary * right.Imaginary;
            Imaginary = Real * right.Imaginary + Imaginary * right.Real;

            return this;
        }
    }
}

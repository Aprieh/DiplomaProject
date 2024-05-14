public struct ApproxParams
{
    public double Teta;
    public double Alpha;
    public double TBase;
    public double AlphaR;
};
public class TemperatureCalculator
{
    private double L, D, c; //параметры основания радиатора
    private double h, delta, z, b; //параметры ребра для оптимизации
    private double P, t_env, lambda, epsilon; //тепловые параметры
    private double prec; //ограничения
    private double F, S, U, h_1, F_nr, S_cov, Per, S_hs;
    public LinkedList<ApproxParams> Approximations { get; private set; } = new();

    public TemperatureCalculator(
        double h, double delta, double z, // parameters for optimization
        double L, double D, double c, // base parameters of the heatsinkStaticParams
        double P, double t_env, double lambda, double epsilon,
        double theta_0 = 10, double alpha_0 = 10, double alpha_r0 = 10,
        double prec = 0.0001) // acceptable value
    {
        this.h = h;
        this.delta = delta;
        this.z = z;
        this.L = L;
        this.D = D;
        this.c = c;
        this.P = P;
        this.t_env = t_env;
        this.lambda = lambda;
        this.epsilon = epsilon;
        this.prec = prec;

        AdditionalGeometricalParameters(h, delta, z, L, D, c);
        Approximations.AddLast(new ApproxParams { Teta = theta_0, Alpha = alpha_0, TBase = theta_0 + t_env, AlphaR = alpha_r0 });
        CalculateTemperature();
    }
    private static double DefineRibSpacing(double L, double delta, double z)
    {
        return (L - (delta * (z + 1))) / z;
    }
    private static double ThermalCondNoRib(double L, double D, double z, double delta, double t_env, double t_base_0)
    {
        double F_nr = L * D - z * delta * D;
        double t_m = 0.5 * (t_base_0 + t_env);
        double A1 = -0.0012 * t_m + 1.3866;
        double A2 = -0.0034 * t_m + 1.67;
        double powTerm = ((t_base_0 - t_env) <= Math.Pow(0.840 / D, 3)) ? 0.25 : 0.33;
        double alpha_nr = ((t_base_0 - t_env) <= Math.Pow(0.840 / D, 3)) ? A1 * Math.Pow((t_base_0 - t_env) / D, powTerm) : A2 * Math.Pow((t_base_0 - t_env), powTerm);
        return alpha_nr * F_nr;
    }
    private static double ThermalCondLight(double L, double D, double h, double c, double t_base_0, double t_env, double epsilon)
    {
        double t_m = 0.5 * (t_base_0 + t_env);
        double T = t_m + 273;
        double alpha_l = 0.227 * epsilon * Math.Pow(T / 100, 3);
        double S_cov = L * D + 2 * (L + D) * (h + c);
        double sigma_l = alpha_l * S_cov;
        return sigma_l;
    }
    public double GetTemperature()
    {
        return Approximations.Last().TBase;
    }
    private void AdditionalGeometricalParameters(double h, double delta, double z, double L, double D, double c)
    {
        b = DefineRibSpacing(L, delta, z);
        F = L * D;
        S = delta * D;
        U = 2 * (delta + D);
        h_1 = h + S / U;
        F_nr = L * D - z * delta * D;
        S_cov = L * D + 2 * (L + D) * (h + c);
        Per = (2 * h + delta + b) * z + 2 * h + delta + 2 * c;
        S_hs = Per * D;
    }

    public TemperatureCalculator(
    double h, double delta, double z, HeatsinkStaticParameters heatsinkStaticParameters,
    double theta_0 = 10, double alpha_0 = 10, double alpha_r0 = 10,
    double prec = 0.00001) // acceptable value
    {
        this.h = h; this.delta = delta; this.z = z;
        L = heatsinkStaticParameters.Length;
        D = heatsinkStaticParameters.Width;
        c = heatsinkStaticParameters.BaseThickness;
        P = heatsinkStaticParameters.TDP;
        t_env = heatsinkStaticParameters.TemperatureEnvironment;
        lambda = heatsinkStaticParameters.Conductivity;
        epsilon = heatsinkStaticParameters.Emissivity;
        this.prec = prec;
        AdditionalGeometricalParameters(h, delta, z, L, D, c);

        Approximations.AddLast(new ApproxParams { Teta = theta_0, Alpha = alpha_0, TBase = theta_0 + t_env, AlphaR = alpha_r0 });

        CalculateTemperature();
    }

    private double alpha_rFun(double m, double theta_0, double h, double h_1, double t_env, double D)
    {
        double theta_01 = theta_0;
        double theta_h1 = theta_01 * Math.Cosh(m * (h_1 - h)) / Math.Cosh(m * h_1);
        if (Approximations.Count == 1)
            theta_h1 = 0;
        double t_m1 = 0.5 * (theta_h1 + theta_01) + t_env;
        double T_m1 = 0.5 * (t_m1 + t_env) + 273;
        double betta = 1 / T_m1;
        double v = Math.Pow(T_m1, 1.75) / (1.387 * Math.Pow(10, 9));
        double lambda_v = 1.96 * Math.Pow(10, -4) * Math.Pow(T_m1, 0.861);
        double roh = 353 / T_m1;
        double c_r = 0.5 * Math.Pow(10, 3) * Math.Pow(T_m1, 0.121);
        double a = lambda_v / (c_r * roh);
        double Pr1 = v / a;
        double GR1 = (betta * 9.8 * Math.Pow(D, 3) * (t_m1 - t_env)) / (Math.Pow(v, 2));
        double condition = GR1 * Pr1;
        double Nu1;
        if (0.001 < condition && condition <= 500.0)
            Nu1 = 1.18 * Math.Pow(condition, 0.125);
        else if (500.0 < condition && condition <= 20000000.0)
            Nu1 = 0.54 * Math.Pow(condition, 0.25);
        else if (20000000.0 < condition)
            Nu1 = 0.135 * Math.Pow(condition, 0.33);
        else
            Nu1 = 1;
        return Nu1 * lambda_v / D;
    }
    private static double RibCondRatio(double alpha_r, double U, double lambda, double S)
    {
        return Math.Sqrt(alpha_r * U / (lambda * S));
    }
    double ThermalCondRib(double alpha_r, double U, double lambda, double S, double z)
    {
        double m = Math.Sqrt(alpha_r * U / (lambda * S));
        return z * (lambda * m * S) / (1 / Math.Tanh(m * h_1));
    }
    void CalculateApproximation(double h, double U, double delta, double S, double L, double D, double h_1, double z,
    double theta_0, double lambda, double t_env, double t_base_0, double epsilon, double c, double alpha_0, double S_hs)
    {
        double sigma_r1 = ThermalCondRib(alpha_0, U, lambda, S, z);
        double alpha_r1 = alpha_rFun(RibCondRatio(alpha_0, U, lambda, S), theta_0, h, h_1, t_env, D);
        double sigma_nr1 = ThermalCondNoRib(L, D, z, delta, t_env, t_base_0);
        double sigma_l1 = ThermalCondLight(L, D, h, c, t_base_0, t_env, epsilon);
        double sigma = sigma_r1 + sigma_nr1 + sigma_l1;
        double theta_1 = P / sigma;
        double t_base_1 = theta_1 + t_env;
        double alpha1 = sigma / S_hs;
        ApproxParams temp = new() { Teta = theta_1, Alpha = alpha1, TBase = t_base_1, AlphaR = alpha_r1 };
        Approximations.AddLast(temp);
    }
    void CalculateTemperature()
    {
        bool continueCalculation;
        do
        {
            var lastApprox = Approximations.Last();
            CalculateApproximation(h, U, delta, S, L, D, h_1, z, lastApprox.Teta, lambda, t_env, lastApprox.TBase, epsilon, c, lastApprox.AlphaR, S_hs);
            var newApprox = Approximations.Last();
            continueCalculation = Math.Abs(newApprox.Teta - lastApprox.Teta) > prec && Approximations.Count <= 5000;
        } while (continueCalculation);
    }
    public double GetVolume()
    {
        double baseWeight = L * D * c;
        double ribweight = h * delta * D;
        return baseWeight + ribweight * (z + 1);
    }
}

public struct HeatsinkStaticParameters
{
    public double Length { get; set; }
    public double Width { get; set; }
    public double BaseThickness { get; set; }
    public double TDP { get; set; }
    public double TemperatureEnvironment { get; set; }
    public double Conductivity { get; set; }
    public double Emissivity { get; set; }
    public HeatsinkStaticParameters(double length, double width, double baseThickness,
        double tDP, double temperatureEnvironment, double conductivity, double emissivity)
    {
        Length = length;
        Width = width;
        BaseThickness = baseThickness;
        TDP = tDP;
        TemperatureEnvironment = temperatureEnvironment;
        Conductivity = conductivity;
        Emissivity = emissivity;
    }
}
public struct RibParams
{
    public (double Min, double Max) Height;
    public (double Min, double Max) Thickness;
    public (int Min, int Max) Count;
    public bool IsHeightConst, IsThicknessConst, IsCountConst = false;
    public bool AllConst = false;
    public RibParams(double heightMin, double heightMax, double thicknessMin, double thicknessMax, int countMin, int countMax)
    {
        Height.Min = heightMin;
        Height.Max = heightMax;
        Thickness.Min = thicknessMin;
        Thickness.Max = thicknessMax;
        Count.Min = countMin;
        Count.Max = countMax;
        DefineConstParams();
    }
    private void DefineConstParams()
    {
        IsHeightConst = Height.Min == Height.Max;
        IsThicknessConst = Thickness.Min == Thickness.Max;
        IsCountConst = Count.Min == Count.Max;
        AllConst = IsHeightConst && IsThicknessConst && IsCountConst;
    }
}

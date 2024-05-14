using GeneticSharp;

namespace DiplomaProject
{
    public class OptimizationAlgorithm
    {
        private RibParams ribParams;
        private readonly HeatsinkStaticParameters heatsinkStaticParameters;
        private double temperatureLimit;
        SpecialConditions specialConditions;
        public OptimizationResult optimizationResult;
        public OptimizationAlgorithm(HeatsinkStaticParameters heatsinkStaticParameters, RibParams ribParams, double temperatureLimit)
        {
            this.ribParams = ribParams;
            this.heatsinkStaticParameters = heatsinkStaticParameters;
            this.temperatureLimit = temperatureLimit;
            specialConditions = new SpecialConditions(this.ribParams, heatsinkStaticParameters, temperatureLimit);
        }
        public void PerformOptimization()
        {
            ribParams.Count.Min = specialConditions.ribParams.Count.Min;
            ribParams.Count.Max = specialConditions.ribParams.Count.Max;
            if (specialConditions.AllStatic)
            {
                HandleStaticCondition();
            }
            else if (specialConditions.IsTooHighMin || specialConditions.IsTooLowMax)
            {
                CheckBoundaryConditions();
            }
            else
            {
                IFitness fitness = new MyFitness(heatsinkStaticParameters, temperatureLimit);
                IChromosome chromosome = new MyChromosome(specialConditions.ribParams);

                var population = new Population(50, 70, chromosome);
                var selection = new EliteSelection();
                var crossover = new UniformCrossover(0.5f);
                var mutation = new UniformMutation(true);
                var termination = new GenerationNumberTermination(500); // Define a termination condition.

                var geneticAlgorithm = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
                {
                    Termination = termination
                };

                geneticAlgorithm.Start();

                var bestChromosome = geneticAlgorithm.BestChromosome as MyChromosome;
                optimizationResult = new OptimizationResult();
                if (bestChromosome != null)
                {

                    if (bestChromosome.ActiveGenes.Contains(0))
                    {
                        optimizationResult.BestHeight = (double)bestChromosome.GetGene(bestChromosome.ActiveGenes.IndexOf(0)).Value;
                    }
                    else
                    {
                        optimizationResult.BestHeight = bestChromosome.height.Min;
                    }
                    if (bestChromosome.ActiveGenes.Contains(1))
                    {
                        optimizationResult.BestThickness = (double)bestChromosome.GetGene(bestChromosome.ActiveGenes.IndexOf(1)).Value;
                    }
                    else
                    {
                        optimizationResult.BestThickness = bestChromosome.thickness.Min;
                    }
                    if (bestChromosome.ActiveGenes.Contains(2))
                    {
                        optimizationResult.BestCount = Convert.ToInt32(bestChromosome.GetGene(bestChromosome.ActiveGenes.IndexOf(2)).Value); // Ensure correct casting Convert.ToInt32(myChromosome.GetGene(2).Value);
                    }
                    else
                    {
                        optimizationResult.BestCount = bestChromosome.count.Min;
                    }
                    var tempCal = new TemperatureCalculator(optimizationResult.BestHeight, optimizationResult.BestThickness, optimizationResult.BestCount, heatsinkStaticParameters);
                    optimizationResult.ResultingTemperature = tempCal.GetTemperature();
                    optimizationResult.Volume = tempCal.GetVolume();
                    optimizationResult.StatusMessage = "Успешная оптимизация" + ((specialConditions.UnrealisticSpacingText != "") ? specialConditions.UnrealisticSpacingText : "");
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        private void CheckBoundaryConditions()
        {
            if (specialConditions.IsTooHighMin)
            {
                optimizationResult = new OptimizationResult(ribParams.Height.Min, ribParams.Thickness.Min, ribParams.Count.Min,
                    specialConditions.MinTemperature, specialConditions.MinVolume, specialConditions.TooHighMinText + specialConditions.UnrealisticSpacingText);
            }
            else if (specialConditions.IsTooLowMax)
            {
                double ribSpace;
                double ribThickness = ribParams.Thickness.Max;
                do
                {
                    ribSpace = (heatsinkStaticParameters.Length - (ribThickness * (ribParams.Count.Max + 1))) / ribParams.Count.Max;
                    ribThickness = ribThickness - 0.001;
                } while (ribSpace < 0.001 && !(ribThickness < ribParams.Thickness.Min));
                optimizationResult = new OptimizationResult(ribParams.Height.Max, ribThickness, ribParams.Count.Max, specialConditions.MaxTemperature, specialConditions.MaxVolume, specialConditions.TooLowMaxText + specialConditions.UnrealisticSpacingText);
            }
        }

        private void HandleStaticCondition()
        {
            var tempCal = new TemperatureCalculator(ribParams.Height.Min, ribParams.Thickness.Min, ribParams.Count.Min, heatsinkStaticParameters);
            optimizationResult = new OptimizationResult(ribParams.Height.Min,
                ribParams.Thickness.Min, ribParams.Count.Min,
                tempCal.GetTemperature(), tempCal.GetVolume(),
                specialConditions.AllStaticText);
        }

        public class MyFitness : IFitness
        {
            private readonly HeatsinkStaticParameters heatsinkStaticParameters;
            private readonly double temperatureLimit;

            public MyFitness(HeatsinkStaticParameters heatsinkStaticParameters, double temperatureLimit)
            {
                this.heatsinkStaticParameters = heatsinkStaticParameters;
                this.temperatureLimit = temperatureLimit;
            }

            public double Evaluate(IChromosome chromosome)
            {
                var myChromosome = chromosome as MyChromosome;
                if (myChromosome == null)
                {
                    throw new ArgumentException("Chromosome must be of type MyChromosome.", nameof(chromosome));
                }
                double height, thickness;
                int count;
                if (myChromosome.ActiveGenes.Contains(0))
                {
                    height = (double)myChromosome.GetGene(myChromosome.ActiveGenes.IndexOf(0)).Value;
                }
                else
                {
                    height = myChromosome.height.Min;
                }
                if (myChromosome.ActiveGenes.Contains(1))
                {
                    thickness = (double)myChromosome.GetGene(myChromosome.ActiveGenes.IndexOf(1)).Value;
                }
                else
                {
                    thickness = myChromosome.thickness.Min;
                }
                if (myChromosome.ActiveGenes.Contains(2))
                {
                    count = Convert.ToInt32(myChromosome.GetGene(myChromosome.ActiveGenes.IndexOf(2)).Value);
                }
                else
                {
                    count = myChromosome.count.Min;
                }
                double ribSpacing = (heatsinkStaticParameters.Length - (thickness * (count + 1))) / count;
                if (ribSpacing < 0.001)
                {
                    return double.MinValue;
                }
                double temperature = new TemperatureCalculator(height, thickness, count, heatsinkStaticParameters).GetTemperature();
                return 1 / (temperatureLimit - temperature);
            }
        }
        public class MyChromosome : ChromosomeBase
        {
            public (double Min, double Max) height { get; private set; }
            public (double Min, double Max) thickness { get; private set; }
            public (int Min, int Max) count { get; private set; }


            private List<int> activeGenes = [];
            public List<int> ActiveGenes
            {
                get { return activeGenes; }
            }
            public MyChromosome(double heightMin, double heightMax, double thicknessMin, double thicknessMax, int countMin, int countMax)
                : base(CalculateActiveGenes(heightMin, heightMax, thicknessMin, thicknessMax, countMin, countMax))
            {
                height = (heightMin, heightMax);
                thickness = (thicknessMin, thicknessMax);
                count = (countMin, countMax);


                if (height.Min != height.Max) activeGenes.Add(0);// Initialize active genes.
                if (thickness.Min != thickness.Max) activeGenes.Add(1);
                if (count.Min != count.Max) activeGenes.Add(2);

                if (activeGenes.Count == 1) activeGenes.Add(3); //dummy gene

                for (int i = 0; i < activeGenes.Count; i++)// Initialize your genes based on active ones.
                {
                    ReplaceGene(i, GenerateGene(i));
                }

            }
            public MyChromosome(RibParams ribParams)
                : base(CalculateActiveGenes(ribParams))
            {
                height = (ribParams.Height.Min, ribParams.Height.Max);
                thickness = (ribParams.Thickness.Min, ribParams.Thickness.Max);
                count = (ribParams.Count.Min, ribParams.Count.Max);


                if (!ribParams.IsHeightConst) activeGenes.Add(0);// Initialize active genes.
                if (!ribParams.IsThicknessConst) activeGenes.Add(1);
                if (!ribParams.IsCountConst) activeGenes.Add(2);

                if (activeGenes.Count == 1) activeGenes.Add(3); //dummy gene

                for (int i = 0; i < activeGenes.Count; i++)// Initialize your genes based on active ones.
                {
                    ReplaceGene(i, GenerateGene(i));
                }
            }
            private static int CalculateActiveGenes(double heightMin, double heightMax, double thicknessMin, double thicknessMax, int countMin, int countMax)
            {
                int activeCount = 0;
                if (heightMin != heightMax) activeCount++;
                if (thicknessMin != thicknessMax) activeCount++;
                if (countMin != countMax) activeCount++;

                if (activeCount == 1) activeCount++; //dummy gene
                return activeCount;
            }
            private static int CalculateActiveGenes(RibParams ribParams)
            {
                int activeCount = 0;
                if (!ribParams.IsHeightConst) activeCount++;
                if (!ribParams.IsThicknessConst) activeCount++;
                if (!ribParams.IsCountConst) activeCount++;

                if (activeCount == 1) activeCount++; //dummy gene
                return activeCount;
            }

            public override Gene GenerateGene(int geneIndex)
            {
                int index = activeGenes.ElementAt<int>(geneIndex);

                // Check which gene should be generated based on the active genes list.
                double val = 0;
                switch (index)
                {
                    case 0:
                        val = Math.Round(RandomizationProvider.Current.GetDouble(height.Min, height.Max), 3);
                        break;
                    case 1:
                        val = Math.Round(RandomizationProvider.Current.GetDouble(thickness.Min, thickness.Max), 3);
                        break;
                    case 2:
                        val = RandomizationProvider.Current.GetInt(count.Min, count.Max);
                        break;
                    case 3: //dummy gene
                        val = 1;
                        break;
                }
                return new Gene(val);
            }

            public override IChromosome CreateNew()
            {
                return new MyChromosome(height.Min, height.Max, thickness.Min, thickness.Max, count.Min, count.Max);
            }

            public override IChromosome Clone()
            {
                return base.Clone() as IChromosome;
            }
        }
        private class SpecialConditions
        {
            public string AllStaticText { get; }
            public string TooHighMinText { get; }
            public string TooLowMaxText { get; }
            public string UnrealisticSpacingText { get; private set; }
            public bool AllStatic { get; private set; }
            public bool TooHighMin { get; private set; }
            public bool TooLowMax { get; private set; }
            public double MinTemperature { get; private set; }
            public double MinVolume { get; private set; }
            public double MaxVolume { get; private set; }
            public double MaxTemperature { get; private set; }
            private readonly HeatsinkStaticParameters heatsinkStaticParams;
            public RibParams ribParams;
            private readonly double temperatureLimit;
            public bool IsGeometryUnrealisticStaticCase { get; private set; }
            public bool IsGeometryUnrealisticLowerBound { get; private set; }
            public bool IsGeometryUnrealisticUpperBound { get; private set; }
            public SpecialConditions(RibParams ribParams, HeatsinkStaticParameters heatsinkStaticParams, double temperatureLimit)
            {
                this.heatsinkStaticParams = heatsinkStaticParams;
                this.ribParams = ribParams;
                this.temperatureLimit = temperatureLimit;
                AllStaticText = "Все параметры постоянны; оптимизация не требуется";
                TooHighMinText = "Нижняя граница диапазона слишком большая для подбора оптимальных параметров";
                TooLowMaxText = "Верхняя граница диапазона слишком малая для подбора оптимальных параметров";
                UnrealisticSpacingText = "";
                AllStatic = ribParams.AllConst;
                TooHighMin = false;
                TooLowMax = false;
                GeometryRealismCheck();
                MinTemperature = new TemperatureCalculator(this.ribParams.Height.Min, this.ribParams.Thickness.Min, this.ribParams.Count.Min, heatsinkStaticParams).GetTemperature();
                MinVolume = new TemperatureCalculator(this.ribParams.Height.Min, this.ribParams.Thickness.Min, this.ribParams.Count.Min, heatsinkStaticParams).GetVolume();
                MaxTemperature = new TemperatureCalculator(this.ribParams.Height.Max, this.ribParams.Thickness.Max, this.ribParams.Count.Max, heatsinkStaticParams).GetTemperature();
                MaxVolume = new TemperatureCalculator(this.ribParams.Height.Max, this.ribParams.Thickness.Max, this.ribParams.Count.Max, heatsinkStaticParams).GetVolume();
                DefineFlags();

            }
            private void GeometryRealismCheck()
            {
                if (AllStatic)
                {
                    double ribSpace = (heatsinkStaticParams.Length - (ribParams.Thickness.Min * (ribParams.Count.Min + 1))) / ribParams.Count.Min;
                    if (ribSpace < 0.001)
                    {
                        //IsGeometryUnrealisticStaticCase = true;
                        throw new Exception("Недостижимая геометрия! Расстояние между ребрами слишком малое (менее 1 мм).");
                    }
                    else
                        IsGeometryUnrealisticStaticCase = false;
                }
                else
                {
                    double ribSpaceForLowerBound = (heatsinkStaticParams.Length - (ribParams.Thickness.Min * (ribParams.Count.Min + 1))) / ribParams.Count.Min, ribSpaceForUpperBound;
                    if (ribSpaceForLowerBound < 0.001)
                    {
                        if (ribParams.Count.Max == ribParams.Count.Min)
                            throw new Exception("Недостижимая геометрия! Минимально возможное расстояние между ребрами слишком малое (менее 1 мм).");
                        else
                            throw new Exception("Недостижимая геометрия! Минимально возможное расстояние между ребрами нижней границы диапазона слишком малое (менее 1 мм).");
                    }
                    ribSpaceForUpperBound = (heatsinkStaticParams.Length - (ribParams.Thickness.Min * (ribParams.Count.Max + 1))) / ribParams.Count.Max;
                    if (ribSpaceForUpperBound < 0.001)
                    {
                        IsGeometryUnrealisticUpperBound = true;
                    }
                    if (IsGeometryUnrealisticUpperBound == true)
                    {
                        int ribs = ribParams.Count.Max;
                        do
                        {
                            ribSpaceForUpperBound = (heatsinkStaticParams.Length - (ribParams.Thickness.Min * (ribs + 1))) / ribs;
                            ribs--;
                        } while (ribParams.Count.Min <= ribs && ribSpaceForUpperBound < 0.001);
                        if (++ribs < ribParams.Count.Max)
                        {
                            UnrealisticSpacingText = $". Максимально возможное количество ребер при заданных диапазонах: {ribs}";
                            ribParams.Count.Max = ribs;
                        }
                    }
                }
            }
            public void DefineFlags()
            {
                if (!AllStatic)
                {
                    TooHighMin = MinTemperature <= temperatureLimit - 1;
                    TooLowMax = MaxTemperature >= temperatureLimit - 1;
                }
            }
            public bool IsTooHighMin => TooHighMin;
            public bool IsTooLowMax => TooLowMax;
        }
        public class OptimizationResult
        {
            public double BestHeight { get; set; }
            public double BestThickness { get; set; }
            public int BestCount { get; set; }
            public double ResultingTemperature { get; set; }
            public double Volume { get; set; }
            public string StatusMessage { get; set; }

            public OptimizationResult(double height, double thickness, int count, double temperature, double volume, string statusMessage)
            {
                BestHeight = height;
                BestThickness = thickness;
                BestCount = count;
                ResultingTemperature = temperature;
                StatusMessage = statusMessage;
                Volume = volume;
            }
            public OptimizationResult()
            {
                BestHeight = 0;
                BestThickness = 0;
                BestCount = 0;
                ResultingTemperature = 0;
                StatusMessage = "";
                Volume = 0;
            }
        }
    }
}
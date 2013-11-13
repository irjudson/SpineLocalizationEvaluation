using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineLocalizationEvaluate
{
    public class Vertabrae
    {
        public int id { get; set; }
        public String label { get; set; }
        public Decimal x { get; set; }
        public Decimal y { get; set; }
        public Decimal z { get; set; }

        public Vertabrae()
        {
            x = 0.0M;
            y = 0.0M;
            z = 0.0M;
        }

        public override string ToString()
        {
            return this.id + " - " + this.label + " : " + this.x + ", " + this.y + ", " + this.z;
        }

        public static Vertabrae operator -(Vertabrae v1, Vertabrae v2)
        {
            Vertabrae v = new Vertabrae();
            if (v1 == null && v2 == null)
            {
                return v;
            }
            else if (v1 == null && v2 != null)
            {
                v = v2;
            }
            else if (v1 != null && v2 == null)
            {
                v = v1;
            }
            else if ((v1.id == v2.id) & (v1.label == v2.label))
            {
                v.id = v1.id;
                v.label = v1.label;
                v.x = v1.x - v2.x;
                v.y = v1.y - v2.y;
                v.z = v1.z - v2.z;
            }
            return v;
        }

        public void Square()
        {
            this.x = this.x * this.x;
            this.y = this.y * this.y;
            this.z = this.z * this.z;
        }

        public Decimal ComputeSum()
        {
            return this.x + this.y + this.z;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int NumberOfVertabrae = 26;
            String referenceDirectory = null;
            String submissionDirectory = null;
            String outputDirectory = null;
            String line = null;
            List<String> ids = new List<String>();
            Dictionary<String, Vertabrae[]> reference = new Dictionary<String, Vertabrae[]>();
            Dictionary<String, Vertabrae[]> submission = new Dictionary<String, Vertabrae[]>();
            Vertabrae[] record = null;
            Decimal[, ,] results = null;
            Decimal[,] summary = null;

            // Deal with arguments
            if (args.Length == 2)
            {
                referenceDirectory = Path.Combine(args[0], "ref");
                submissionDirectory = Path.Combine(args[0], "res");
                outputDirectory = args[1];
            }
            else
            {
                Console.WriteLine("Not enough arguments to execute evaluation.");
                Environment.Exit(1);
            }

            // Check on directories
            if (!Directory.Exists(referenceDirectory))
            {
                Console.WriteLine("Reference directory does not exist.");
                Environment.Exit(1);
            }
            else
            {
                if (!Directory.Exists(Path.Combine(referenceDirectory, "labels")))
                {
                    Console.WriteLine("Reference directory structure is missing labels directory.");
                    Environment.Exit(1);
                }
                if (!File.Exists(Path.Combine(referenceDirectory, "ids.txt")))
                {
                    Console.WriteLine("Reference directory structure is missing ids.txt file.");
                    Environment.Exit(1);
                }
            }

            if (!Directory.Exists(submissionDirectory))
            {
                Console.WriteLine("Reference directory does not exist.");
                Environment.Exit(1);
            }

            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine("Output directory does not exist.");
                Environment.Exit(1);
            }

            System.IO.StreamReader idsFile = new System.IO.StreamReader(Path.Combine(referenceDirectory, "ids.txt"));
            while ((line = idsFile.ReadLine()) != null)
            {
                ids.Add(line);
            }

            results = new Decimal[ids.Count, NumberOfVertabrae, 5];

            foreach (String id in ids)
            {
                String filename = Path.Combine(referenceDirectory, "labels", id + ".lml");
                if (File.Exists(filename))
                {
                    String[] values;
                    System.IO.StreamReader referenceFile = new System.IO.StreamReader(filename);
                    record = new Vertabrae[NumberOfVertabrae];
                    while ((line = referenceFile.ReadLine()) != null)
                    {
                        values = line.Split();
                        if (values.Length == 5)
                        {
                            Vertabrae vertabra = new Vertabrae();
                            vertabra.id = Convert.ToInt16(values[0]) / 10;
                            vertabra.label = values[1];
                            vertabra.x = Convert.ToDecimal(values[2]);
                            vertabra.y = Convert.ToDecimal(values[3]);
                            vertabra.z = Convert.ToDecimal(values[4]);
                            record[vertabra.id - 1] = vertabra;
                        }
                        else
                        {
                            Console.WriteLine("Skipping short line in reference file.");
                            Console.WriteLine("\tLine: " + line);
                        }
                    }
                    reference.Add(id, record);
                }
                else
                {
                    Console.WriteLine("Missing reference file: " + id + ".lml");
                    Environment.Exit(1);
                }

                filename = Path.Combine(submissionDirectory, id + ".lml");
                if (File.Exists(Path.Combine(submissionDirectory, id + ".lml")))
                {
                    String[] values;
                    System.IO.StreamReader submissionFile = new System.IO.StreamReader(filename);
                    record = new Vertabrae[NumberOfVertabrae];
                    while ((line = submissionFile.ReadLine()) != null)
                    {
                        values = line.Split();
                        if (values.Length == 5)
                        {
                            Vertabrae vertabra = new Vertabrae();
                            vertabra.id = Convert.ToInt16(values[0]) / 10;
                            vertabra.label = values[1];
                            vertabra.x = Convert.ToDecimal(values[2]);
                            vertabra.y = Convert.ToDecimal(values[3]);
                            vertabra.z = Convert.ToDecimal(values[4]);
                            record[vertabra.id - 1] = vertabra;
                        }
                        else
                        {
                            Console.WriteLine("Skipping short line in submission file.");
                            Console.WriteLine("\tLine: " + line);
                        }
                    }
                    submission.Add(id, record);
                }
                else
                {
                    Console.WriteLine("Missing submission file: " + id + ".lml");
                    Environment.Exit(1);
                }
            }

            // Calculate error
            foreach (String id in ids)
            {
                int idx = ids.IndexOf(id);

                for (int i = 0; i < NumberOfVertabrae; i++)
                {
                    results[idx, i, 0] = 0.000M;
                    Vertabrae difference = reference[id][i] - submission[id][i];
                    difference.Square();
                    results[idx, i, 0] = Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(difference.ComputeSum())));
                    results[idx, i, 1] = (reference[id][i] != null && submission[id][i] != null) ? 1 : 0; // True Positive 
                    results[idx, i, 2] = (reference[id][i] == null && submission[id][i] == null) ? 1 : 0; // True Negative
                    results[idx, i, 3] = (reference[id][i] == null && submission[id][i] != null) ? 1 : 0; // False Positive
                    results[idx, i, 4] = (reference[id][i] != null && submission[id][i] == null) ? 1 : 0; // False Negative
                }

                System.IO.StreamWriter sliceOut = new System.IO.StreamWriter(Path.Combine(outputDirectory, id + ".txt"));
                for (int j = 0; j < 5; j++)
                {
                    for (int k = 0; k < NumberOfVertabrae; k++)
                    {
                        if (j == 0)
                        {
                            sliceOut.Write("\t"+results[idx, k, j].ToString("0.000"));
                        }
                        else
                        {
                            sliceOut.Write("\t"+results[idx, k, j]);
                        }
                    }
                    sliceOut.WriteLine();
                }

                for (int j = 0; j < NumberOfVertabrae; j++)
                {
                    if (reference[id][j] == null)
                    {
                        sliceOut.Write("\t0");
                    }
                    else
                    {
                        sliceOut.Write("\t1");

                    }
                }
                sliceOut.WriteLine();
                for (int j = 0; j < NumberOfVertabrae; j++)
                {
                    if (submission[id][j] == null)
                    {
                        sliceOut.Write("\t0");
                    }
                    else
                    {
                        sliceOut.Write("\t1");

                    }
                }
                    sliceOut.Close();
            }

            summary = new Decimal[NumberOfVertabrae, 6];
            Decimal meanError = 0.0M;
            Decimal errorsFound = 0;
            Decimal truePositivesFound = 0;
            Decimal trueNegativesFound = 0;
            Decimal falsePositivesFound = 0;
            Decimal falseNegativesFound = 0;
            for (int i = 0; i < NumberOfVertabrae; i++)
            {
                foreach (String id in ids)
                {
                    int idx = ids.IndexOf(id);
                    summary[i, 0] += results[idx, i, 0];
                    summary[i, 1] += results[idx, i, 1];
                    truePositivesFound += results[idx, i, 1];
                    summary[i, 2] += results[idx, i, 2];
                    trueNegativesFound += results[idx, i, 2];
                    summary[i, 3] += results[idx, i, 3];
                    falsePositivesFound += results[idx, i, 3];
                    summary[i, 4] += results[idx, i, 4];
                    falseNegativesFound += results[idx, i, 4];
                }
            }

            for (int i = 0; i < NumberOfVertabrae; i++)
            {
                if (summary[i, 1] > 0)
                {
                    summary[i, 5] = summary[i, 0] / summary[i, 1];
                    meanError += summary[i, 5];
                    errorsFound += 1;
                }
                else
                {
                    summary[i, 5] = 0.000M;
                }
            }
            System.IO.StreamWriter scoresOut = new System.IO.StreamWriter(Path.Combine(outputDirectory, "scores.txt"));
            Decimal errorReported = (meanError/errorsFound);
            scoresOut.WriteLine("Mean Error: " + errorReported.ToString("0.000")); 
            scoresOut.WriteLine("False Positives Found: " + falsePositivesFound);
            scoresOut.WriteLine("False Negatives Found: " + falseNegativesFound);
            scoresOut.Close();

            Console.WriteLine("Finished evaluating submission.");
            Environment.Exit(0);
        }
    }
}

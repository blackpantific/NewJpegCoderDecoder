using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJpegCoderDecoder
{
    public partial class Program
    {
        public static Dictionary<byte, double> AlphabetAndPropabilitiesHuffman { get; set; } = new Dictionary<byte, double>();
        public static List<double> SortedPropabilitiesHuffman { get; set; } = new List<double>();
        public static List<byte> AlphabetLettersSortedByPropabilitiesHuffman { get; set; } = new List<byte>();
        public static List<List<byte>> C { get; set; }//матрица код слов
        public static List<byte> L { get; set; }//матрица длин код слов

        public static List<byte> HuffmanTextCoder(List<byte> byteArray)//преобразует польз текст в текст Хаффмана
        {
            List<byte> outputList = new List<byte>();
            for (int i = 0; i < byteArray.Count; i++)
            {
                var c = AlphabetLettersSortedByPropabilitiesHuffman.IndexOf(byteArray[i]);
                var d = C[c];
                outputList.AddRange(d);
            }


            return outputList;
        }
        public static void HuffmanCodeBuilder(List<double> sortedProbabilities)//получает отсорт вер-ти и выдает готовые коды на каждый символ
        {
            List<double> P = new List<double>(sortedProbabilities);//вероятности
            List<List<byte>> T = new List<List<byte>>();//потомки узлов
            C = new List<List<byte>>();//кодовые слова
            L = new List<byte>();//длины кодовых слов

            for (int i = 0; i < P.Count; i++)
            {
                T.Add(new List<byte>());//инициализация строки в матрице T
                C.Add(new List<byte>());//инициализация строки в матрице C
                L.Add(0);//инициализация строки в матрице (матрица одномерна)
                T[i].Add(Convert.ToByte(i));
            }

            bool endBuilding = true;//пока в матрице P не останется единственный элемент 
            Func<double, bool> func = (arg) => arg != 0;
            double p_iValue = 0;
            double p_jValue = 0;
            int p_iPosition = 0;
            int p_jPosition = 0;
            while (endBuilding)
            {



                p_iValue = P.Where(func).Min();
                p_iPosition = P.IndexOf(p_iValue);
                P[p_iPosition] = 0;

                try
                {
                    p_jValue = P.Where(func).Min();
                    p_jPosition = P.IndexOf(p_jValue);
                    P[p_jPosition] = 0;
                }
                catch                //мы закончили создание код слов, так как остался 1 элемент и все отортированы
                {
                    endBuilding = false;
                    break;
                }
                if (p_iPosition < p_jPosition)//все ок, по алгоритму 
                {
                    P[p_iPosition] = p_iValue + p_jValue;
                    foreach (var item in T[p_iPosition])
                    {
                        C[item].Add(0);
                        L[item] += 1;
                    }
                    foreach (var item in T[p_jPosition])
                    {
                        T[p_iPosition].Add(item);
                        C[item].Add(1);
                        L[item] += 1;
                    }
                    T[p_jPosition].Clear();



                }
                else//меняем местами
                {
                    P[p_jPosition] = p_iValue + p_jValue;
                    foreach (var item in T[p_jPosition])
                    {
                        C[item].Add(0);
                        L[item] += 1;
                    }
                    foreach (var item in T[p_iPosition])
                    {
                        T[p_jPosition].Add(item);
                        C[item].Add(1);
                        L[item] += 1;
                    }
                    T[p_iPosition].Clear();
                }




            }

            for (int i = 0; i < C.Count; i++)
            {
                C[i].Reverse();
            }
            //реверс матрицы слов

        }
        public static void GettingAlphabetAndPropabilities(List<byte> byteArray/*List<byte> Alphabet, List<double> AlphabetPropabilities*/)
        {
            var numberOfSymbolsInArray = byteArray.Count;
            double countSymbols;//сколько раз встречается тот или иной элемент
            double propability;
            for (int i = 0; i < numberOfSymbolsInArray; i++)
            {
                if (AlphabetAndPropabilitiesHuffman.ContainsKey(byteArray[i]))
                {
                    continue;
                }
                else
                {
                    countSymbols = byteArray.Where(x => x.Equals(byteArray[i])).Count();
                    propability = countSymbols / numberOfSymbolsInArray;
                    AlphabetAndPropabilitiesHuffman.Add(byteArray[i], propability);
                }
            }


            AlphabetAndPropabilitiesHuffman = AlphabetAndPropabilitiesHuffman.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);//сортировка по вероятностям

        }
        public static List<byte> GettingHuffmanWordsFromData(List<byte> byteArray, int alphabetSize, List<byte> LenghtOfWords)
        {
            int index = 0;
            C = new List<List<byte>>();
            for (int i = 0; i < alphabetSize; i++)
            {
                C.Add(new List<byte>());
                C[i].AddRange(byteArray.GetRange(index, LenghtOfWords[i]));
                index += LenghtOfWords[i];
            }

            byteArray.RemoveRange(0, index);
            return byteArray;
        }
        public static List<byte> GettingDataFromHuffmanWords(List<byte> byteArray, int numberOfWords)
        {
            List<byte> ListToMoveToFront = new List<byte>();
            int iterator = 0;

            List<byte> newWord = new List<byte>();
            for (int i = 0; i < byteArray.Count; i++)
            {
                if (iterator != numberOfWords)
                {
                    newWord.Add(byteArray[i]);
                    for (int j = 0; j < C.Count; j++)
                    {
                        if (newWord.SequenceEqual(C[j]))
                        {
                            ListToMoveToFront.Add(AlphabetLettersSortedByPropabilitiesHuffman[j]);
                            iterator += 1;
                            newWord.Clear();
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }

            }

            return ListToMoveToFront;
        }
    }
}

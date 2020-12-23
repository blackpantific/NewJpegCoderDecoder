using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJpegCoderDecoder
{
    public partial class Program
    {
        public static int iteratorArithm { get; set; } = 0;

        public static List<int[]> arrayOfCoef { get; set; } = new List<int[]>();

        public static int Width { get; set; } = 0;
        public static int Height { get; set; } = 0;
        public static int Comp { get; set; } = 0;




        static void Main(string[] args)
        {
            byte[] buffer;
            FileStream fileStream;

            if (args[4] == "1")
            {

                using (fileStream = File.OpenRead(args[1]))
                {
                    var fileSize = fileStream.Length;
                    buffer = new byte[fileSize];

                    fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));

                }
                NanoJPEG nanoJPEG = new NanoJPEG();

                arrayOfCoef = nanoJPEG.njDecode(buffer);

                List<short> vs = new List<short>();
                for (int i = 0; i < arrayOfCoef.Count; i++)
                {
                    for (int j = 0; j < arrayOfCoef[i].Length; j++)
                    {
                        vs.Add((short)arrayOfCoef[i][j]);
                    }
                }

                List<byte> lb = new List<byte>();

                lb.AddRange(BitConverter.GetBytes(Width));//ширина файла
                lb.AddRange(BitConverter.GetBytes(Height));//высота файла
                lb.AddRange(BitConverter.GetBytes(Comp));//Comp файла

                lb.AddRange(vs.SelectMany(BitConverter.GetBytes).ToArray());

                var listAfterArithmeticCoding = ArithmeticCoding(lb, Convert.ToInt32(args[3]));//10 самый лучшее сжатие

                var afterHuffmanCoding = HuffmanCoding(listAfterArithmeticCoding);

                using (FileStream fs = File.Create(args[2]))
                {
                    fs.Write(afterHuffmanCoding.ToArray(), 0, afterHuffmanCoding.Count);
                }
            }
            else
            {
                ///////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////

                using (fileStream = File.OpenRead(args[2]))
                {
                    var fileSize = fileStream.Length;
                    buffer = new byte[fileSize];

                    fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));

                }
                var afterHuffmanCoding = new List<byte>(buffer);

                var afterHuffmaDecoding = HuffmanDecoding(afterHuffmanCoding);

                var listAfterArithmeticDecoding = ArithmeticDecoding(afterHuffmaDecoding);

                Width = BitConverter.ToInt32(listAfterArithmeticDecoding.GetRange(0, 4).ToArray(), 0);//позаменять lb
                Height = BitConverter.ToInt32(listAfterArithmeticDecoding.GetRange(4, 4).ToArray(), 0);
                Comp = BitConverter.ToInt32(listAfterArithmeticDecoding.GetRange(8, 4).ToArray(), 0);
                listAfterArithmeticDecoding.RemoveRange(0, 12);

                var ListOFMatrix = new List<byte>(listAfterArithmeticDecoding);
                List<int> intList = new List<int>();
                List<int[]> listArraysOfInt = new List<int[]>();

                for (int l = 0; l < ListOFMatrix.Count - 1; l++)
                {
                    intList.Add(Convert.ToInt32(BitConverter.ToInt16(ListOFMatrix.GetRange(l, 2).ToArray(), 0)));
                    l += 1;
                }

                for (int p = 0; p < intList.Count; p += 64)
                {
                    listArraysOfInt.Add(intList.GetRange(p, 64).ToArray());
                }




                jo_write_jpg(args[1], listArraysOfInt, Width, Height, Comp, 20);


            }

        }


        public static List<byte> HuffmanCoding(List<byte> OutputFileList)
        {

            List<byte> BufferAsList;
            
            BufferAsList = new List<byte>(OutputFileList);

            GettingAlphabetAndPropabilities(BufferAsList);

            SortedPropabilitiesHuffman = AlphabetAndPropabilitiesHuffman.Select(d => d.Value).ToList();//сортируем вероятности для построения кода Хаффмана
            AlphabetLettersSortedByPropabilitiesHuffman = AlphabetAndPropabilitiesHuffman.Select(l => l.Key).ToList();


            HuffmanCodeBuilder(SortedPropabilitiesHuffman);//передаем список сортированых вероятностей из словаря

            var InformationAboutText = new List<byte>();//вся инфа о кодах Хаффмана для декодера

            InformationAboutText.Add((byte)(AlphabetAndPropabilitiesHuffman.Count - 1));//количество символов в алфавите
                                                                                        //потому что мы отнимаем 1 при передаче, для того чтобы если алфавит 256, то число уместилось в 1 байт

            InformationAboutText.InsertRange(1, BitConverter.GetBytes(BufferAsList.Count));//кол-во символов в тексте

            var alphabetLetters = AlphabetAndPropabilitiesHuffman.Select(d => d.Key).ToList();
            InformationAboutText.InsertRange(5, alphabetLetters);//передаем алфавит в последовательносте убывания вероятностей каждой буквы

            InformationAboutText.InsertRange(5 + alphabetLetters.Count, L);//вставляем длины каждого слова Хаффмана к каждой букве

            List<byte> ListOfAllHuffmanWords = new List<byte>();//Все слова Хаффмана по убыванию вероятностей в списке
            for (int i = 0; i < C.Count; i++)
            {
                ListOfAllHuffmanWords.AddRange(C[i]);
            }

            var codeText = HuffmanTextCoder(BufferAsList);

            ListOfAllHuffmanWords.AddRange(codeText);//склеиваем кодовые слова и текст из кодовых слов

            var wordsArrayAfterConvertion = ListOfAllHuffmanWords.ToBitArray(ListOfAllHuffmanWords.Count);
            var byteArrayWords = wordsArrayAfterConvertion.BitArrayToByteArray();//переработанный массив из слов алфавита и текста


            InformationAboutText.AddRange(byteArrayWords);//полный текст на запись в файл

            return InformationAboutText;
        }

        public static List<byte> HuffmanDecoding(List<byte> OutputFileList)
        {
            FileStream fileStream;
            List<byte> HuffmanWordsWithData;
            List<byte> HuffmanDataInBitsList;


            int alphabetSize = (int)OutputFileList[0];//размер алфавита
            alphabetSize += 1;//потому что мы отнимаем 1 при передаче, для того чтобы если алфавит 256, то число уместилось в 1 байт
            var userTextLenght = BitConverter.ToInt32(OutputFileList.GetRange(1, 4).ToArray(), 0);//количество букв в тексте

            AlphabetLettersSortedByPropabilitiesHuffman = new List<byte>(OutputFileList.GetRange(5, alphabetSize));//получаем алфавит в упорядоченном виде(по убыв вероятностей)
            L = new List<byte>(OutputFileList.GetRange((5 + alphabetSize), alphabetSize));//считываем длины код слов
            var header = alphabetSize * 2 + 5;//размер заголовка

            HuffmanWordsWithData = OutputFileList.GetRange(header, OutputFileList.Count - header);
            HuffmanDataInBitsList = HuffmanWordsWithData.ByteListToBitList();

            var textToDecode = GettingHuffmanWordsFromData(HuffmanDataInBitsList, alphabetSize, L);//кодовые слова вставляет в матрицу код слов, возвращает только текст для декодирования без алфавита в начале

            var textToMoveToFront = GettingDataFromHuffmanWords(textToDecode, userTextLenght);

            return textToMoveToFront;
        }

        public static List<byte> ArithmeticDecoding(List<byte> OutputFileList)
        {
           

            var numberOfAlphabetSymbols = OutputFileList[0] + 1;//количество символов в алфавите
            var sizeOfBlock = OutputFileList[1] + 1;//размер суперсимвола
            var numberSupersymbols = BitConverter.ToUInt32(OutputFileList.GetRange(2, 4).ToArray(), 0);//количество букв(суперсимволов) в алфавите
            var numberOfBlocksToDecode = BitConverter.ToUInt32(OutputFileList.GetRange(6, 4).ToArray(), 0);//количество всего блоков для декодирования
            var numbersOfNulls = OutputFileList[10];//количество нулей в последнем блоке

            OutputFileList.RemoveRange(0, 11);

            AlphabetAndPropabilities = new Dictionary<byte, double>();
            int j = 0;
            for (int i = 0; i < numberOfAlphabetSymbols; i++)
            {

                var block = OutputFileList.GetRange(j, 9);
                AlphabetAndPropabilities.Add(block[0], BitConverter.ToDouble(block.GetRange(1, 8).ToArray(), 0));
                j += (8 + 1);

            }

            OutputFileList.RemoveRange(0, j);

            for (int i = 0; i < numberSupersymbols; i++)
            {
                LenghtOfWords.Add(OutputFileList[i]);
            }
            OutputFileList.RemoveRange(0, (int)numberSupersymbols);

            OutputFileList = new List<byte>(OutputFileList.ByteListToBitList());

            var textToDecode = GettingArithmeticWordsFromData(OutputFileList, numberSupersymbols, LenghtOfWords);

            //AlphabetAndPropabilities.Add(1, 0.1);
            //AlphabetAndPropabilities.Add(2, 0.6);
            //AlphabetAndPropabilities.Add(3, 0.3);

            GettingCumulativeProbability(AlphabetAndPropabilities);//снова получаем вероятности для нахождения букв исходного текста


            List<byte> inputText = GettingDataFromArithmeticWords(textToDecode, numberOfBlocksToDecode, sizeOfBlock);



            //List<byte> inputText1 = NewGettingDataFromArithmeticWords(textToDecode, numberOfBlocksToDecode, sizeOfBlock);
            //работает хуже,ем предыд метод



            inputText.RemoveRange(inputText.Count - numbersOfNulls, numbersOfNulls);

            return inputText;
        }

        public static List<byte> ArithmeticCoding(List<byte> OutputFileList, int sizeOfBlock)
        {

            ArithmeticCodeBuilder(OutputFileList, sizeOfBlock);
            var Output = new List<byte>();


            Output.Add((byte)(AlphabetAndPropabilities.Count - 1));//количество символов в алфавите -1 чтобы не было ошибки

            Output.Add(Convert.ToByte(sizeOfBlock - 1));//добавляем размер блока, теоретически от 2 до 255


            uint numOfSupersymbols = (uint)BlocksAndTheirCodeWords.Count;
            Output.AddRange(BitConverter.GetBytes(numOfSupersymbols));//количество суперсимволов(букв алфавита)

            Output.AddRange(BitConverter.GetBytes(NumberOfBlocksToDecode));//количество суперсимволов в тексте, сколько нужно декодировать

            Output.Add(NumberOfNullsInLastBlock);//добавляем количество нулей в последнем блоке


            for (int i = 0; i < AlphabetProbCumulativeProb.Count; i++)
            {
                Output.Add(AlphabetProbCumulativeProb.ElementAt(i).Key);//добавляем символ алфавита
                Output.AddRange(BitConverter.GetBytes(AlphabetProbCumulativeProb.ElementAt(i).Value.Item1));//вероятность символа
            }
            //var d = BitConverter.ToDouble(array.GetRange(1,8).ToArray(),0);
            var symbLenght = 0;
            for (int i = 0; i < BlocksAndTheirCodeWords.Count; i++)
            {
                symbLenght = BlocksAndTheirCodeWords.ElementAt(i).Value.Item2.Count;
                if (symbLenght < 256)
                {
                    Output.Add((byte)symbLenght);
                }
                else
                {
                    //error
                }
            }


            var listOfAllArithmeticWords = new List<byte>();//склеиваем все кодовые слова вместе
            for (int i = 0; i < BlocksAndTheirCodeWords.Count; i++)//добавляем все кодовые слов
            {
                listOfAllArithmeticWords.AddRange(BlocksAndTheirCodeWords.ElementAt(i).Value.Item2);
            }

            listOfAllArithmeticWords.AddRange(ListBinaryToTransform);//склеиваем кодовые слова и текст из кодовых слов для трансформации в битовый массив

            var wordsAfterConvertion = listOfAllArithmeticWords.ToBitArray(listOfAllArithmeticWords.Count);//трансформируем в BitArray
            var byteArrayWords = wordsAfterConvertion.BitArrayToByteArray();//BitArray в массив байт

            //var reverse = byteArrayWords.ByteArrayToBitList();

            Output.AddRange(byteArrayWords);

            return Output;
        }
    }
}
